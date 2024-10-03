// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;

    [Serializable]
    public class CompleteStateMenu : WorkflowBaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name { get; } = "CompleteState";

        /// <inheritdoc/>
        public override string Text => this.IsFirstState ? "Submit" : "Approve"; // (1) "Submit" -> (2..last-2) "Approve" -> (last) HIDDEN

        /// <inheritdoc/>
        public override string CssClass => string.Empty;

        /// <inheritdoc/>
        public override string Template { get; } = string.Empty;

        /// <inheritdoc/>
        public override string Parent { get; } = Constants.LeftMenu;

        /// <inheritdoc/>
        public override string Loader { get; } = "CompleteState";

        /// <inheritdoc/>
        public override int Order { get; } = 78;

        /// <inheritdoc/>
        public override bool Visible() => base.Visible()
            && !this.IsLastState // not the last 'Published' state
            && !this.IsPriorState // not the state that is before last 'Published' state
            && (this.IsDraftWithPermissions || this.IsReviewOrOtherIntermediateStateWithPermissions);
    }
}
