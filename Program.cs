using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;

class UserAdv
{
    public string NomeComp { get; set; }
    public int Idade { get; set; }
    public string InscricaoOAB { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
    public string Estado { get; set; }
    public List<string> Formacao { get; set; }
    public string TempoExperiencia { get; set; }
    public List<string> AreasAtuacao { get; set; }
    public List<string> Idiomas { get; set; }
}

class UserEsc
{
    public int EscritorioId { get; set; }
    public string NomeEsc { get; set; }
    public string Cnpj { get; set; }
    public string Estado { get; set; }
    public string Endereco { get; set; } 
    public string Telefone { get; set; }
    public string SociOAB { get; set; }
    public List<string> PrefAreas { get; set; }
    public List<string> IdiomasEx { get; set; }
    public string MinExperiencia { get; set; }
}

class Program
{
    static string connectionString =
        @"Server=DESKTOP-9IDITBH\SQLEXPRESS;Database=LexorDB;Integrated Security=True;";

    static void Main(string[] args)
    {
        List<UserEsc> escritorios = CarregarEscritoriosDoBanco();

        UserAdv novoAdvogado = new UserAdv();

        Console.Write("Insira seu Nome: ");
        novoAdvogado.NomeComp = Console.ReadLine();

        Console.Write("Insira sua idade: ");
        novoAdvogado.Idade = int.Parse(Console.ReadLine());

        Console.Write("Insira seu número da OAB: ");
        novoAdvogado.InscricaoOAB = Console.ReadLine();

        Console.Write("Informe seu tempo de experiência (ex: 2 anos, 6 meses): ");
        novoAdvogado.TempoExperiencia = Console.ReadLine();

        Console.Write("Insira o telefone: ");
        novoAdvogado.Telefone = Console.ReadLine();

        Console.Write("Insira o e-mail: ");
        novoAdvogado.Email = Console.ReadLine();

        Console.Write("Informe o estado que você mora (ex: SP, RJ): ");
        novoAdvogado.Estado = Console.ReadLine().ToUpper().Trim();

        Console.Write("Informe as áreas de atuação (separe por vírgula): ");
        novoAdvogado.AreasAtuacao = Console.ReadLine()
            .Split(',')
            .Select(a => a.Trim())
            .Where(a => !string.IsNullOrEmpty(a))
            .ToList();

        Console.Write("Informe os idiomas que domina (separe por vírgula): ");
        novoAdvogado.Idiomas = Console.ReadLine()
            .Split(',')
            .Select(i => i.Trim())
            .Where(i => !string.IsNullOrEmpty(i))
            .ToList();

        SalvarUsuarioNoBanco(novoAdvogado);

        var recomendacoes = RecomendarEscritorios(novoAdvogado, escritorios);

        Console.WriteLine("\n * Recomendações para você: *\n");

        if (recomendacoes.Count == 0)
        {
            Console.WriteLine("Nenhum escritório com suas preferências foi encontrado.");
        }
        else
        {
            foreach (var r in recomendacoes)
            {
                Console.WriteLine($"{r.NomeEsc}");
                Console.WriteLine($"Estado: {r.Estado}");
                Console.WriteLine($"Tel: {r.Telefone}");
                Console.WriteLine($"Áreas: {string.Join(", ", r.PrefAreas)}");
                Console.WriteLine($"Idiomas: {(r.IdiomasEx.Any(i => !string.IsNullOrEmpty(i))
                    ? string.Join(", ", r.IdiomasEx.Where(i => !string.IsNullOrEmpty(i)))
                    : "Não especificado")}");
                Console.WriteLine($"Experiência mínima: {(string.IsNullOrEmpty(r.MinExperiencia) ? "Não especificada" : r.MinExperiencia)}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("Pressione ENTER para sair...");
        Console.ReadLine();
    }

    static List<UserEsc> CarregarEscritoriosDoBanco()
    {
        var lista = new List<UserEsc>();

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string sql = @"SELECT EscritorioId, NomeEsc, Cnpj, Estado, Telefone,
                                  SociOAB, PrefAreas, IdiomasEx, MinExperiencia
                           FROM dbo.Escritorios";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var esc = new UserEsc
                        {
                            EscritorioId = r.GetInt32(r.GetOrdinal("EscritorioId")),
                            NomeEsc = r["NomeEsc"].ToString(),
                            Cnpj = r["Cnpj"].ToString(),
                            Estado = r["Estado"].ToString(),
                            Telefone = r["Telefone"].ToString(),
                            SociOAB = r["SociOAB"].ToString(),
                            PrefAreas = r["PrefAreas"].ToString()
                                .Split(',')
                                .Select(a => a.Trim())
                                .Where(a => !string.IsNullOrEmpty(a))
                                .ToList(),
                            IdiomasEx = r["IdiomasEx"].ToString()
                                .Split(',')
                                .Select(i => i.Trim())
                                .Where(i => !string.IsNullOrEmpty(i))
                                .ToList(),
                            MinExperiencia = r["MinExperiencia"].ToString()
                        };

                        lista.Add(esc);
                    }
                }
            }
        }

        return lista;
    }

    static void SalvarUsuarioNoBanco(UserAdv u)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string sql = @"INSERT INTO dbo.Usuarios
                           (NomeComp, Idade, InscricaoOAB, Telefone, Email,
                            Estado, Formacao, TempoExperiencia, AreasAtuacao, Idiomas)
                           VALUES
                           (@NomeComp, @Idade, @InscricaoOAB, @Telefone, @Email,
                            @Estado, @Formacao, @TempoExperiencia, @AreasAtuacao, @Idiomas)";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@NomeComp", u.NomeComp);
                cmd.Parameters.AddWithValue("@Idade", u.Idade);
                cmd.Parameters.AddWithValue("@InscricaoOAB", u.InscricaoOAB ?? "");
                cmd.Parameters.AddWithValue("@Telefone", u.Telefone ?? "");
                cmd.Parameters.AddWithValue("@Email", u.Email ?? "");
                cmd.Parameters.AddWithValue("@Estado", u.Estado ?? "");
                cmd.Parameters.AddWithValue("@Formacao", u.Formacao != null ? string.Join(", ", u.Formacao) : "");
                cmd.Parameters.AddWithValue("@TempoExperiencia", u.TempoExperiencia ?? "");
                cmd.Parameters.AddWithValue("@AreasAtuacao", u.AreasAtuacao != null ? string.Join(", ", u.AreasAtuacao) : "");
                cmd.Parameters.AddWithValue("@Idiomas", u.Idiomas != null ? string.Join(", ", u.Idiomas) : "");

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
    static List<UserEsc> RecomendarEscritorios(UserAdv advogado, List<UserEsc> escritorios)
    {
        return escritorios
            .Select(e => new
            {
                Escritorio = e,
                Pontuacao = CalcularPontuacao(advogado, e),
                CompatibilidadeAreas = CalcularCompatibilidadeAreas(advogado, e),
                CompatibilidadeExperiencia = VerificarExperiencia(advogado, e),
                CompatibilidadeEstado = VerificarEstado(advogado, e),
                CompatibilidadeIdiomas = CalcularCompatibilidadeIdiomas(advogado, e)
            })
            .Where(x => x.Pontuacao >= 15)
            .OrderByDescending(x => x.Pontuacao)
            .ThenByDescending(x => x.CompatibilidadeAreas)
            .ThenByDescending(x => x.CompatibilidadeExperiencia)
            .Select(x => x.Escritorio)
            .ToList();
    }

    static int CalcularPontuacao(UserAdv advogado, UserEsc escritorio)
    {
        int pontuacao = 0;
        pontuacao += CalcularCompatibilidadeAreas(advogado, escritorio) * 3;
        pontuacao += VerificarEstado(advogado, escritorio) * 2;
        pontuacao += CalcularCompatibilidadeIdiomas(advogado, escritorio) * 2;
        pontuacao += VerificarExperiencia(advogado, escritorio) * 3;

        if (string.IsNullOrEmpty(escritorio.MinExperiencia))
            pontuacao += 5;

        return pontuacao;
    }

    static int CalcularCompatibilidadeAreas(UserAdv advogado, UserEsc escritorio)
    {
        if (advogado.AreasAtuacao == null || !advogado.AreasAtuacao.Any() ||
            escritorio.PrefAreas == null || !escritorio.PrefAreas.Any())
            return 0;

        var areasComuns = advogado.AreasAtuacao
            .Select(a => a.ToLower().Trim())
            .Intersect(escritorio.PrefAreas
                .Select(p => p.ToLower().Trim()))
            .Count();

        if (areasComuns == advogado.AreasAtuacao.Count)
            areasComuns += 2;

        return areasComuns;
    }

    static int VerificarEstado(UserAdv advogado, UserEsc escritorio)
    {
        if (string.IsNullOrEmpty(advogado.Estado) || string.IsNullOrEmpty(escritorio.Estado))
            return 0;

        return advogado.Estado.Equals(escritorio.Estado, StringComparison.OrdinalIgnoreCase) ? 3 : 0;
    }

    static int CalcularCompatibilidadeIdiomas(UserAdv advogado, UserEsc escritorio)
    {
        if (advogado.Idiomas == null || !advogado.Idiomas.Any())
            return 0;

        var idiomasEscritorio = escritorio.IdiomasEx ?? new List<string>();

        if (!idiomasEscritorio.Any() || idiomasEscritorio.All(string.IsNullOrEmpty))
            return 1;

        var idiomasComuns = advogado.Idiomas
            .Select(i => i.ToLower().Trim())
            .Intersect(idiomasEscritorio
                .Where(i => !string.IsNullOrEmpty(i))
                .Select(i => i.ToLower().Trim()))
            .Count();

        return idiomasComuns;
    }

    static int VerificarExperiencia(UserAdv advogado, UserEsc escritorio)
    {
        if (string.IsNullOrEmpty(escritorio.MinExperiencia))
            return 2;

        if (string.IsNullOrEmpty(advogado.TempoExperiencia))
            return 0;

        try
        {
            var expAdvogado = ConverterExperienciaParaMeses(advogado.TempoExperiencia);
            var expEscritorio = ConverterExperienciaParaMeses(escritorio.MinExperiencia);

            if (expAdvogado >= expEscritorio)
                return 3;
            else if (expAdvogado >= expEscritorio * 0.5)
                return 1;
            else
                return 0;
        }
        catch
        {
            return advogado.TempoExperiencia.ToLower()
                .Contains(escritorio.MinExperiencia.ToLower()) ? 1 : 0;
        }
    }

    static double ConverterExperienciaParaMeses(string experiencia)
    {
        if (string.IsNullOrEmpty(experiencia))
            return 0;

        experiencia = experiencia.ToLower();
        var partes = experiencia.Split(' ');

        if (partes.Length < 2) return 0;

        if (double.TryParse(partes[0], out double valor))
        {
            if (experiencia.Contains("ano"))
                return valor * 12;
            else if (experiencia.Contains("mês") || experiencia.Contains("mes"))
                return valor;
        }

        return 0;
    }
}
