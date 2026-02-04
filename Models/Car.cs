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

    public int Views { get; set; }

    public int UserId { get; set; } // Later voor login
}
