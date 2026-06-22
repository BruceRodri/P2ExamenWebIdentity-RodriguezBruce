using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NorthwindApp.Models;

public partial class UsState
{
    [Key]
    public short StateId { get; set; }

    public string? StateName { get; set; }

    public string? StateAbbr { get; set; }

    public string? StateRegion { get; set; }
}
