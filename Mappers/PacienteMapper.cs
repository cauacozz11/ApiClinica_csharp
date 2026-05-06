using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Mappers;

// Mapper manual: converte entre Model (banco) e DTOs (entrada/saída da API)
// Evita expor o Model diretamente e mantém controle total sobre os campos trafegados
public static class PacienteMapper
{
    // Converte Model → ReadDTO (usado nas respostas GET)
    public static PacienteReadDTO ToReadDTO(Paciente paciente) => new()
    {
        Id = paciente.Id,
        Nome = paciente.Nome,
        Email = paciente.Email,
        Telefone = paciente.Telefone,
        DataNasc = paciente.DataNasc,
        Cpf = paciente.Cpf
    };

    // Converte CreateDTO → Model (usado no POST antes de salvar no banco)
    public static Paciente ToModel(PacienteCreateDTO dto) => new()
    {
        Nome = dto.Nome,
        Email = dto.Email,
        Telefone = dto.Telefone,
        DataNasc = dto.DataNasc,
        Cpf = dto.Cpf
    };
}