using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Mappers;

// Mapper manual: converte entre Model (banco) e DTOs (entrada/saída da API)
public static class ConsultaMapper
{
    // Converte Model → ReadDTO (usado nas respostas GET)
    public static ConsultaReadDTO ToReadDTO(Consulta consulta) => new()
    {
        Id = consulta.Id,
        PacienteId = consulta.PacienteId,
        MedicoId = consulta.MedicoId,
        DataHora = consulta.DataHora
    };

    // Converte CreateDTO → Model (usado no POST antes de salvar no banco)
    public static Consulta ToModel(ConsultaCreateDTO dto) => new()
    {
        PacienteId = dto.PacienteId,
        MedicoId = dto.MedicoId,
        DataHora = dto.DataHora
    };
}