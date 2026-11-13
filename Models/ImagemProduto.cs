using System.ComponentModel.DataAnnotations;

namespace UniEletronicsProjetoUninove.Models
{
    public class ImagemProduto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty;

        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }
    }
}