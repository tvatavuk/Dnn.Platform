// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System.Linq;

    using Dnn.EditBar.Library.Items;
    using DotNetNuke.Application;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Personalization;

    public abstract class WorkflowBaseMenuItem : BaseMenuItem
    {
        private readonly IWorkflowEngine workflowEngine = WorkflowEngine.Instance;

        internal Workflow Workflow => WorkflowManager.Instance.GetWorkflow(this.WorkflowState.WorkflowID);

        internal WorkflowState WorkflowState => /*this.workflowState ??=*/ WorkflowStateManager.Instance.GetWorkflowState(this.ContentItem.StateID);

        internal bool IsEditMode => Personalization.GetUserMode() == PortalSettings.Mode.Edit;

        internal bool IsPlatform => DotNetNukeContext.Current.Application.SKU == "DNN";

        internal bool IsWorkflowEnabled => this.IsVersioningEnabled && TabWorkflowSettings.Instance.IsWorkflowEnabled(PortalSettings.Current.PortalId, TabController.CurrentPage.TabID);

        internal bool IsFirstState => this.WorkflowState.StateName == this.Workflow.FirstState.StateName; // 'Draft'

        internal bool IsPriorState => this.WorkflowState.StateName == this.PriorState?.StateName;

        internal bool IsLastState => this.WorkflowState.StateName == this.Workflow.LastState.StateName; // 'Published'

        internal bool IsDraftWithPermissions => this.workflowEngine.IsWorkflowOnDraft(this.ContentItem) && this.HasDraftPermission;

        internal bool IsReviewOrOtherIntermediateStateWithPermissions => !this.workflowEngine.IsWorkflowCompleted(this.ContentItem) && WorkflowSecurity.Instance.HasStateReviewerPermission(this.ContentItem.StateID);

        internal bool IsWorkflowCompleted => WorkflowEngine.Instance.IsWorkflowCompleted(TabController.CurrentPage.ContentItemId);

        internal bool HasBeenPublished => TabController.CurrentPage.HasBeenPublished;

        internal bool HasUnpublishVersion => TabVersionBuilder.Instance.GetUnPublishedVersion(TabController.CurrentPage.TabID) != null;

        internal bool HasUnpublishVersionWithPermissions => this.HasUnpublishVersion && WorkflowSecurity.Instance.HasStateReviewerPermission(this.ContentItem.StateID);

        internal bool HasDraftPermission => PermissionProvider.Instance().CanAddContentToPage(TabController.CurrentPage);

        private ContentItem ContentItem => Util.GetContentController().GetContentItem(TabController.CurrentPage.ContentItemId);

        private bool IsVersioningEnabled => TabVersionSettings.Instance.IsVersioningEnabled(PortalSettings.Current.PortalId, TabController.CurrentPage.TabID);

        // State before the last one.
        private WorkflowState PriorState => this.Workflow.States == null || !this.Workflow.States.Any() ? null : this.Workflow.States.OrderBy(s => s.Order).Reverse().Skip(1).FirstOrDefault();

        public override bool Visible() =>
            this.IsEditMode
            && this.IsPlatform
            && this.IsWorkflowEnabled;
    }
}
