// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Models;

using DotNetNuke.ComponentModel.DataAnnotations;

// DAL2 POCOs for migration
// Legacy (module) workflow tables (being removed after migration)
[TableName("Workflow")]
[PrimaryKey("WorkflowID", AutoIncrement = true)]
internal sealed class LegacyWorkflowRow
{
    public int WorkflowID { get; set; }

    public int? PortalID { get; set; }

    public string WorkflowName { get; set; }
}
