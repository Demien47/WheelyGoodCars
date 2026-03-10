using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WheelyGoodCars.Data;
using WheelyGoodCars.Models;

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
        public IFormFile? CarImage { get; set; }  // Foto upload

        [BindProperty]
        public int Step { get; set; } = 1;

        [BindProperty]
        public List<int> SelectedTags { get; set; } = new();

        public List<Tag> AllTags { get; set; } = new();

        // GET: start bij stap 1 en laad alle tags alvast
        public async Task OnGetAsync()
        {
            Step = 1;
            AllTags = await _context.Tags.ToListAsync();
        }

        // POST: stap 1 ? stap 2
        public async Task<IActionResult> OnPostNextAsync()
        {
            Step = 2;
            AllTags = await _context.Tags.ToListAsync(); // belangrijk om tags te laden
            return Page();
        }

        // POST: opslaan auto + foto + tags
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                Step = 2;
                AllTags = await _context.Tags.ToListAsync(); // opnieuw tags ophalen
                return Page();
            }

            // Fake login
            Car.UserId = 1;
            Car.Views = 0; // start met 0 views

            // Set created timestamp (new property)
            Car.CreatedAt = DateTime.Now;

            // FOTO UPLOAD
            if (CarImage != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(CarImage.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await CarImage.CopyToAsync(stream);
                }

                Car.ImagePath = "/uploads/" + fileName;
            }

            // AUTO OPSLAAN
            _context.Cars.Add(Car);
            await _context.SaveChangesAsync();

            // TAGS OPSLAAN
            if (SelectedTags.Count > 0)
            {
                foreach (var tagId in SelectedTags)
                {
                    _context.CarTags.Add(new CarTag
                    {
                        CarId = Car.Id,
                        TagId = tagId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }
}