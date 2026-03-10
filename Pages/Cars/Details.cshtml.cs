using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WheelyGoodCars.Data;
using WheelyGoodCars.Models;

namespace WheelyGoodCars.Pages.Cars
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Car Car { get; set; }

        // Separate list for display – do not assign into the tracked Car navigation
        public List<Tag> Tags { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Car = await _context.Cars.FindAsync(id);

            if (Car == null) return NotFound();

            // Load tags for display via the join table (do NOT assign into Car.Tags)
            Tags = await _context.CarTags
                .Where(ct => ct.CarId == id)
                .Include(ct => ct.Tag)
                .Select(ct => ct.Tag)
                .ToListAsync();

            // Increase views on the tracked entity
            Car.Views += 1;

            // Log a row into CarViews so "views today" can be calculated reliably
            _context.CarViews.Add(new CarView
            {
                CarId = id,
                ViewedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Page();
        }
    }
}