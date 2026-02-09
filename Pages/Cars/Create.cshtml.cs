using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WheelyGoodCars.Data;
using WheelyGoodCars.Models;

namespace WheelyGoodCars.Pages.Cars
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Car Car { get; set; }

        // Stap van het formulier: 1 of 2
        [BindProperty]
        public int Step { get; set; } = 1;

        public void OnGet()
        {
            // Begin bij stap 1
            Step = 1;
        }

        public IActionResult OnPostNext()
        {
            // Ga van stap 1 naar stap 2
            Step = 2;
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                Step = 2;
                return Page();
            }

            // Fake login
            Car.UserId = 1;

            _context.Cars.Add(Car);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
