using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WheelyGoodCars.Data;

namespace WheelyGoodCars.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly AppDbContext _context;

    public DashboardModel(AppDbContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
        // Page render - data is fetched by JS via OnGetStats
    }

    // GET handler returning JSON stats
    public async Task<IActionResult> OnGetStatsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // Number currently offered (not sold)
        var offeredCount = await _context.Cars.CountAsync(c => !c.IsSold);

        // Number sold
        var soldCount = await _context.Cars.CountAsync(c => c.IsSold);

        // Number offered today (requires CreatedAt column)
        var offeredToday = 0;
        try
        {
            offeredToday = await _context.Cars.CountAsync(c =>
                EF.Property<DateTime>(c, "CreatedAt") >= today &&
                EF.Property<DateTime>(c, "CreatedAt") < tomorrow);
        }
        catch
        {
            // If CreatedAt column doesn't exist yet, fall back to 0
            offeredToday = 0;
        }

        // Number of distinct providers (UserId)
        var providers = await _context.Cars.Select(c => c.UserId).Distinct().CountAsync();

        // Views today (CarViews.ViewedAt) — use range comparison to ensure EF can translate to SQL
        var viewsToday = await _context.CarViews.CountAsync(v => v.ViewedAt >= today && v.ViewedAt < tomorrow);

        // Average cars per provider
        double avgCarsPerProvider = providers > 0
            ? await _context.Cars.CountAsync() / (double)providers
            : 0.0;

        // Views per day for last 7 days
        var from = today.AddDays(-6);
        var viewGroups = await _context.CarViews
            .Where(v => v.ViewedAt >= from && v.ViewedAt < tomorrow)
            .GroupBy(v => v.ViewedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var viewsLast7 = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var d = from.AddDays(i);
                var g = viewGroups.FirstOrDefault(x => x.Date == d);
                return new { date = d.ToString("yyyy-MM-dd"), count = g?.Count ?? 0 };
            })
            .ToList();

        // Cars added per day for last 7 days (if CreatedAt exists)
        List<object> carsAddedLast7;
        try
        {
            var carsGroups = await _context.Cars
                .Where(c => EF.Property<DateTime>(c, "CreatedAt") >= from && EF.Property<DateTime>(c, "CreatedAt") < tomorrow)
                .GroupBy(c => EF.Property<DateTime>(c, "CreatedAt").Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Cast anonymous-type items to object so the resulting List<object> assignment succeeds
            carsAddedLast7 = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var d = from.AddDays(i);
                    var g = carsGroups.FirstOrDefault(x => x.Date == d);
                    return new { date = d.ToString("yyyy-MM-dd"), count = g?.Count ?? 0 };
                })
                .Cast<object>()
                .ToList();
        }
        catch
        {
            // CreatedAt not present
            carsAddedLast7 = Enumerable.Range(0, 7)
                .Select(i => new { date = from.AddDays(i).ToString("yyyy-MM-dd"), count = 0 })
                .ToList<object>();
        }

        var payload = new
        {
            offeredCount,
            soldCount,
            offeredToday,
            providers,
            viewsToday,
            avgCarsPerProvider = Math.Round(avgCarsPerProvider, 2),
            viewsLast7,
            carsAddedLast7
        };

        return new JsonResult(payload);
    }
}