namespace ApiClinica.DTOs;

// DTO usado nas respostas GET: representa os dados de uma consulta retornados pela API
public class ConsultaReadDTO
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public int MedicoId { get; set; }
    public DateTime DataHora { get; set; }
}