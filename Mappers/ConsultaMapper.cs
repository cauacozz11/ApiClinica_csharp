using ApiClinica.DTOs;
using ApiClinica.Models;
using ApiClinica.Interfaces;

namespace ApiClinica.Mappers;

public class ConsultaMapper : IConsultaMapper
{

    public ConsultaReadDTO ToReadDTO(Consulta consulta) => new()
    {
        Id = consulta.Id,
        PacienteId = consulta.PacienteId,
        MedicoId = consulta.MedicoId,
        DataHora = consulta.DataHora
    };


    public Consulta ToModel(ConsultaCreateDTO dto) => new()
    {
        PacienteId = dto.PacienteId,
        MedicoId = dto.MedicoId,
        DataHora = dto.DataHora
    };
}