using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Interfaces;

public interface IConsultaMapper
{
    ConsultaReadDTO ToReadDTO(Consulta consulta);
    Consulta ToModel(ConsultaCreateDTO dto);
}