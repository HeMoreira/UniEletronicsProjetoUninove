using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniEletronicsProjetoUninove.Models;

namespace UniEletronicsProjetoUninove.Data
{
    public static class DbInicializador
    {
        public static void Inicializar(AppDbContext context, IConfiguration config, ILogger logger)
        {
            context.Database.EnsureCreated();

            AtualizarEsquemaUsuarios(context);
            RecriarAdministrador(context, config, logger);
        }

        // Adiciona o que falta em bancos criados antes desta versão, sem apagar dados.
        private static void AtualizarEsquemaUsuarios(AppDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Usuarios"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Usuarios"" PRIMARY KEY AUTOINCREMENT,
                    ""Nome"" TEXT NOT NULL,
                    ""Telefone"" TEXT NOT NULL,
                    ""Gmail"" TEXT NOT NULL,
                    ""DataNascimento"" TEXT NOT NULL,
                    ""SenhaHash"" TEXT NOT NULL,
                    ""Role"" TEXT NOT NULL DEFAULT 'Cliente'
                )");

            // Tabela Usuarios já existia (versão anterior) sem a coluna Role
            var temRole = context.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM pragma_table_info('Usuarios') WHERE name = 'Role'")
                .AsEnumerable()
                .First();

            if (temRole == 0)
            {
                context.Database.ExecuteSqlRaw(
                    @"ALTER TABLE ""Usuarios"" ADD COLUMN ""Role"" TEXT NOT NULL DEFAULT 'Cliente'");
            }

            context.Database.ExecuteSqlRaw(
                @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Usuarios_Gmail"" ON ""Usuarios"" (""Gmail"")");

            // Só pode existir um usuário com Role = 'Administrador'
            context.Database.ExecuteSqlRaw(
                @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Usuarios_Role_Administrador"" ON ""Usuarios"" (""Role"") WHERE ""Role"" = 'Administrador'");
        }

        // Cria o único administrador, apenas se ainda não existir nenhum.
        private static void RecriarAdministrador(AppDbContext context, IConfiguration config, ILogger logger)
        {
            var removidos = context.Usuarios
                .Where(u => u.Role == Papeis.Administrador)
                .ExecuteDelete();

            if (removidos > 0)
                logger.LogInformation("{Qtd} administrador(es) antigo(s) removido(s).", removidos);

            var email = (config["AdminInicial:Email"] ?? "admin@unieletronics.com").Trim().ToLowerInvariant();

            // Se esse e-mail pertence a um cliente, não dá para criar outra conta com ele
            // (índice único em Gmail), então promove essa conta (mantém a senha dela)
            var existente = context.Usuarios.FirstOrDefault(u => u.Gmail == email);
            if (existente != null)
            {
                existente.Role = Papeis.Administrador;
                context.SaveChanges();
                logger.LogWarning("A conta {Email} já existia como cliente e foi promovida a administrador.", email);
                return;
            }

            var senha = config["AdminInicial:Senha"];
            var senhaGerada = string.IsNullOrWhiteSpace(senha);
            if (senhaGerada)
            {
                // Sem senha configurada: gera uma aleatória e mostra no log
                senha = RandomNumberGenerator.GetString("ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789", 16);
            }

            var admin = new Usuario
            {
                Nome = config["AdminInicial:Nome"] ?? "Administrador",
                Telefone = string.Empty,
                Gmail = email,
                DataNascimento = new DateTime(2000, 1, 1),
                Role = Papeis.Administrador
            };
            admin.SenhaHash = new PasswordHasher<Usuario>().HashPassword(admin, senha!);

            context.Usuarios.Add(admin);
            context.SaveChanges();

            if (senhaGerada)
                logger.LogWarning("\n\nAdministrador recriado. E-mail: {Email} | \nSenha gerada (muda a cada inicialização): {Senha}\n\n", email, senha);
            else
                logger.LogInformation("Administrador recriado com o e-mail {Email}.", email);
        }
    }
}