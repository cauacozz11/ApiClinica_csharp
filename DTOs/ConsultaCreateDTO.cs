namespace ApiClinica.DTOs;

// DTO usado no POST: o cliente informa apenas os IDs e o horário para agendar uma consulta
public class ConsultaCreateDTO
{
    public int PacienteId { get; set; }
    public int MedicoId { get; set; }
    public DateTime DataHora { get; set; }
}