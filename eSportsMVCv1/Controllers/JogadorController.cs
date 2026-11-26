using eSportsMVCv1.Data;
using eSportsMVCv1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eSportsMVCv1.Controllers;

[Authorize]
public class JogadorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public JogadorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? nickname)
    {
        var query = _context.Jogadores.Include(j => j.Equipa).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(nickname)) query = query.Where(j => j.Nickname.Contains(nickname));
        ViewBag.Nickname = nickname;
        return View(await query.OrderBy(j => j.Nickname).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var jogador = await _context.Jogadores.Include(j => j.Equipa).Include(j => j.Pontuacoes).AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);
        return jogador is null ? NotFound() : View(jogador);
    }

    public async Task<IActionResult> Create()
    {
        await LoadTeamsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Jogador jogador)
    {
        var equipa = await _context.Equipas.FindAsync(jogador.EquipaId);
        if (equipa is null || !CanManage(equipa)) ModelState.AddModelError(nameof(jogador.EquipaId), "You cannot add players to this team.");
        if (!ModelState.IsValid) { await LoadTeamsAsync(); return View(jogador); }
        _context.Add(jogador);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var jogador = await _context.Jogadores.FindAsync(id);
        if (jogador is null) return NotFound();
        var equipa = await _context.Equipas.FindAsync(jogador.EquipaId);
        if (equipa is null || !CanManage(equipa)) return Forbid();
        await LoadTeamsAsync();
        return View(jogador);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Jogador input)
    {
        if (id != input.Id) return NotFound();
        var jogador = await _context.Jogadores.FindAsync(id);
        if (jogador is null) return NotFound();
        var equipa = await _context.Equipas.FindAsync(input.EquipaId);
        if (equipa is null || !CanManage(equipa)) return Forbid();
        if (!ModelState.IsValid) { await LoadTeamsAsync(); return View(input); }
        jogador.Nome = input.Nome;
        jogador.Nickname = input.Nickname;
        jogador.Idade = input.Idade;
        jogador.EquipaId = input.EquipaId;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var jogador = await _context.Jogadores.Include(j => j.Equipa).AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);
        return jogador is null ? NotFound() : !CanManage(jogador.Equipa!) ? Forbid() : View(jogador);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var jogador = await _context.Jogadores.Include(j => j.Equipa).FirstOrDefaultAsync(j => j.Id == id);
        if (jogador is null) return RedirectToAction(nameof(Index));
        if (!CanManage(jogador.Equipa!)) return Forbid();
        _context.Jogadores.Remove(jogador);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadTeamsAsync() => ViewBag.Equipas = await _context.Equipas.AsNoTracking().ToListAsync();
    private bool CanManage(Equipa equipa) => User.IsInRole("Administrator") || equipa.GestorId == _userManager.GetUserId(User);
}
