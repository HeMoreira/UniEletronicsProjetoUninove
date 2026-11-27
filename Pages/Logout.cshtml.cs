using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UniEletronicsProjetoUninove.Services;

namespace UniEletronicsProjetoUninove.Pages;

public class LogoutModel : PageModel
{
    private readonly IRegistroLog _log;

    public LogoutModel(IRegistroLog log)
    {
        _log = log;
    }

    // Se alguém acessar /Logout direto pela URL, só volta ao catálogo
    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            await _log.RegistrarAsync("LOGOUT",
                ("E-mail", User.FindFirstValue(ClaimTypes.Email)),
                ("IP", HttpContext.Connection.RemoteIpAddress?.ToString()));
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Index");
    }
}