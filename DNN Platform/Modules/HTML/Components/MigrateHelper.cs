// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Components;

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Data;
using DotNetNuke.Modules.Html.Models;

/// <summary>
/// Provides helper methods for migrating HTML workflows and related database schema changes.
/// </summary>
public class MigrateHelper
{
    /// <summary>
    /// Executes the migration of HTML workflows, including running the migration procedure,
    /// dropping obsolete tables and procedures, and adding required foreign keys.
    /// </summary>
    public static void MigrateHtmlWorkflows()
    {
        var db = Data.DataProvider.Instance();
        var databaseOwner = db.DatabaseOwner;
        var objectQualifier = db.ObjectQualifier;

        var localizationHelper = new LocalizationHelper();

        // "Published,veröffentlicht,Publicado,Publié,Pubblicato,Publiceren"
        var publishedNames = new HashSet<string>(
            localizationHelper.StateLocalizations("DefaultWorkflowState3.StateName").Select(Normalize),
            StringComparer.OrdinalIgnoreCase);

        // "Draft,Entwurf,Borrador,Brouillon,Bozza,Concept"
        var draftNames = new HashSet<string>(
            localizationHelper.StateLocalizations("DefaultWorkflowState1.StateName").Select(Normalize),
            StringComparer.OrdinalIgnoreCase);

        // 1. Execute the migration logic in-code (replaces the stored procedure call)
        RunHtmlWorkflowMigration(databaseOwner, objectQualifier, publishedNames, draftNames);

        // 2. Add FK_HtmlText_WorkflowStates if it does not exist
        db.ExecuteSQL($@"
            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_{objectQualifier}HtmlText_{objectQualifier}WorkflowStates') AND parent_object_id = OBJECT_ID(N'{databaseOwner}{objectQualifier}HtmlText'))
                ALTER TABLE {databaseOwner}{objectQualifier}HtmlText WITH NOCHECK ADD CONSTRAINT FK_{objectQualifier}HtmlText_{objectQualifier}WorkflowStates FOREIGN KEY (StateID) REFERENCES {databaseOwner}{objectQualifier}ContentWorkflowStates (StateID);
            ");

        // 3. Add FK_HtmlTextLog_WorkflowStates if it does not exist
        db.ExecuteSQL($@"
            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_{objectQualifier}HtmlTextLog_{objectQualifier}WorkflowStates') AND parent_object_id = OBJECT_ID(N'{databaseOwner}{objectQualifier}HtmlTextLog'))
                ALTER TABLE {databaseOwner}{objectQualifier}HtmlTextLog WITH NOCHECK ADD CONSTRAINT FK_{objectQualifier}HtmlTextLog_{objectQualifier}WorkflowStates FOREIGN KEY (StateID) REFERENCES {databaseOwner}{objectQualifier}ContentWorkflowStates (StateID);
            ");

        // 4. Enable HtmlText constraints after checking existing data
        db.ExecuteSQL($@"
            ALTER TABLE {databaseOwner}{objectQualifier}HtmlText WITH CHECK CHECK CONSTRAINT FK_{objectQualifier}HtmlText_{objectQualifier}WorkflowStates;
            ");

        // 5. Enable HtmlTextLog constraints after checking existing data
        db.ExecuteSQL($@"
            ALTER TABLE {databaseOwner}{objectQualifier}HtmlTextLog WITH CHECK CHECK CONSTRAINT FK_{objectQualifier}HtmlTextLog_{objectQualifier}WorkflowStates;
            ");

        // 7. Drop WorkflowStatePermission table if it exists
        db.ExecuteSQL($@"
            IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'{databaseOwner}{objectQualifier}WorkflowStatePermission') AND OBJECTPROPERTY(id, N'IsTable') = 1)
                DROP TABLE {databaseOwner}{objectQualifier}WorkflowStatePermission;
            ");

        // 8. Drop WorkflowStates table if it exists
        db.ExecuteSQL($@"
            IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{databaseOwner}{objectQualifier}WorkflowStates') AND type in (N'U'))
                DROP TABLE {databaseOwner}{objectQualifier}WorkflowStates;
            ");

        // 9. Drop Workflow table if it exists
        db.ExecuteSQL($@"
            IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{databaseOwner}{objectQualifier}Workflow') AND type in (N'U'))
                DROP TABLE {databaseOwner}{objectQualifier}Workflow;
            ");
    }

    /// <summary>
    /// Migrates HtmlText and HtmlTextLog StateID references from legacy Workflow/WorkflowStates
    /// to the new ContentWorkflows/ContentWorkflowStates, using localized Draft/Published names.
    /// Uses DAL 2 repositories and an in-memory mapping; no inline SQL.
    /// </summary>
    private static void RunHtmlWorkflowMigration(string databaseOwner, string objectQualifier, ISet<string> publishedNames, ISet<string> draftNames)
    {
        using (var ctx = DataContext.Instance())
        {
            ctx.BeginTransaction();

            // Repositories (DAL2)
            var repModules = ctx.GetRepository<ModuleRow>();
            var repHtmlText = ctx.GetRepository<HtmlTextRow>();
            var repHtmlTextLog = ctx.GetRepository<HtmlTextLogRow>();
            var repLegacyWorkflow = ctx.GetRepository<LegacyWorkflowRow>();
            var repLegacyWorkflowState = ctx.GetRepository<LegacyWorkflowStateRow>();
            var repContentWorkflow = ctx.GetRepository<ContentWorkflowRow>();
            var repContentWorkflowState = ctx.GetRepository<ContentWorkflowStateRow>();

            // Load needed data in memory
            var modules = repModules.Get().ToDictionary(m => m.ModuleID, m => m);
            var itemIdToModuleId = repHtmlText.Get().ToDictionary(h => h.ItemID, h => h.ModuleID);

            var legacyWorkflows = repLegacyWorkflow.Get().ToList();
            var legacyWorkflowStates = repLegacyWorkflowState.Get().ToList();
            var legacyWorkflowById = legacyWorkflows.ToDictionary(w => w.WorkflowID, w => w);
            var legacyStateById = legacyWorkflowStates.ToDictionary(s => s.StateID, s => s);

            // Only map legacy workflows we know how to translate
            var legacyNameToKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Direct Publish", "DirectPublish" },
                    { "Content Staging", "SaveDraft" },
                };

            var contentWorkflows = repContentWorkflow.Get().ToList();
            var contentWorkflowStates = repContentWorkflowState.Get().ToList();

            // Index content workflows by portal and key
            var cwByPortalAndKey = contentWorkflows
                .GroupBy(cw => (PortalID: cw.PortalID ?? -1, Key: cw.WorkflowKey))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Index content states by workflowId
            var cwsByWorkflow = contentWorkflowStates
                .GroupBy(s => s.WorkflowID)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Build mapping: (portalId, oldStateId) -> newStateId
            var stateMapping = new Dictionary<(int PortalId, int OldStateId), int>();

            foreach (var ws in legacyWorkflowStates)
            {
                if (!legacyWorkflowById.TryGetValue(ws.WorkflowID, out var w))
                {
                    continue;
                }

                if (!legacyNameToKey.TryGetValue(w.WorkflowName ?? string.Empty, out var expectedKey))
                {
                    // Skip legacy workflows we do not translate
                    continue;
                }

                // Determine target portals:
                // - If legacy workflow is portal-specific: just that portal
                // - If global (null): all portals that have a content workflow for that key
                IEnumerable<int> targetPortals;

                if (w.PortalID.HasValue)
                {
                    targetPortals = new[] { w.PortalID.Value };
                }
                else
                {
                    targetPortals = contentWorkflows
                        .Where(cw => cw.WorkflowKey == expectedKey && cw.PortalID.HasValue)
                        .Select(cw => cw.PortalID.Value)
                        .Distinct()
                        .ToArray();
                }

                foreach (var portalId in targetPortals)
                {
                    if (!cwByPortalAndKey.TryGetValue((portalId, expectedKey), out var cwsList) || cwsList.Count == 0)
                    {
                        continue;
                    }

                    // Prefer first workflow for that portal/key (there should be one)
                    var targetCw = cwsList[0];

                    if (!cwsByWorkflow.TryGetValue(targetCw.WorkflowID, out var targetStates) || targetStates.Count == 0)
                    {
                        continue;
                    }

                    // Match target state by name or localized alias logic
                    var match = FindMatchingNewState(targetStates, ws.StateName, publishedNames, draftNames);
                    if (match != null)
                    {
                        stateMapping[(portalId, ws.StateID)] = match.StateID;
                    }
                }
            }

            // Helper: find portal's direct publish published state for fallback
            int? GetFallbackPublishedStateId(int portalId)
            {
                if (!cwByPortalAndKey.TryGetValue((portalId, "DirectPublish"), out var cwl) || cwl.Count == 0)
                {
                    return null;
                }

                var cw = cwl[0];
                if (!cwsByWorkflow.TryGetValue(cw.WorkflowID, out var states) || states.Count == 0)
                {
                    return null;
                }

                var published = states.FirstOrDefault(s => publishedNames.Contains(Normalize(s.StateName)));
                if (published != null)
                {
                    return published.StateID;
                }

                // DirectPublish should only have a single state; fall back to the first entry regardless of name.
                var fallbackState = states.OrderBy(s => s.StateID).FirstOrDefault();
                return fallbackState?.StateID;
            }

            // Phase 1 + 2 + fallback for HtmlText
            var htmlTexts = repHtmlText.Get().ToList();
            foreach (var ht in htmlTexts)
            {
                if (!modules.TryGetValue(ht.ModuleID, out var module))
                {
                    continue; // orphaned; skip
                }

                var portalId = module.PortalID;

                // Phase 1: direct map
                if (stateMapping.TryGetValue((portalId, ht.StateID), out var newStateId))
                {
                    if (newStateId != ht.StateID)
                    {
                        ht.StateID = newStateId;
                        repHtmlText.Update(ht);
                    }

                    continue;
                }

                // Phase 2: name-based match within any CW in portal
                var newByName = TryMapByNameFromPortalCW(ht.StateID, portalId, legacyStateById, cwsByWorkflow, contentWorkflows, publishedNames, draftNames);
                if (newByName.HasValue && newByName.Value != ht.StateID)
                {
                    ht.StateID = newByName.Value;
                    repHtmlText.Update(ht);
                    continue;
                }

                // Phase 3: fallback to portal's DirectPublish Published
                var fallback = GetFallbackPublishedStateId(portalId);
                if (fallback.HasValue && fallback.Value != ht.StateID)
                {
                    ht.StateID = fallback.Value;
                    repHtmlText.Update(ht);
                }
            }

            // Phase 1 + 2 for HtmlTextLog (no fallback)
            var logs = repHtmlTextLog.Get().ToList();
            foreach (var log in logs)
            {
                if (!itemIdToModuleId.TryGetValue(log.ItemID, out var moduleId) || !modules.TryGetValue(moduleId, out var module))
                {
                    continue; // orphaned; skip
                }

                var portalId = module.PortalID;

                // Phase 1: direct map
                if (stateMapping.TryGetValue((portalId, log.StateID), out var newStateId))
                {
                    if (newStateId != log.StateID)
                    {
                        log.StateID = newStateId;
                        repHtmlTextLog.Update(log);
                    }

                    continue;
                }

                // Phase 2: name-based match within portal
                var newByName = TryMapByNameFromPortalCW(log.StateID, portalId, legacyStateById, cwsByWorkflow, contentWorkflows, publishedNames, draftNames);
                if (newByName.HasValue && newByName.Value != log.StateID)
                {
                    log.StateID = newByName.Value;
                    repHtmlTextLog.Update(log);
                }
            }

            ctx.Commit();
        }
    }

    private static string Normalize(string s) => (s ?? string.Empty).Trim();

    private static ContentWorkflowStateRow FindMatchingNewState(IEnumerable<ContentWorkflowStateRow> targetStates, string legacyStateName, ISet<string> publishedNames, ISet<string> draftNames)
    {
        var legacy = Normalize(legacyStateName);

        // Exact name match first
        var exact = targetStates.FirstOrDefault(ts => string.Equals(Normalize(ts.StateName), legacy, StringComparison.OrdinalIgnoreCase));
        if (exact != null)
        {
            return exact;
        }

        // Legacy 'Published' aliases
        if (string.Equals(legacy, "Published", StringComparison.OrdinalIgnoreCase))
        {
            var pub = targetStates.FirstOrDefault(ts => publishedNames.Contains(Normalize(ts.StateName)));
            if (pub != null)
            {
                return pub;
            }
        }

        // Legacy 'Draft' aliases
        if (string.Equals(legacy, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            var dr = targetStates.FirstOrDefault(ts => draftNames.Contains(Normalize(ts.StateName)));
            if (dr != null)
            {
                return dr;
            }
        }

        return null;
    }

    private static int? TryMapByNameFromPortalCW(
        int legacyStateId,
        int portalId,
        IDictionary<int, LegacyWorkflowStateRow> legacyStateById,
        IDictionary<int, List<ContentWorkflowStateRow>> cwsByWorkflow,
        IEnumerable<ContentWorkflowRow> allCw,
        ISet<string> publishedNames,
        ISet<string> draftNames)
    {
        if (!legacyStateById.TryGetValue(legacyStateId, out var legacyState))
        {
            return null;
        }

        // Enumerate all content workflows for this portal and try to match by name/aliases
        foreach (var cw in allCw.Where(x => (x.PortalID ?? -1) == portalId))
        {
            if (!cwsByWorkflow.TryGetValue(cw.WorkflowID, out var states) || states.Count == 0)
            {
                continue;
            }

            var match = FindMatchingNewState(states, legacyState.StateName, publishedNames, draftNames);
            if (match != null)
            {
                return match.StateID;
            }
        }

        return null;
    }
}
