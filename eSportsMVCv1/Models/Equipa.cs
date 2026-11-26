using System.ComponentModel.DataAnnotations;

namespace eSportsMVCv1.Models
{
    public class Equipa
    {
        public int Id { get; set; }

        public string? GestorId { get; set; }          // owner (captain)
        public ApplicationUser? Gestor { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = "";

        [Required(ErrorMessage = "O país é obrigatório.")]
        [StringLength(60)]
        public string Pais { get; set; } = "";

        public ICollection<Jogador> Jogadores { get; set; } = new List<Jogador>();
        public ICollection<Torneio> Torneios { get; set; } = new List<Torneio>();
    }
}
