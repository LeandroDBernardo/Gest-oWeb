using System.ComponentModel.DataAnnotations;

namespace eSportsMVCv1.Models
{
    public class Jogador
    {
        public int Id { get; set; }

        public int EquipaId { get; set; }
        public Equipa? Equipa { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = "";

        [Required(ErrorMessage = "O nickname é obrigatório.")]
        [StringLength(30)]
        public string Nickname { get; set; } = "";

        [Range(15, 45, ErrorMessage = "A idade deve estar entre 15 e 45 anos.")]
        public int Idade { get; set; }

        public ICollection<Pontuacao> Pontuacoes { get; set; } = new List<Pontuacao>();
    }
}
