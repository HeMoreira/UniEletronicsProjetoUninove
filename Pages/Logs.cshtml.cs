using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniEletronicsProjetoUninove.Services;

namespace UniEletronicsProjetoUninove.Pages;

// Acesso restrito ao administrador pela regra AuthorizeFolder("/", "SomenteAdmin") do Program.cs
public class LogsModel : PageModel
{
    private const int MaximoLinhas = 1000;

    private readonly IRegistroLog _log;

    public LogsModel(IRegistroLog log)
    {
        _log = log;
    }

    public IReadOnlyList<string> Linhas { get; private set; } = Array.Empty<string>();

    public async Task OnGetAsync()
    {
        Linhas = await _log.LerUltimasLinhasAsync(MaximoLinhas);
    }

    // Baixa o arquivo completo
    public async Task<IActionResult> OnGetBaixarAsync()
    {
        var linhas = await _log.LerUltimasLinhasAsync(int.MaxValue);
        var bytes = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", linhas) + (linhas.Count > 0 ? "\n" : ""));
        return File(bytes, "text/plain; charset=utf-8", "registros.log");
    }
}
