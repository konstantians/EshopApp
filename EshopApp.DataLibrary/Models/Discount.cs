﻿namespace EshopApp.DataLibrary.Models;

public class Discount
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int? Percentage { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<Variant> Variants { get; set; } = new List<Variant>();
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
