using System;
using System.Collections.Generic;
using System.Linq;
using WheelyGoodCars.Data;
using WheelyGoodCars.Models;

namespace WheelyGoodCars
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            var random = new Random();

            // -------------------------
            // Stop als er al auto's zijn
            // -------------------------
            if (context.Cars.Any()) return;

            // -------------------------
            // Tags (20)
            // -------------------------
            var tagNames = new[]
            {
                "Elektrisch", "Hybride", "Diesel", "Benzine", "Automaat", "Handgeschakeld",
                "Sport", "SUV", "Gezinsauto", "Occasion", "Nieuw", "Luxe", "Compact",
                "Stationwagen", "4WD", "Airco", "Navigatie", "Bluetooth", "Cruise Control", "Panoramadak"
            };

            var tags = tagNames.Select(t => new Tag { Name = t }).ToList();
            context.Tags.AddRange(tags);
            context.SaveChanges();

            // -------------------------
            // Users (150 aanbieders)
            // -------------------------
            if (!context.Users.Any())
            {
                var users = Enumerable.Range(1, 150)
                    .Select(i => new User
                    {
                        Name = $"Aanbieder{i}",
                        Email = $"aanbieder{i}@mail.com"
                    }).ToList();

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            var usersList = context.Users.ToList(); // voor auto’s

            // -------------------------
            // Merken en modellen
            // -------------------------
            var brands = new[] { "BMW", "Audi", "Tesla", "Volkswagen", "Toyota", "Mercedes", "Ford", "Opel", "Renault", "Honda" };
            var models = new Dictionary<string, string[]>
            {
                {"BMW", new[]{ "X1", "X3", "X5", "3 Series", "5 Series" } },
                {"Audi", new[]{ "A1", "A3", "A4", "A6", "Q3", "Q5" } },
                {"Tesla", new[]{ "Model S", "Model 3", "Model X", "Model Y" } },
                {"Volkswagen", new[]{ "Golf", "Polo", "Passat", "T-Roc" } },
                {"Toyota", new[]{ "Yaris", "Corolla", "RAV4", "Camry" } },
                {"Mercedes", new[]{ "A-Class", "C-Class", "E-Class", "GLA" } },
                {"Ford", new[]{ "Fiesta", "Focus", "Kuga", "Mustang" } },
                {"Opel", new[]{ "Corsa", "Astra", "Insignia" } },
                {"Renault", new[]{ "Clio", "Megane", "Captur" } },
                {"Honda", new[]{ "Civic", "Accord", "CR-V" } }
            };

            // -------------------------
            // Auto's (250)
            // -------------------------
            var cars = new List<Car>();
            for (int i = 0; i < 250; i++)
            {
                var brand = brands[random.Next(brands.Length)];
                var model = models[brand][random.Next(models[brand].Length)];

                var user = usersList[i % usersList.Count]; // verdeel auto's over 150 aanbieders

                var car = new Car
                {
                    Brand = brand,
                    Model = model,
                    LicensePlate = $"NL-{random.Next(1000, 9999)}-{(char)('A' + random.Next(0, 26))}{(char)('A' + random.Next(0, 26))}",
                    Year = random.Next(2005, 2024),
                    Mileage = random.Next(0, 200000),
                    Price = random.Next(2000, 50000),
                    UserId = user.Id,
                    IsSold = false,
                    ImagePath = null
                };
                cars.Add(car);
            }
            context.Cars.AddRange(cars);
            context.SaveChanges();

            // -------------------------
            // CarTags (1-5 per auto)
            // -------------------------
            var carTags = new List<CarTag>();
            foreach (var car in cars)
            {
                var numberOfTags = random.Next(1, 6);
                var selectedTags = tags.OrderBy(x => random.Next()).Take(numberOfTags);

                foreach (var tag in selectedTags)
                {
                    carTags.Add(new CarTag
                    {
                        CarId = car.Id,
                        TagId = tag.Id
                    });
                }
            }
            context.CarTags.AddRange(carTags);
            context.SaveChanges();
        }
    }
}