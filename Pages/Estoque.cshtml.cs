using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniEletronicsProjetoUninove.Data;
using UniEletronicsProjetoUninove.Models;

namespace UniEletronicsProjetoUninove.Pages
{
    public class EstoqueModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EstoqueModel(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<Produto> Produtos { get; set; } = new();

        [TempData] public string? Mensagem { get; set; }
        [TempData] public string? Erro { get; set; }

        public async Task OnGetAsync()
        {
            Produtos = await _context.Produtos
                .AsNoTracking()
                .OrderBy(p => p.Codigo)
                .ThenBy(p => p.Id)
                .ToListAsync();
        }

        // POST /Estoque?handler=Editar
        public async Task<IActionResult> OnPostEditarAsync(int id, string? nome, string? categoria, int quantidade, string? preco)
        {
            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);
            if (produto == null)
            {
                Erro = "Produto não encontrado.";
                return RedirectToPage();
            }

            nome = nome?.Trim();
            categoria = categoria?.Trim();

            if (string.IsNullOrWhiteSpace(nome) || nome.Length > 100)
            {
                Erro = "Informe um nome válido (até 100 caracteres).";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(categoria) || categoria.Length > 50)
            {
                Erro = "Informe uma categoria válida (até 50 caracteres).";
                return RedirectToPage();
            }

            if (quantidade < 0)
            {
                Erro = "A quantidade não pode ser negativa.";
                return RedirectToPage();
            }

            // Aceita "2500.50" (input number) e "2500,50" (digitado à mão)
            var precoTexto = (preco ?? string.Empty).Trim().Replace(',', '.');
            if (!decimal.TryParse(precoTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out var precoValor)
                || precoValor < 0.01m || precoValor > 999999.99m)
            {
                Erro = "Informe um preço válido.";
                return RedirectToPage();
            }

            produto.Nome = nome;
            produto.Categoria = categoria;
            produto.Quantidade = quantidade;
            produto.Preco = precoValor;

            await _context.SaveChangesAsync();

            Mensagem = $"Produto \"{produto.Nome}\" atualizado com sucesso.";
            return RedirectToPage();
        }

        // POST /Estoque?handler=Excluir&id=5
        public async Task<IActionResult> OnPostExcluirAsync(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.ImagensSecundarias)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
            {
                Erro = "Produto não encontrado.";
                return RedirectToPage();
            }

            var nome = produto.Nome;

            // Guarda os caminhos das imagens enviadas pelo cadastro antes de remover do banco
            var arquivos = new List<string?> { produto.ImagemPrincipal };
            arquivos.AddRange(produto.ImagensSecundarias.Select(i => i.Url));

            _context.Produtos.Remove(produto); // remove também as ImagensSecundarias (cascata)
            await _context.SaveChangesAsync();

            foreach (var caminho in arquivos)
            {
                ApagarArquivoDeUpload(caminho);
            }

            Mensagem = $"Produto \"{nome}\" excluído com sucesso.";
            return RedirectToPage();
        }

        /// <summary>
        /// Apaga o arquivo físico somente se ele estiver em wwwroot/uploads
        /// (as imagens fixas em /images/ nunca são apagadas).
        /// </summary>
        private void ApagarArquivoDeUpload(string? caminhoRelativo)
        {
            if (string.IsNullOrWhiteSpace(caminhoRelativo)) return;
            if (!caminhoRelativo.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)) return;

            try
            {
                var nomeArquivo = Path.GetFileName(caminhoRelativo); // evita "../"
                var caminhoFisico = Path.Combine(_environment.WebRootPath, "uploads", nomeArquivo);

                if (System.IO.File.Exists(caminhoFisico))
                {
                    System.IO.File.Delete(caminhoFisico);
                }
            }
            catch (IOException)
            {
                // Se o arquivo estiver em uso, o produto já foi excluído; apenas ignora.
            }
        }
    }
}
