namespace ApiClinica.Models;

// Modelo que representa a tabela "Consultas" no banco de dados
public class Consulta
{
    public int Id { get; set; }

    // Chaves estrangeiras: referenciam Paciente e Medico pelo Id
    public int PacienteId { get; set; }
    public int MedicoId { get; set; }

    // Propriedades de navegação: permitem acessar os objetos relacionados diretamente
    // "null!" informa ao compilador que serão preenchidas pelo EF Core (não serão nulas em uso)
    public Paciente Paciente { get; set; } = null!;
    public Medico Medico { get; set; } = null!;

    public DateTime DataHora { get; set; }
}