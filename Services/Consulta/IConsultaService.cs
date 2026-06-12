using ApiClinica.DTOs;

namespace ApiClinica.Services;

public interface IConsultaService 
{
    Task<IEnumerable<ConsultaReadDTO>> GetAll();
    Task<ConsultaReadDTO> GetById(int id);
    Task<ConsultaReadDTO> Create(ConsultaCreateDTO dto);
    Task<ConsultaReadDTO> Update(int id, ConsultaUpdateDTO dto);
    Task Delete(int id);
}