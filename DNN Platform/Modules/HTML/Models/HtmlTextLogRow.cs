// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Models;

using DotNetNuke.ComponentModel.DataAnnotations;

// DAL2 POCOs for migration
[TableName("HtmlTextLog")]
[PrimaryKey("HtmlTextLogID", AutoIncrement = true)]
internal sealed class HtmlTextLogRow
{
    public int HtmlTextLogID { get; set; }

    public int ItemID { get; set; }

    public int StateID { get; set; }
}
