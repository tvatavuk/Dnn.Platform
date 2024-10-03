// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;

    [Serializable]
    public class DiscardWorkflowMenu : WorkflowBaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name { get; } = "DiscardWorkflow";

        /// <inheritdoc/>
        public override string Text => "Discard";

        /// <inheritdoc/>
        public override string CssClass => string.Empty;

        /// <inheritdoc/>
        public override string Template { get; } = string.Empty;

        /// <inheritdoc/>
        public override string Parent { get; } = Constants.LeftMenu;

        /// <inheritdoc/>
        public override string Loader { get; } = "DiscardWorkflow";

        /// <inheritdoc/>
        public override int Order { get; } = 81;

        /// <inheritdoc/>
        public override bool Visible() => base.Visible()
            && ((!this.IsLastState && (this.IsDraftWithPermissions || this.IsReviewOrOtherIntermediateStateWithPermissions))
            || (this.IsLastState && this.HasUnpublishVersion && this.HasDraftPermission)); // for Direct Publish workflow
    }
}
