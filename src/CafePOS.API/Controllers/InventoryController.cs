using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafePOS.Infrastructure.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace CafePOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public InventoryController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> GetInventory()
    {
        var ingredients = await _context.Ingredients
            .OrderBy(i => i.Name)
            .ToListAsync();

        return Ok(new { success = true, data = ingredients });
    }
}
