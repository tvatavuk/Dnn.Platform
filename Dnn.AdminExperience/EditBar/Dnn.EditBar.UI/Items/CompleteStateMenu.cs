// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;
    using Dnn.EditBar.Library.Items;
    using DotNetNuke.Application;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Personalization;

    [Serializable]
    public class CompleteStateMenu : BaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name { get; } = "CompleteState";

        /// <inheritdoc/>
        public override string Text => "Submit";

        /// <inheritdoc/>
        public override string CssClass => string.Empty;

        /// <inheritdoc/>
        public override string Template { get; } = string.Empty;

        /// <inheritdoc/>
        public override string Parent { get; } = Constants.LeftMenu;

        /// <inheritdoc/>
        public override string Loader { get; } = "CompleteState";

        /// <inheritdoc/>
        public override int Order { get; } = 77;

        /// <inheritdoc/>
        public override bool Visible()
        {
            var contentItem = Util.GetContentController().GetContentItem(TabController.CurrentPage.ContentItemId);
            return Personalization.GetUserMode() == PortalSettings.Mode.Edit
                && DotNetNukeContext.Current.Application.SKU == "DNN" // IsPlatform
                && TabWorkflowSettings.Instance.IsWorkflowEnabled(PortalSettings.Current.PortalId) // workflow is enabled
                && ((WorkflowEngine.Instance.IsWorkflowOnDraft(contentItem) && PermissionProvider.Instance().CanAddContentToPage(TabController.CurrentPage))
                    || (!WorkflowEngine.Instance.IsWorkflowCompleted(contentItem) && WorkflowSecurity.Instance.HasStateReviewerPermission(contentItem.StateID)));
        }
    }
}
