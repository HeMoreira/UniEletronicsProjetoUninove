using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniEletronicsProjetoUninove.Data;
using UniEletronicsProjetoUninove.Models;

namespace UniEletronicsProjetoUninove.Pages
{
    public class RegistroVendasModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public RegistroVendasModel(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Produto ProdutoInput { get; set; } = new();

        [BindProperty]
        public IFormFile? UploadImagem { get; set; }

        // Propriedade para envio de múltiplas imagens secundárias
        [BindProperty]
        public List<IFormFile>? UploadImagensSecundarias { get; set; }

        public List<Produto> Produtos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Carrega os produtos incluindo as imagens secundárias associadas
            Produtos = await _context.Produtos
                .Include(p => p.ImagensSecundarias)
                .OrderByDescending(p => p.DataDeRegistro)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("ProdutoInput.Codigo");

            // Validação de limite máximo de 5 imagens secundárias
            if (UploadImagensSecundarias != null && UploadImagensSecundarias.Count > 5)
            {
                ModelState.AddModelError("UploadImagensSecundarias", "Você pode enviar no máximo 5 imagens secundárias.");
            }

            if (!ModelState.IsValid)
            {
                Produtos = await _context.Produtos
                    .Include(p => p.ImagensSecundarias)
                    .OrderByDescending(p => p.DataDeRegistro)
                    .ToListAsync();

                return Page();
            }

            int maxCodigo = await _context.Produtos.MaxAsync(p => (int?)p.Codigo) ?? 0;
            int novoCodigo = maxCodigo + 1;

            if (novoCodigo > 999)
            {
                ModelState.AddModelError(string.Empty, "Limite de códigos (999) atingido.");
                Produtos = await _context.Produtos
                    .Include(p => p.ImagensSecundarias)
                    .OrderByDescending(p => p.DataDeRegistro)
                    .ToListAsync();

                return Page();
            }

            ProdutoInput.Codigo = novoCodigo;

            string folderPath = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Upload da imagem principal
            if (UploadImagem != null)
            {
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(UploadImagem.FileName)}";
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadImagem.CopyToAsync(stream);
                }

                ProdutoInput.ImagemPrincipal = $"/uploads/{fileName}";
            }

            // Upload das imagens secundárias (Até 5)
            if (UploadImagensSecundarias != null && UploadImagensSecundarias.Any())
            {
                foreach (var imgFile in UploadImagensSecundarias.Take(5))
                {
                    if (imgFile.Length > 0)
                    {
                        string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imgFile.FileName)}";
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imgFile.CopyToAsync(stream);
                        }

                        ProdutoInput.ImagensSecundarias.Add(new ImagemProduto
                        {
                            Url = $"/uploads/{fileName}"
                        });
                    }
                }
            }

            _context.Produtos.Add(ProdutoInput);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}