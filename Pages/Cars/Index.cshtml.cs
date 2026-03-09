using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WheelyGoodCars.Data;
using WheelyGoodCars.Models;

namespace WheelyGoodCars.Pages.Cars
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Car> Cars { get; set; }

        public async Task OnGetAsync()
        {
            // Fake login user
            int currentUserId = 1;

            var cars = await _context.Cars
                .Where(c => c.UserId == currentUserId)
                .ToListAsync();

            if (cars.Count == 0)
            {
                Cars = cars;
                return;
            }

            var carIds = cars.Select(c => c.Id).ToList();

            var carTags = await _context.CarTags
                .Where(ct => carIds.Contains(ct.CarId))
                .Include(ct => ct.Tag)
                .ToListAsync();

            foreach (var car in cars)
            {
                car.Tags = carTags
                    .Where(ct => ct.CarId == car.Id)
                    .Select(ct => ct.Tag)
                    .ToList();
            }

            Cars = cars;
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            car.IsSold = !car.IsSold;
            await _context.SaveChangesAsync();

            // Stuur JSON terug zodat JS kan updaten
            return new JsonResult(new { success = true, isSold = car.IsSold });
        }
    }
}
