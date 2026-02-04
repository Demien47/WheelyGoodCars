namespace WheelyGoodCars.Models;

public class CarTag
{
    public int CarId { get; set; }
    public Car Car { get; set; }

    public int TagId { get; set; }
    public Tag Tag { get; set; }
}
