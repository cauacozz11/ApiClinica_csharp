using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Interfaces;

public interface IMedicoMapper
{
    MedicoReadDTO ToReadDTO(Medico medico);
    Medico ToModel(MedicoCreateDTO dto);
}