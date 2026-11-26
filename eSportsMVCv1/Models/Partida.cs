using System.ComponentModel.DataAnnotations;

namespace eSportsMVCv1.Models
{
    public enum EstadoPartida { Agendada, Concluida, Validada }

    public class Partida
    {
        public int Id { get; set; }
        public int TorneioId { get; set; }
        public Torneio? Torneio { get; set; }
        [Required]
        public DateTime Data { get; set; }
        public int IdEquipaA { get; set; }
        public Equipa? EquipaA { get; set; }
        public int IdEquipaB { get; set; }
        public Equipa? EquipaB { get; set; }
        [Range(0, int.MaxValue)]
        public int? ResultadoA { get; set; }
        [Range(0, int.MaxValue)]
        public int? ResultadoB { get; set; }
        public EstadoPartida Estado { get; set; } = EstadoPartida.Agendada;
    }
}
