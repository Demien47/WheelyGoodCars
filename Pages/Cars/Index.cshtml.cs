using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WheelyGoodCars.Data;
using WheelyGoodCars.Models;

namespace WheelyGoodCars.Pages.Cars;

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
        int currentUserId = 1; // Fake login

        Cars = await _context.Cars
            .Where(c => c.UserId == currentUserId)
            .ToListAsync();
    }
}
