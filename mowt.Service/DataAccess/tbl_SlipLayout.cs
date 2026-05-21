using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mowt.Service.DataAccess;

public partial class tbl_SlipLayout : BaseEntity
{
    //[Key]
    //public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Text2 { get; set; } = string.Empty;
    public string Text3 { get; set; } = string.Empty;
    public string Text4 { get; set; } = string.Empty;
    public string Text5 { get; set; } = string.Empty;
    public int FontSize { get; set; } = 16;
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
    public string Alignment { get; set; } = "left";
    public string Alignment2 { get; set; } = "left";
    public string Alignment3 { get; set; } = "right";
    public string Alignment4 { get; set; } = "right";
    public string Alignment5 { get; set; } = "right";
    public string FontFamily { get; set; } = "Arial";
    public int RectWidth { get; set; } = 200;
    public int SlipID { get; set; }
    public int PrintItemType { get; set; }
    public bool isMultiLine { get; set; } = false;

    public double LineHeight { get; set; } = 1.2;
}
