using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NorthwindApp.Models;

public partial class CustomerDemographic
{
    [Key]
    public string CustomerTypeId { get; set; } = null!;

    public string? CustomerDesc { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
