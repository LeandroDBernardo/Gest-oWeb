using eSportsMVCv1.Data;
using eSportsMVCv1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eSportsMVCv1.Controllers;

public class EquipaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EquipaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index() => View(await _context.Equipas.Include(e => e.Gestor).AsNoTracking().ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var equipa = await _context.Equipas.Include(e => e.Gestor).Include(e => e.Jogadores).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        return equipa is null ? NotFound() : View(equipa);
    }

    [Authorize]
    public IActionResult Create() => View();

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Equipa equipa)
    {
        if (!ModelState.IsValid) return View(equipa);
        equipa.GestorId = _userManager.GetUserId(User);
        _context.Add(equipa);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var equipa = await _context.Equipas.FindAsync(id);
        return equipa is null || !CanManage(equipa) ? Forbid() : View(equipa);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Equipa input)
    {
        if (id != input.Id) return NotFound();
        var equipa = await _context.Equipas.FindAsync(id);
        if (equipa is null) return NotFound();
        if (!CanManage(equipa)) return Forbid();
        if (!ModelState.IsValid) return View(input);
        equipa.Nome = input.Nome;
        equipa.Pais = input.Pais;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var equipa = await _context.Equipas.Include(e => e.Gestor).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        return equipa is null || !CanManage(equipa) ? Forbid() : View(equipa);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var equipa = await _context.Equipas.FindAsync(id);
        if (equipa is null) return RedirectToAction(nameof(Index));
        if (!CanManage(equipa)) return Forbid();
        if (await _context.Partidas.AnyAsync(p => p.IdEquipaA == id || p.IdEquipaB == id))
        {
            TempData["Error"] = "This team cannot be deleted because it has scheduled or recorded matches.";
            return RedirectToAction(nameof(Index));
        }
        _context.Equipas.Remove(equipa);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CanManage(Equipa equipa) => User.IsInRole("Administrator") || equipa.GestorId == _userManager.GetUserId(User);
}
