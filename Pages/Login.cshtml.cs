using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniEletronicsProjetoUninove.Data;
using Papeis = UniEletronicsProjetoUninove.Models.Papeis;
using UsuarioModel = UniEletronicsProjetoUninove.Models.Usuario; // alias: evita conflito com a propriedade "Usuario"

namespace UniEletronicsProjetoUninove.Pages;

public class LoginModel : PageModel
{
    private readonly AppDbContext _context;

    public LoginModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    [Required(ErrorMessage = "Informe o e-mail.")]
    [Display(Name = "E-mail")]
    public string Usuario { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Todos os usuários (inclusive o administrador) vêm do banco de dados
        var email = Usuario.Trim().ToLowerInvariant();
        var conta = await _context.Usuarios.FirstOrDefaultAsync(u => u.Gmail == email);

        var hasher = new PasswordHasher<UsuarioModel>();
        var senhaOk = false;

        if (conta != null)
        {
            var resultado = hasher.VerifyHashedPassword(conta, conta.SenhaHash, Senha);
            senhaOk = resultado != PasswordVerificationResult.Failed;

            // Atualiza o hash se o algoritmo do ASP.NET evoluiu
            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                conta.SenhaHash = hasher.HashPassword(conta, Senha);
                await _context.SaveChangesAsync();
            }
        }
        else
        {
            // Gasta o mesmo tempo de uma verificação real (dificulta descobrir e-mails cadastrados)
            hasher.HashPassword(new UsuarioModel(), Senha);
        }

        if (!senhaOk || conta == null)
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            return Page();
        }

        var identity = new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, conta.Id.ToString()),
            new(ClaimTypes.Name, conta.Nome),
            new(ClaimTypes.Email, conta.Gmail),
            new(ClaimTypes.Role, conta.Role)
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (Url.IsLocalUrl(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return conta.Role == Papeis.Administrador
            ? RedirectToPage("/Admin")
            : RedirectToPage("/Index");
    }
}