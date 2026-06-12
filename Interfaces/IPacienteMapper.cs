using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Interfaces;

public interface IPacienteMapper
{
    PacienteReadDTO ToReadDTO(Paciente paciente);
    Paciente ToModel(PacienteCreateDTO dto);
}