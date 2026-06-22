using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NorthwindApp.Models;

public partial class Region
{
    [Key]
    public short RegionId { get; set; }

    public string RegionDescription { get; set; } = null!;

    public virtual ICollection<Territory> Territories { get; set; } = new List<Territory>();
}
