using System.ComponentModel.DataAnnotations;

namespace eSportsMVCv1.Models
{
    public class Campeonato
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = "";

        [Range(2000, 2100, ErrorMessage = "Ano inválido.")]
        public int Ano { get; set; }

        public ICollection<Torneio> Torneios { get; set; } = new List<Torneio>();
    }
}
