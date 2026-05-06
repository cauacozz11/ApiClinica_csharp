namespace ApiClinica.Models;

// Modelo que representa a tabela "Medicos" no banco de dados
public class Medico
{
    // EF Core usa "Id" como chave primária por convenção (auto-incremento)
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Telefone { get; set; }
    public required string CRM { get; set; }

    // Propriedade de navegação: permite acessar as consultas do médico via EF Core
    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
}