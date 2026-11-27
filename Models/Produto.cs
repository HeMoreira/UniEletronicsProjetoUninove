using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniEletronicsProjetoUninove.Models
{
    public class Produto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataDeRegistro { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999.99, ErrorMessage = "Informe um preço válido.")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser de no mínimo 1.")]
        public int Quantidade { get; set; }

        public int Codigo { get; set; }

        public string? ImagemPrincipal { get; set; }

        // Lista de Imagens Secundárias (Opcional, até 5 imagens)
        public List<ImagemProduto> ImagensSecundarias { get; set; } = new();

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;

        // Situação do pedido/produto (exibida na página Pedidos)
        [Required(ErrorMessage = "O status é obrigatório.")]
        [StringLength(20)]
        [AllowedValues(StatusProduto.Enviado, StatusProduto.ACaminho, StatusProduto.Recebido, StatusProduto.Cancelado,
            ErrorMessage = "Selecione um status válido.")]
        public string Status { get; set; } = StatusProduto.Enviado;
    }

    // Valores aceitos para Produto.Status (os mesmos usados nos filtros da página Pedidos)
    public static class StatusProduto
    {
        public const string Enviado = "Enviado";
        public const string ACaminho = "A caminho";
        public const string Recebido = "Recebido";
        public const string Cancelado = "Cancelado";

        public static readonly string[] Todos = { Enviado, ACaminho, Recebido, Cancelado };
    }
}