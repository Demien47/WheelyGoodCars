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
        private readonly IWebHostEnvironment _env;

        public EditModel(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Car Car { get; set; } = new Car();

        [BindProperty]
        public IFormFile? CarImage { get; set; } // Foto upload

        [BindProperty]
        public List<int> SelectedTags { get; set; } = new(); // Tags die aangevinkt zijn

        public List<Tag> AllTags { get; set; } = new(); // Alle beschikbare tags

        // GET: bestaande auto + tags ophalen
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Car = await _context.Cars
                .Include(c => c.Tags) // Tags erbij laden
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Car == null) return NotFound();

            AllTags = await _context.Tags.ToListAsync();

            // Vooraf geselecteerde tags vullen
            SelectedTags = Car.Tags.Select(t => t.Id).ToList();

            return Page();
        }

        // POST: auto + foto + tags opslaan
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AllTags = await _context.Tags.ToListAsync();
                return Page();
            }

            // Foto upload
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

            // Update auto
            _context.Attach(Car).State = EntityState.Modified;

            // TAGS aanpassen
            var currentCarTags = _context.CarTags.Where(ct => ct.CarId == Car.Id);
            _context.CarTags.RemoveRange(currentCarTags);

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
            }

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