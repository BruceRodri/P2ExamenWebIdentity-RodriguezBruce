using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NorthwindApp.Models;

public partial class Category
{
    [Key]
    public short CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public byte[]? Picture { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
