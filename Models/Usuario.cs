using System.ComponentModel.DataAnnotations;

namespace UniEletronicsProjetoUninove.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Gmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        // Guarda apenas o hash (PBKDF2), nunca a senha original
        public string SenhaHash { get; set; } = string.Empty;

        // "Administrador" (um único, criado na inicialização) ou "Cliente" (cadastro comum)
        [Required]
        public string Role { get; set; } = Papeis.Cliente;
    }
}