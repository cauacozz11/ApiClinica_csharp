using ApiClinica.DTOs;
using ApiClinica.Models;
using ApiClinica.Interfaces;

namespace ApiClinica.Mappers;

public class PacienteMapper : IPacienteMapper
{
    
    public PacienteReadDTO ToReadDTO(Paciente paciente) => new()
    {
        Id = paciente.Id,
        Nome = paciente.Nome,
        Email = paciente.Email,
        Telefone = paciente.Telefone,
        DataNasc = paciente.DataNasc,
        Cpf = paciente.Cpf
    };

    public Paciente ToModel(PacienteCreateDTO dto) => new()
    {
        Nome = dto.Nome,
        Email = dto.Email,
        Telefone = dto.Telefone,
        DataNasc = dto.DataNasc,
        Cpf = dto.Cpf
    };
}