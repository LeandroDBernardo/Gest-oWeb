using eSportsMVCv1.Models;
using Microsoft.EntityFrameworkCore;

namespace eSportsMVCv1.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var campeonato = await context.Campeonatos.FirstOrDefaultAsync(c => c.Nome == "RCM5 eSports Championship")
            ?? new Campeonato { Nome = "RCM5 eSports Championship", Ano = DateTime.UtcNow.Year };
        if (campeonato.Id == 0) context.Campeonatos.Add(campeonato);

        var tournament = await context.Torneios.Include(t => t.Equipas).FirstOrDefaultAsync(t => t.Nome == "UEFA Champions League");
        if (tournament is null)
        {
            tournament = await context.Torneios.Include(t => t.Equipas).FirstOrDefaultAsync(t => t.Nome == "RCM5 Opening Cup")
                ?? new Torneio();
        }
        tournament.Nome = "UEFA Champions League";
        tournament.Jogo = "UEFA Champions League";
        tournament.Estado = EstadoTorneio.Aberto;
        tournament.Campeonato = campeonato;
        if (tournament.Id == 0) context.Torneios.Add(tournament);

        var teamsData = new[]
        {
            (Name: "Real Madrid", Country: "Spain", Players: new[] { "Thibaut Courtois", "Dani Carvajal", "Éder Militão", "Antonio Rüdiger", "Ferland Mendy", "Federico Valverde", "Jude Bellingham", "Luka Modrić", "Vinícius Júnior", "Kylian Mbappé", "Rodrygo" }),
            (Name: "Barcelona", Country: "Spain", Players: new[] { "Marc-André ter Stegen", "Jules Koundé", "Ronald Araújo", "Pau Cubarsí", "Alejandro Balde", "Pedri", "Frenkie de Jong", "Gavi", "Lamine Yamal", "Robert Lewandowski", "Raphinha" }),
            (Name: "Manchester City", Country: "England", Players: new[] { "Ederson", "Kyle Walker", "Rúben Dias", "Joško Gvardiol", "Nathan Aké", "Rodri", "Kevin De Bruyne", "Bernardo Silva", "Phil Foden", "Erling Haaland", "Jérémy Doku" }),
            (Name: "Bayern Munich", Country: "Germany", Players: new[] { "Manuel Neuer", "Joshua Kimmich", "Dayot Upamecano", "Alphonso Davies", "Matthijs de Ligt", "Leon Goretzka", "Jamal Musiala", "Leroy Sané", "Serge Gnabry", "Harry Kane", "Kingsley Coman" }),
            (Name: "Galatasaray", Country: "Türkiye", Players: new[] { "Fernando Muslera", "Kaan Ayhan", "Davinson Sánchez", "Victor Nelsson", "Abdülkerim Bardakcı", "Lucas Torreira", "Dries Mertens", "Barış Alper Yılmaz", "Kerem Aktürkoğlu", "Mauro Icardi", "Wilfried Zaha" }),
            (Name: "Beşiktaş", Country: "Türkiye", Players: new[] { "Mert Günok", "Onur Bulut", "Gabriel Paulista", "Felix Uduokhai", "Arthur Masuaku", "Gedson Fernandes", "Salih Uçan", "Milot Rashica", "Rafa Silva", "Semih Kılıçsoy", "Ciro Immobile" }),
            (Name: "Fenerbahçe", Country: "Türkiye", Players: new[] { "Dominik Livaković", "Bright Osayi-Samuel", "Alexander Djiku", "Çağlar Söyüncü", "Jayden Oosterwolde", "Fred", "İsmail Yüksek", "İrfan Can Kahveci", "Dušan Tadić", "Edin Džeko", "Youssef En-Nesyri" })
        };

        var teams = new List<Equipa>();
        for (var teamIndex = 0; teamIndex < teamsData.Length; teamIndex++)
        {
            var data = teamsData[teamIndex];
            var oldName = new[] { "Lisbon Titans", "Porto Dragons", "Madrid Wolves", "Paris Falcons" }.ElementAtOrDefault(teamIndex);
            var team = await context.Equipas.Include(e => e.Jogadores).FirstOrDefaultAsync(e => e.Nome == data.Name || e.Nome == oldName);
            if (team is null)
            {
                team = new Equipa { Nome = data.Name, Pais = data.Country };
                context.Equipas.Add(team);
            }
            team.Nome = data.Name;
            team.Pais = data.Country;
            teams.Add(team);

            for (var playerIndex = 0; playerIndex < data.Players.Length; playerIndex++)
            {
                var player = team.Jogadores.OrderBy(p => p.Id).ElementAtOrDefault(playerIndex);
                if (player is null)
                {
                    player = new Jogador { Equipa = team };
                    context.Jogadores.Add(player);
                }
                player.Nome = data.Players[playerIndex];
                player.Nickname = data.Players[playerIndex];
                player.Idade = 20 + (playerIndex % 15);
            }
        }

        foreach (var team in teams)
        {
            if (!tournament.Equipas.Any(existing => existing.Id == team.Id))
            {
                tournament.Equipas.Add(team);
            }
        }
        await context.SaveChangesAsync();

        foreach (var player in teams.SelectMany(t => t.Jogadores))
        {
            if (!await context.Pontuacoes.AnyAsync(p => p.JogadorId == player.Id))
            {
                context.Pontuacoes.AddRange(
                    new Pontuacao { JogadorId = player.Id, Valor = 75 },
                    new Pontuacao { JogadorId = player.Id, Valor = 80 });
            }
        }

        if (!await context.Partidas.AnyAsync(p => p.TorneioId == tournament.Id))
        {
            context.Partidas.Add(new Partida
            {
                TorneioId = tournament.Id,
                Data = DateTime.Now.AddDays(1),
                IdEquipaA = teams[0].Id,
                IdEquipaB = teams[1].Id,
                Estado = EstadoPartida.Agendada
            });
        }

        await context.SaveChangesAsync();
    }
}