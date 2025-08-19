// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Models;

using DotNetNuke.ComponentModel.DataAnnotations;

// DAL2 POCOs for migration
[TableName("Modules")]
[PrimaryKey("ModuleID", AutoIncrement = false)]
internal sealed class ModuleRow
{
    public int ModuleID { get; set; }

    public int PortalID { get; set; }
}
