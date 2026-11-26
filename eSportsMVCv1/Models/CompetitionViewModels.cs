using System.ComponentModel.DataAnnotations;

namespace eSportsMVCv1.Models;

public class PartidaFormViewModel
{
    public int Id { get; set; }
    [Required]
    public int TorneioId { get; set; }
    [Required]
    public DateTime Data { get; set; } = DateTime.Now;
    [Required]
    public int IdEquipaA { get; set; }
    [Required]
    public int IdEquipaB { get; set; }
    [Range(0, int.MaxValue)]
    public int? ResultadoA { get; set; }
    [Range(0, int.MaxValue)]
    public int? ResultadoB { get; set; }
    public EstadoPartida Estado { get; set; } = EstadoPartida.Agendada;
}

public class TeamStandingViewModel
{
    public int EquipaId { get; set; }
    public string Nome { get; set; } = "";
    public int J { get; set; }
    public int V { get; set; }
    public int E { get; set; }
    public int D { get; set; }
    public int Pontos { get; set; }
}

public class PlayerRankingViewModel
{
    public int JogadorId { get; set; }
    public string Nickname { get; set; } = "";
    public string Equipa { get; set; } = "";
    public double Media { get; set; }
}
