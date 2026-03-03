using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WheelyGoodCars.Data;
using WheelyGoodCars.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace WheelyGoodCars.Pages.Cars
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // Voor wwwroot path

        public CreateModel(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Car Car { get; set; } = new Car();

        [BindProperty]
        public IFormFile? CarImage { get; set; }  // Voor foto upload

        [BindProperty]
        public int Step { get; set; } = 1;

        public void OnGet()
        {
            Step = 1;
        }

        public IActionResult OnPostNext()
        {
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
            Car.Views = 0; // Start met 0 views

            // Foto upload
            if (CarImage != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Unieke bestandsnaam om conflicten te voorkomen
                var fileName = Guid.NewGuid() + Path.GetExtension(CarImage.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await CarImage.CopyToAsync(stream);
                }

                // Relatief webpad opslaan in database
                Car.ImagePath = "/uploads/" + fileName;
            }

            _context.Cars.Add(Car);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
