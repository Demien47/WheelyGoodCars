using System.Collections.Generic;

namespace WheelyGoodCars.Models
{
    public class User
    {
        public int Id { get; set; }           // Primary key
        public string Name { get; set; }      // Naam van de aanbieder
        public string Email { get; set; }     // Email van de aanbieder

        public ICollection<Car> Cars { get; set; } // Navigatie-eigenschap
    }
}