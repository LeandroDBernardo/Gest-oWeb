using System.ComponentModel.DataAnnotations;

namespace eSportsMVCv1.Models
{
    public class Pontuacao
    {
        public int Id { get; set; }
        public int JogadorId { get; set; }
        public Jogador? Jogador { get; set; }

        [Range(0, 100, ErrorMessage = "A pontuação deve estar entre 0 e 100.")]
        public int Valor { get; set; }
    }
}
