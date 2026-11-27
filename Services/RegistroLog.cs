using System.Text;

namespace UniEletronicsProjetoUninove.Services
{
    /// <summary>
    /// Log em arquivo (logs/registros.log). Cada evento ocupa UMA linha, gravada sempre no final do arquivo:
    /// [2026-11-27 13:24:51] [CADASTRO] Nome: Maria | Telefone: 11912345678 | E-mail: ... | Senha: ********
    /// </summary>
    public interface IRegistroLog
    {
        /// <summary>Acrescenta uma linha ao final do arquivo de log.</summary>
        Task RegistrarAsync(string evento, params (string Campo, string? Valor)[] campos);

        /// <summary>Devolve as últimas linhas do log (da mais antiga para a mais recente).</summary>
        Task<IReadOnlyList<string>> LerUltimasLinhasAsync(int quantidade = 1000);

        /// <summary>Caminho completo do arquivo de log.</summary>
        string CaminhoArquivo { get; }
    }

    public class RegistroLogArquivo : IRegistroLog
    {
        // Um único "porteiro" para o processo inteiro: várias requisições gravando ao mesmo tempo
        // não embaralham as linhas nem causam erro de arquivo em uso.
        private static readonly SemaphoreSlim _trava = new(1, 1);
        private static readonly Encoding _utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private readonly ILogger<RegistroLogArquivo> _logger;

        public string CaminhoArquivo { get; }

        public RegistroLogArquivo(IWebHostEnvironment env, ILogger<RegistroLogArquivo> logger)
        {
            _logger = logger;

            var pasta = Path.Combine(env.ContentRootPath, "logs");
            Directory.CreateDirectory(pasta);
            CaminhoArquivo = Path.Combine(pasta, "registros.log");
        }

        /// <summary>Tira quebras de linha (evita "forjar" linhas falsas no log) e limita o tamanho.</summary>
        public static string Limpar(string? valor, int max = 200)
        {
            if (string.IsNullOrWhiteSpace(valor)) return "-";

            var sb = new StringBuilder(valor.Length);
            foreach (var c in valor)
                sb.Append(char.IsControl(c) ? ' ' : c);

            var texto = sb.ToString().Replace(" | ", " / ").Trim();
            return texto.Length > max ? texto[..max] + "..." : texto;
        }

        public async Task RegistrarAsync(string evento, params (string Campo, string? Valor)[] campos)
        {
            var linha = new StringBuilder();
            linha.Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("] ");
            linha.Append('[').Append(Limpar(evento, 40)).Append(']');

            for (var i = 0; i < campos.Length; i++)
            {
                linha.Append(i == 0 ? " " : " | ");
                linha.Append(Limpar(campos[i].Campo, 40)).Append(": ").Append(Limpar(campos[i].Valor));
            }
            linha.Append('\n');

            await _trava.WaitAsync();
            try
            {
                // AppendAllTextAsync = grava SEMPRE ao final do arquivo (e cria o arquivo se não existir)
                await File.AppendAllTextAsync(CaminhoArquivo, linha.ToString(), _utf8);
            }
            catch (Exception ex)
            {
                // Falha no log nunca pode derrubar o cadastro/login do usuário
                _logger.LogError(ex, "Não foi possível gravar no arquivo de log {Caminho}", CaminhoArquivo);
            }
            finally
            {
                _trava.Release();
            }
        }

        public async Task<IReadOnlyList<string>> LerUltimasLinhasAsync(int quantidade = 1000)
        {
            await _trava.WaitAsync();
            try
            {
                if (!File.Exists(CaminhoArquivo)) return Array.Empty<string>();

                var linhas = await File.ReadAllLinesAsync(CaminhoArquivo, _utf8);
                return linhas.Length <= quantidade ? linhas : linhas[^quantidade..];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Não foi possível ler o arquivo de log {Caminho}", CaminhoArquivo);
                return Array.Empty<string>();
            }
            finally
            {
                _trava.Release();
            }
        }
    }
}
