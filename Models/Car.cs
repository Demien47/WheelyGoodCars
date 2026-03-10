using System.ComponentModel.DataAnnotations;

namespace WheelyGoodCars.Models;

public class Car
{
    public int Id { get; set; }

    [Required]
    public string LicensePlate { get; set; }

    public string Brand { get; set; }

    public string Model { get; set; }

    public int Year { get; set; }

    public int Mileage { get; set; }

    public decimal Price { get; set; }

    public bool IsSold { get; set; }

    public List<Tag> Tags { get; set; } = new();

    public int UserId { get; set; } // Later voor login
    public string? ImagePath { get; set; } // pad naar foto
    public int Views { get; set; } = 0;   // aantal keer bekeken

    // New: timestamp when car was offered
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
