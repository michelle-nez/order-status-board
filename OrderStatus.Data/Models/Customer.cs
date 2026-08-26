using System.ComponentModel.DataAnnotations;

namespace OrderStatus.Data.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    // One customer has many orders.
    public List<Order> Orders { get; set; } = new();
}
