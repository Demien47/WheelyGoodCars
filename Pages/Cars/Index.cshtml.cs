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

            Cars = await _context.Cars
                .Where(c => c.UserId == currentUserId)
                .ToListAsync();
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
    }
}
