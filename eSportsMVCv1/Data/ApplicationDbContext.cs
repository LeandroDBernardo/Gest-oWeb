using eSportsMVCv1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eSportsMVCv1.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Campeonato> Campeonatos => Set<Campeonato>();
    public DbSet<Torneio> Torneios => Set<Torneio>();
    public DbSet<Equipa> Equipas => Set<Equipa>();
    public DbSet<Jogador> Jogadores => Set<Jogador>();
    public DbSet<Partida> Partidas => Set<Partida>();
    public DbSet<Pontuacao> Pontuacoes => Set<Pontuacao>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Equipa>()
            .HasOne(e => e.Gestor)
            .WithMany()
            .HasForeignKey(e => e.GestorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Partida>()
            .HasOne(p => p.EquipaA)
            .WithMany()
            .HasForeignKey(p => p.IdEquipaA)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Partida>()
            .HasOne(p => p.EquipaB)
            .WithMany()
            .HasForeignKey(p => p.IdEquipaB)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Torneio>()
            .HasMany(t => t.Equipas)
            .WithMany(e => e.Torneios);
    }
}
