// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Models;

using DotNetNuke.ComponentModel.DataAnnotations;

// DAL2 POCOs for migration
[TableName("WorkflowStates")]
[PrimaryKey("StateID", AutoIncrement = true)]
internal sealed class LegacyWorkflowStateRow
{
    public int StateID { get; set; }

    public int WorkflowID { get; set; }

    public string StateName { get; set; }
}
