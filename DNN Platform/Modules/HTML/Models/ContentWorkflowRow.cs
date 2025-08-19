// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Models;

using DotNetNuke.ComponentModel.DataAnnotations;

// DAL2 POCOs for migration
// New platform-wide workflow tables
[TableName("ContentWorkflows")]
[PrimaryKey("WorkflowID", AutoIncrement = true)]
internal sealed class ContentWorkflowRow
{
    public int WorkflowID { get; set; }

    public int? PortalID { get; set; }

    public string WorkflowKey { get; set; }
}
