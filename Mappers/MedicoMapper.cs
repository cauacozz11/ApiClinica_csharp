using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Mappers;

// Mapper manual: converte entre Model (banco) e DTOs (entrada/saída da API)
public static class MedicoMapper
{
    // Converte Model → ReadDTO (usado nas respostas GET)
    public static MedicoReadDTO ToReadDTO(Medico medico) => new()
    {
        Id = medico.Id,
        Nome = medico.Nome,
        Email = medico.Email,
        Telefone = medico.Telefone,
        CRM = medico.CRM
    };

    // Converte CreateDTO → Model (usado no POST antes de salvar no banco)
    public static Medico ToModel(MedicoCreateDTO dto) => new()
    {
        Nome = dto.Nome,
        Email = dto.Email,
        Telefone = dto.Telefone,
        CRM = dto.CRM
    };
}