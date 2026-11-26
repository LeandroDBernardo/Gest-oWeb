using eSportsMVCv1.Data;
using eSportsMVCv1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eSportsMVCv1.Controllers;

public class CampeonatoController : Controller
{
    private readonly ApplicationDbContext _context;
    public CampeonatoController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index() => View(await _context.Campeonatos.Include(c => c.Torneios).AsNoTracking().ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var campeonato = await _context.Campeonatos.Include(c => c.Torneios).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return campeonato is null ? NotFound() : View(campeonato);
    }

    [Authorize(Roles = "Administrator")]
    public IActionResult Create() => View();

    [HttpPost, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Campeonato campeonato)
    {
        if (!ModelState.IsValid) return View(campeonato);
        _context.Add(campeonato);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var campeonato = await _context.Campeonatos.FindAsync(id);
        return campeonato is null ? NotFound() : View(campeonato);
    }

    [HttpPost, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Campeonato input)
    {
        if (id != input.Id) return NotFound();
        if (!ModelState.IsValid) return View(input);
        var campeonato = await _context.Campeonatos.FindAsync(id);
        if (campeonato is null) return NotFound();
        campeonato.Nome = input.Nome;
        campeonato.Ano = input.Ano;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var campeonato = await _context.Campeonatos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return campeonato is null ? NotFound() : View(campeonato);
    }

    [HttpPost, ActionName("Delete"), Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var campeonato = await _context.Campeonatos.FindAsync(id);
        if (campeonato is not null) { _context.Campeonatos.Remove(campeonato); await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}
