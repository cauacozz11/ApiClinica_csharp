namespace ApiClinica.DTOs;

// DTO usado no PATCH: todos os campos são opcionais
// Se PacienteId ou MedicoId for alterado, todas as validações de conflito são refeitas
public class ConsultaUpdateDTO
{
    public int? PacienteId { get; set; }
    public int? MedicoId { get; set; }
    public DateTime? DataHora { get; set; }
}