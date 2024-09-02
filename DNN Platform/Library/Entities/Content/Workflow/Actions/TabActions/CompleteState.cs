// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions.TabActions
{
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// Completes a state, moving the workflow forward to the next state. If the next state is not the last one,
    /// it sends notifications to the reviewers of the next state; otherwise, it sends a notification to the user
    /// who submitted the draft once the workflow is complete.
    /// </summary>
    internal class CompleteState : TabActionBase, IWorkflowAction
    {
        /// <inheritdoc />
        public void DoActionOnStateChanging(StateTransaction stateTransaction)
        {
            // nothing
        }

        /// <inheritdoc />
        public void DoActionOnStateChanged(StateTransaction stateTransaction)
            => RemoveCache(stateTransaction);

        /// <inheritdoc />
        public ActionMessage GetActionMessage(StateTransaction stateTransaction, WorkflowState currentState)
        {
            var contentItem = GetContentItem(stateTransaction.ContentItemId);

            // TODO: add real message with localization
            return new ActionMessage
            {
                Subject = $"CompleteState TabAction {nameof(CompleteState)}: {contentItem.ContentTitle}",
                Body = $"CompleteState TabAction {nameof(CompleteState)}...",
            };
        }
    }
}
