using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniEletronicsProjetoUninove.Data;
using UniEletronicsProjetoUninove.Models;

namespace UniEletronicsProjetoUninove.Pages;

public class CadastroModel : PageModel
{
    private readonly AppDbContext _context;

    public CadastroModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Gmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Telefone: mantém só os dígitos e valida (10 ou 11 dígitos, com DDD)
        var telefone = new string(Input.Telefone.Where(char.IsDigit).ToArray());
        if (telefone.Length is < 10 or > 11)
        {
            ModelState.AddModelError("Input.Telefone", "Informe um telefone válido com DDD.");
        }

        // Data de nascimento
        if (Input.DataNascimento.HasValue)
        {
            var data = Input.DataNascimento.Value.Date;
            if (data > DateTime.Today)
                ModelState.AddModelError("Input.DataNascimento", "A data de nascimento não pode ser futura.");
            else if (data < new DateTime(1900, 1, 1))
                ModelState.AddModelError("Input.DataNascimento", "Data de nascimento inválida.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Gmail.Trim().ToLowerInvariant();

        // E-mail duplicado
        var emailJaExiste = await _context.Usuarios.AnyAsync(u => u.Gmail == email);
        if (emailJaExiste)
        {
            ModelState.AddModelError("Input.Gmail", "Este e-mail já está cadastrado.");
            return Page();
        }

        var usuario = new Usuario
        {
            Nome = Input.Nome.Trim(),
            Telefone = telefone,
            Gmail = email,
            DataNascimento = Input.DataNascimento!.Value.Date,
            Role = Papeis.Cliente // cadastro público nunca cria administrador
        };

        // Gera o hash da senha (PBKDF2 com salt, já incluso no ASP.NET Core)
        var hasher = new PasswordHasher<Usuario>();
        usuario.SenhaHash = hasher.HashPassword(usuario, Input.Senha);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Já deixa o cliente logado após o cadastro (role "Cliente": só compra no catálogo)
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Gmail),
            new(ClaimTypes.Role, usuario.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return RedirectToPage("/Index");
    }
}