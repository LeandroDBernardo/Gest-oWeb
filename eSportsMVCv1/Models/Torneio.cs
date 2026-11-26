using System.ComponentModel.DataAnnotations;

namespace eSportsMVCv1.Models
{
    public enum EstadoTorneio { Aberto, EmCurso, Terminado }

    public class Torneio
    {
        public int Id { get; set; }

        public int CampeonatoId { get; set; }
        public Campeonato? Campeonato { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = "";

        [Required(ErrorMessage = "O jogo é obrigatório.")]
        [StringLength(100)]
        public string Jogo { get; set; } = "";

        public EstadoTorneio Estado { get; set; } = EstadoTorneio.Aberto;

        // N:N (RF05, due 03-Dez; included now so you don't need a new migration)
        public ICollection<Equipa> Equipas { get; set; } = new List<Equipa>();
        public ICollection<Partida> Partidas { get; set; } = new List<Partida>();
    }
}
