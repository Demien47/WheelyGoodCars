using System;

namespace WheelyGoodCars.Models
{
    public class CarView
    {
        public int Id { get; set; }

        public int CarId { get; set; }
        public Car? Car { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.Now;
    }
}