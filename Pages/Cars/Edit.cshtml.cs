using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WheelyGoodCars.Data;
using WheelyGoodCars.Models;
using System.IO;

namespace WheelyGoodCars.Pages.Cars
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // Voor wwwroot path

        public EditModel(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Car Car { get; set; } = new Car();

        [BindProperty]
        public IFormFile? CarImage { get; set; } // Foto upload

        // GET: bestaande auto laden
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Car = await _context.Cars.FindAsync(id);
            if (Car == null) return NotFound();
            return Page();
        }

        // POST: wijzigingen opslaan
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Foto upload
            if (CarImage != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Unieke naam maken om conflicten te voorkomen
                var fileName = Guid.NewGuid() + Path.GetExtension(CarImage.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await CarImage.CopyToAsync(stream);
                }

                // Relatief pad opslaan in database
                Car.ImagePath = "/uploads/" + fileName;
            }

            // Auto wijzigen
            _context.Attach(Car).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Cars.Any(e => e.Id == Car.Id)) return NotFound();
                else throw;
            }

            return RedirectToPage("Index");
        }
    }
}