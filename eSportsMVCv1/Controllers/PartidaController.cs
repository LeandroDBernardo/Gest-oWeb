using eSportsMVCv1.Data;
using eSportsMVCv1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eSportsMVCv1.Controllers;

public class PartidaController : Controller
{
    private readonly ApplicationDbContext _context;
    public PartidaController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? torneioId, string? jogo, EstadoPartida? estado)
    {
        var query = _context.Partidas.Include(p => p.Torneio).Include(p => p.EquipaA).Include(p => p.EquipaB).AsNoTracking().AsQueryable();
        if (torneioId.HasValue) query = query.Where(p => p.TorneioId == torneioId.Value);
        if (!string.IsNullOrWhiteSpace(jogo)) query = query.Where(p => p.Torneio!.Jogo.Contains(jogo));
        if (estado.HasValue) query = query.Where(p => p.Estado == estado.Value);
        ViewBag.TorneioId = torneioId;
        ViewBag.Jogo = jogo;
        ViewBag.Estado = estado;
        return View(await query.OrderBy(p => p.Data).ToListAsync());
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(int? torneioId)
    {
        await LoadTournamentsAsync();
        var model = new PartidaFormViewModel { TorneioId = torneioId ?? 0 };
        await LoadTeamsAsync(model.TorneioId);
        return View(model);
    }

    [HttpPost, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartidaFormViewModel model)
    {
        if (!await ValidateMatchAsync(model))
        {
            await LoadTournamentsAsync();
            await LoadTeamsAsync(model.TorneioId);
            return View(model);
        }
        _context.Partidas.Add(new Partida
        {
            TorneioId = model.TorneioId,
            Data = model.Data,
            IdEquipaA = model.IdEquipaA,
            IdEquipaB = model.IdEquipaB,
            Estado = EstadoPartida.Agendada
        });
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { torneioId = model.TorneioId });
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Result(int id)
    {
        var partida = await _context.Partidas.FindAsync(id);
        return partida is null ? NotFound() : View(new PartidaFormViewModel
        {
            Id = partida.Id, TorneioId = partida.TorneioId, Data = partida.Data,
            IdEquipaA = partida.IdEquipaA, IdEquipaB = partida.IdEquipaB,
            ResultadoA = partida.ResultadoA, ResultadoB = partida.ResultadoB, Estado = partida.Estado
        });
    }

    [HttpPost, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Result(PartidaFormViewModel model)
    {
        var partida = await _context.Partidas.FindAsync(model.Id);
        if (partida is null) return NotFound();
        if (!model.ResultadoA.HasValue || !model.ResultadoB.HasValue)
            ModelState.AddModelError(string.Empty, "Both results are required.");
        if (model.ResultadoA < 0 || model.ResultadoB < 0)
            ModelState.AddModelError(string.Empty, "Results cannot be negative.");
        if (!ModelState.IsValid) return View(model);
        partida.ResultadoA = model.ResultadoA;
        partida.ResultadoB = model.ResultadoB;
        partida.Estado = EstadoPartida.Validada;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { torneioId = partida.TorneioId });
    }

    private async Task<bool> ValidateMatchAsync(PartidaFormViewModel model)
    {
        if (!ModelState.IsValid) return false;
        if (model.IdEquipaA == model.IdEquipaB)
            ModelState.AddModelError(string.Empty, "A team cannot play against itself.");
        var tournament = await _context.Torneios.Include(t => t.Equipas).FirstOrDefaultAsync(t => t.Id == model.TorneioId);
        if (tournament is null)
        {
            ModelState.AddModelError(nameof(model.TorneioId), "Tournament was not found.");
        }
        else if (!tournament.Equipas.Any(e => e.Id == model.IdEquipaA) || !tournament.Equipas.Any(e => e.Id == model.IdEquipaB))
        {
            ModelState.AddModelError(string.Empty, "Both teams must be enrolled in the tournament.");
        }
        return ModelState.IsValid;
    }

    private async Task LoadTournamentsAsync() => ViewBag.Torneios = await _context.Torneios.Where(t => t.Estado != EstadoTorneio.Terminado).AsNoTracking().ToListAsync();
    private async Task LoadTeamsAsync(int torneioId) => ViewBag.Equipas = await _context.Torneios.Where(t => t.Id == torneioId).SelectMany(t => t.Equipas).AsNoTracking().ToListAsync();
}
