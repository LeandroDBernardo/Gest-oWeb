using eSportsMVCv1.Data;
using eSportsMVCv1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eSportsMVCv1.Controllers;

public class TorneioController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TorneioController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? jogo, int? ano, EstadoTorneio? estado)
    {
        var query = _context.Torneios.Include(t => t.Campeonato).Include(t => t.Equipas).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(jogo)) query = query.Where(t => t.Jogo.Contains(jogo));
        if (ano.HasValue) query = query.Where(t => t.Campeonato!.Ano == ano.Value);
        if (estado.HasValue) query = query.Where(t => t.Estado == estado.Value);
        ViewBag.Jogo = jogo;
        ViewBag.Ano = ano;
        ViewBag.Estado = estado;
        return View(await query.OrderBy(t => t.Nome).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var torneio = await _context.Torneios.Include(t => t.Campeonato).Include(t => t.Equipas).AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (torneio is null) return NotFound();
        var userId = _userManager.GetUserId(User);
        ViewBag.AvailableTeams = await _context.Equipas.Where(e => e.GestorId == userId && !torneio.Equipas.Select(t => t.Id).Contains(e.Id)).AsNoTracking().ToListAsync();
        return View(torneio);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id, int equipaId)
    {
        var torneio = await _context.Torneios.Include(t => t.Equipas).FirstOrDefaultAsync(t => t.Id == id);
        var equipa = await _context.Equipas.FindAsync(equipaId);
        if (torneio is null || equipa is null) return NotFound();
        if (torneio.Estado != EstadoTorneio.Aberto)
        {
            TempData["Error"] = "Teams can only enroll while the tournament is open.";
        }
        else if (equipa.GestorId != _userManager.GetUserId(User) && !User.IsInRole("Administrator"))
        {
            return Forbid();
        }
        else if (!torneio.Equipas.Any(e => e.Id == equipaId))
        {
            torneio.Equipas.Add(equipa);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id, int equipaId)
    {
        var torneio = await _context.Torneios.Include(t => t.Equipas).FirstOrDefaultAsync(t => t.Id == id);
        var equipa = await _context.Equipas.FindAsync(equipaId);
        if (torneio is null || equipa is null) return NotFound();
        if (equipa.GestorId != _userManager.GetUserId(User) && !User.IsInRole("Administrator")) return Forbid();
        var enrolledTeam = torneio.Equipas.FirstOrDefault(e => e.Id == equipaId);
        if (enrolledTeam is not null)
        {
            torneio.Equipas.Remove(enrolledTeam);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id)
    {
        var torneio = await _context.Torneios.Include(t => t.Equipas).FirstOrDefaultAsync(t => t.Id == id);
        if (torneio is null) return NotFound();
        if (torneio.Equipas.Count < 4)
        {
            TempData["Error"] = "A tournament requires at least four enrolled teams before it can start.";
            return RedirectToAction(nameof(Details), new { id });
        }
        torneio.Estado = EstadoTorneio.EmCurso;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create()
    {
        await LoadSelectionsAsync();
        return View();
    }

    [HttpPost, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Torneio torneio, int[] equipaIds)
    {
        if (!ModelState.IsValid) { await LoadSelectionsAsync(); return View(torneio); }
        torneio.Equipas = await _context.Equipas.Where(e => equipaIds.Contains(e.Id)).ToListAsync();
        _context.Add(torneio);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var torneio = await _context.Torneios.Include(t => t.Equipas).FirstOrDefaultAsync(t => t.Id == id);
        if (torneio is null) return NotFound();
        await LoadSelectionsAsync();
        ViewBag.SelectedTeamIds = torneio.Equipas.Select(e => e.Id).ToHashSet();
        return View(torneio);
    }

    [HttpPost, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Torneio input, int[] equipaIds)
    {
        if (id != input.Id) return NotFound();
        if (!ModelState.IsValid) { await LoadSelectionsAsync(); return View(input); }
        var torneio = await _context.Torneios.Include(t => t.Equipas).FirstOrDefaultAsync(t => t.Id == id);
        if (torneio is null) return NotFound();
        torneio.Nome = input.Nome;
        torneio.Jogo = input.Jogo;
        torneio.Estado = input.Estado;
        torneio.CampeonatoId = input.CampeonatoId;
        torneio.Equipas = await _context.Equipas.Where(e => equipaIds.Contains(e.Id)).ToListAsync();
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var torneio = await _context.Torneios.Include(t => t.Campeonato).AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return torneio is null ? NotFound() : View(torneio);
    }

    [HttpPost, ActionName("Delete"), Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var torneio = await _context.Torneios.FindAsync(id);
        if (torneio is not null) { _context.Torneios.Remove(torneio); await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectionsAsync()
    {
        ViewBag.Campeonatos = await _context.Campeonatos.AsNoTracking().ToListAsync();
        ViewBag.Equipas = await _context.Equipas.AsNoTracking().ToListAsync();
    }
}
