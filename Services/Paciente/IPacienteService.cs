using ApiClinica.DTOs;

namespace ApiClinica.Services;

public interface IPacienteService 
{
    Task<IEnumerable<PacienteReadDTO>> GetAll();
    Task<PacienteReadDTO> GetById(int id);
    Task<PacienteReadDTO> Create(PacienteCreateDTO dto);
    Task<PacienteReadDTO> Update(int id, PacienteUpdateDTO dto);
    Task Delete(int id);
}