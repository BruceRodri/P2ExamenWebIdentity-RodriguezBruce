using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NorthwindApp.Models;

public partial class Shipper
{
    [Key]
    public short ShipperId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Phone { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
