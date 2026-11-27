using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniEletronicsProjetoUninove.Data;
using UniEletronicsProjetoUninove.Models;

namespace UniEletronicsProjetoUninove.Pages
{
    public class RastreioModel : PageModel
    {
        private readonly AppDbContext _context;

        public RastreioModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int? ProdutoId { get; set; }

        public bool MostrarResultado { get; private set; }
        public Produto? ProdutoEncontrado { get; private set; }
        public string MensagemErro { get; private set; } = "";

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ProdutoId.HasValue || ProdutoId.Value <= 0)
            {
                MensagemErro = "Digite um ID de produto válido.";
                return Page();
            }

            ProdutoEncontrado = await _context.Produtos
                .AsNoTracking()
                .Include(p => p.ImagensSecundarias)
                .FirstOrDefaultAsync(p => p.Id == ProdutoId.Value);

            MostrarResultado = true;
            if (ProdutoEncontrado == null)
                MensagemErro = $"Não encontramos um produto com o ID {ProdutoId.Value} no banco de dados.";

            return Page();
        }
    }
}
