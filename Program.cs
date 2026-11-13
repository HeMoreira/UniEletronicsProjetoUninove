using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using UniEletronicsProjetoUninove.Data;

var builder = WebApplication.CreateBuilder(args);

// Banco SQLite com caminho absoluto (evita criar arquivos em pastas diferentes).
// Pooling=False: cada conexão fecha o arquivo de verdade ao terminar. Sem isso o .NET mantém
// o arquivo aberto em segundo plano e a aplicação continua lendo/gravando num .db "fantasma"
// mesmo depois de você apagá-lo no Visual Studio.
var caminhoBanco = Path.Combine(builder.Environment.ContentRootPath, "unieletronics.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={caminhoBanco};Pooling=False"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SomenteAdmin", policy => policy.RequireRole("Administrador"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/", "SomenteAdmin");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Logout");
    options.Conventions.AllowAnonymousToPage("/Cadastro");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    options.Conventions.AllowAnonymousToPage("/Error");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

app.Logger.LogInformation("Banco de dados em uso: {Caminho}", caminhoBanco);

// Cria/atualiza o banco e garante o administrador único
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInicializador.Inicializar(context, app.Configuration, app.Logger);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();