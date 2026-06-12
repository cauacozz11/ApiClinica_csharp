using ApiClinica.DTOs;
using ApiClinica.Models;
using ApiClinica.Interfaces;

namespace ApiClinica.Mappers;


public class MedicoMapper : IMedicoMapper
{

    public MedicoReadDTO ToReadDTO(Medico medico) => new()
    {
        Id = medico.Id,
        Nome = medico.Nome,
        Email = medico.Email,
        Telefone = medico.Telefone,
        CRM = medico.CRM
    };


    public Medico ToModel(MedicoCreateDTO dto) => new()
    {
        Nome = dto.Nome,
        Email = dto.Email,
        Telefone = dto.Telefone,
        CRM = dto.CRM
    };
}