using eSportsMVCv1.Data;
using eSportsMVCv1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eSportsMVCv1.Controllers;

public class StatisticsController : Controller
{
    private readonly ApplicationDbContext _context;
    public StatisticsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Standings(int id)
    {
        var torneio = await _context.Torneios.Include(t => t.Equipas).Include(t => t.Partidas).AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (torneio is null) return NotFound();
        var rows = torneio.Equipas.Select(team =>
        {
            var matches = torneio.Partidas.Where(p => p.Estado != EstadoPartida.Agendada && (p.IdEquipaA == team.Id || p.IdEquipaB == team.Id)).ToList();
            var wins = matches.Count(p => (p.IdEquipaA == team.Id && p.ResultadoA > p.ResultadoB) || (p.IdEquipaB == team.Id && p.ResultadoB > p.ResultadoA));
            var draws = matches.Count(p => p.ResultadoA == p.ResultadoB && p.ResultadoA.HasValue);
            return new TeamStandingViewModel { EquipaId = team.Id, Nome = team.Nome, J = matches.Count, V = wins, E = draws, D = matches.Count - wins - draws, Pontos = wins * 3 + draws };
        }).OrderByDescending(x => x.Pontos).ThenByDescending(x => x.V).ThenBy(x => x.Nome).ToList();
        ViewBag.Torneio = torneio;
        return View(rows);
    }

    public async Task<IActionResult> Rankings()
    {
        var players = await _context.Jogadores.Include(j => j.Equipa).Include(j => j.Pontuacoes).AsNoTracking().ToListAsync();
        var topPlayers = players.Where(p => p.Pontuacoes.Count > 0).Select(p => new PlayerRankingViewModel { JogadorId = p.Id, Nickname = p.Nickname, Equipa = p.Equipa?.Nome ?? "", Media = p.Pontuacoes.Average(s => s.Valor) }).OrderByDescending(p => p.Media).ThenBy(p => p.Nickname).Take(3).ToList();
        ViewBag.TopPlayers = topPlayers;
        var topTeams = players.GroupBy(p => p.Equipa).Where(g => g.Key is not null && g.SelectMany(p => p.Pontuacoes).Any()).Select(g => new { Name = g.Key!.Nome, Average = g.SelectMany(p => p.Pontuacoes).Average(s => s.Valor) }).OrderByDescending(x => x.Average).Take(3).ToList();
        return View(topTeams);
    }
}
