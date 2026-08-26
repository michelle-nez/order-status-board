using System.ComponentModel.DataAnnotations;

namespace OrderStatus.Data.Models;

public class Order
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Order number is required.")]
    [MaxLength(40)]
    [Display(Name = "Order number")]
    public string OrderNumber { get; set; } = string.Empty;

    // decimal, not double - money must be exact.
    [Range(0, 999999, ErrorMessage = "Total must be between 0 and 999,999.")]
    public decimal Total { get; set; }

    [Required]
    [MaxLength(40)]
    public string Channel { get; set; } = "Shopify";

    public DateTime PlacedUtc { get; set; } = DateTime.UtcNow;

    // Cancelling hides an order. It never deletes the row.
    public bool IsCancelled { get; set; }

    // Foreign key one: the related second table.
    [Range(1, int.MaxValue, ErrorMessage = "Choose a customer.")]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Foreign key two: the lookup.
    [Range(1, int.MaxValue, ErrorMessage = "Choose a status.")]
    [Display(Name = "Status")]
    public int OrderStateId { get; set; }
    public OrderState? OrderState { get; set; }
}
