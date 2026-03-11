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

        // All tags for the filter UI
        public List<Tag> AllTags { get; set; } = new();

        // Optional selected tag from query string: ?tagId=1
        [FromQuery(Name = "tagId")]
        public int? SelectedTagId { get; set; }

        // Pagination (bind from query ?pageNumber=1)
        [FromQuery(Name = "pageNumber")]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 5; // adjust as needed

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            // Load tags for the filter UI
            AllTags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();

            // Build base query for public offerings: available cars (not sold)
            IQueryable<Car> carsQuery = _context.Cars.Where(c => !c.IsSold).OrderBy(c => c.Id);

            // If a tag is selected, restrict cars to those that have that tag
            if (SelectedTagId.HasValue)
            {
                var carIdsWithTag = _context.CarTags
                    .Where(ct => ct.TagId == SelectedTagId.Value)
                    .Select(ct => ct.CarId);

                carsQuery = carsQuery.Where(c => carIdsWithTag.Contains(c.Id));
            }

            // total count for pagination
            TotalItems = await carsQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

            // fetch page
            var cars = await carsQuery
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
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

        // Live search handler used by AJAX (no page reload) — returns paged results
        public async Task<IActionResult> OnGetSearchAsync([FromQuery] string q, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 6, [FromQuery(Name = "tagId")] int? tagId = null)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return new JsonResult(new { results = Array.Empty<object>(), totalItems = 0, totalPages = 0, pageNumber = 1 });
            }

            var term = q.Trim();
            var pattern = $"%{term}%";

            // base query — only available cars (match Index behaviour)
            IQueryable<Car> carsQuery = _context.Cars.Where(c => !c.IsSold &&
                (EF.Functions.Like(c.Brand ?? "", pattern) || EF.Functions.Like(c.Model ?? "", pattern)));

            if (tagId.HasValue)
            {
                var carIdsWithTag = _context.CarTags
                    .Where(ct => ct.TagId == tagId.Value)
                    .Select(ct => ct.CarId);

                carsQuery = carsQuery.Where(c => carIdsWithTag.Contains(c.Id));
            }

            var totalItems = await carsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages && totalPages > 0) pageNumber = totalPages;

            var matched = await carsQuery
                .OrderBy(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var carIds = matched.Select(c => c.Id).ToList();

            var carTags = await _context.CarTags
                .Where(ct => carIds.Contains(ct.CarId))
                .Include(ct => ct.Tag)
                .ToListAsync();

            var results = matched.Select(c => new
            {
                c.Id,
                c.LicensePlate,
                Brand = c.Brand ?? string.Empty,
                Model = c.Model ?? string.Empty,
                Price = c.Price,
                IsSold = c.IsSold,
                Tags = carTags.Where(ct => ct.CarId == c.Id).Select(ct => ct.Tag.Name).ToArray()
            }).ToArray();

            return new JsonResult(new
            {
                results,
                totalItems,
                totalPages,
                pageNumber
            });
        }
    }
}
