using System.ComponentModel.DataAnnotations;

namespace OrderStatus.Data.Models;

/// <summary>
/// The lookup table. A short controlled list, so a status is never typed two
/// different ways, and SortOrder keeps the board columns in a stable order.
/// </summary>
public class OrderState
{
    public int Id { get; set; }

    [Required]
    [MaxLength(40)]
    public string Name { get; set; } = string.Empty;

    // Drives the left-to-right order of the board columns.
    public int SortOrder { get; set; }

    // The colour swatch the board column uses.
    [Required]
    [MaxLength(20)]
    public string Accent { get; set; } = "#5b8cff";

    public List<Order> Orders { get; set; } = new();
}
