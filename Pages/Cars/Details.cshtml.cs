using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WheelyGoodCars.Data;
using WheelyGoodCars.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;

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

        public async Task<IActionResult> OnGetPdfAsync(int id)
        {
            var car = await _context.Cars
                .Include(c => c.Tags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Titel
                document.Add(new Paragraph("Auto informatie")
                    .SetFontSize(18));

                document.Add(new Paragraph($"Kenteken: {car.LicensePlate}"));
                document.Add(new Paragraph($"Merk: {car.Brand}"));
                document.Add(new Paragraph($"Model: {car.Model}"));
                document.Add(new Paragraph($"Bouwjaar: {car.Year}"));
                document.Add(new Paragraph($"Kilometerstand: {car.Mileage}"));
                document.Add(new Paragraph($"Prijs: € {car.Price}"));

                // Tags
                if (car.Tags.Any())
                {
                    document.Add(new Paragraph("Tags:"));
                    foreach (var tag in car.Tags)
                    {
                        document.Add(new Paragraph($"- {tag.Name}"));
                    }
                }

                document.Close();

                return File(memoryStream.ToArray(),
                    "application/pdf",
                    $"Auto_{car.Id}.pdf");
            }
        }
    }
}