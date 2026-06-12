using ApiClinica.DTOs;

namespace ApiClinica.Services;

public interface IMedicoService
{
    Task<IEnumerable<MedicoReadDTO>> GetAll();
    Task<MedicoReadDTO> GetById(int id);
    Task<MedicoReadDTO> Create(MedicoCreateDTO dto);
    Task<MedicoReadDTO> Update(int id, MedicoUpdateDTO dto);
    Task Delete(int id);
}