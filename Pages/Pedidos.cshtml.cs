using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniEletronicsProjetoUninove.Data;
using UniEletronicsProjetoUninove.Models;

namespace UniEletronicsProjetoUninove.Pages
{
    public class PedidosModel : PageModel
    {
        private readonly AppDbContext _context;

        public PedidosModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Produto> Produtos { get; set; } = new();

        public async Task OnGetAsync()
        {
            Produtos = await _context.Produtos
                .AsNoTracking()
                .OrderByDescending(p => p.DataDeRegistro)
                .ThenBy(p => p.Codigo)
                .ToListAsync();
        }
    }
}