// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library.Items;
    using Dnn.PersonaBar.Pages.Components;
    using DotNetNuke.Application;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Services.Personalization;

    [Serializable]
    public class WorkflowStateMenu : BaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name { get; } = "WorkflowState";

        /// <inheritdoc/>
        public override string Text => TabController.CurrentPage.HasBeenPublished && WorkflowHelper.IsWorkflowCompleted(TabController.CurrentPage) ? "Published" : "Draft";

        /// <inheritdoc/>
        public override string CssClass => string.Empty;

        /// <inheritdoc/>
        public override string Template { get; } = string.Empty;

        /// <inheritdoc/>
        public override string Parent { get; } = Library.Constants.LeftMenu;

        /// <inheritdoc/>
        public override string Loader { get; } = "WorkflowState";

        /// <inheritdoc/>
        public override int Order { get; } = 77;

        /// <inheritdoc/>
        public override bool Visible()
        {
            var contentItem = Util.GetContentController().GetContentItem(TabController.CurrentPage.ContentItemId);
            return Personalization.GetUserMode() == PortalSettings.Mode.Edit
                && DotNetNukeContext.Current.Application.SKU == "DNN" // IsPlatform
                && TabVersionSettings.Instance.IsVersioningEnabled(PortalSettings.Current.PortalId, TabController.CurrentPage.TabID) // versioning is enabled
                && TabWorkflowSettings.Instance.IsWorkflowEnabled(PortalSettings.Current.PortalId, TabController.CurrentPage.TabID); // workflow is enabled
        }
    }
}
