using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Services;

public class MedicoService : IMedicoService
{
    private readonly AppDbContext _context;

    private readonly IMedicoMapper _mapper;

    public MedicoService(AppDbContext context, IMedicoMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MedicoReadDTO>> GetAll()
    {
        var medicos = await _context.Medicos.ToListAsync();
        return medicos.Select(_mapper.ToReadDTO);
    }

    public async Task<MedicoReadDTO> GetById(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) throw new Exception("Médico não encontrado.");
        return _mapper.ToReadDTO(medico);
    }

    public async Task<MedicoReadDTO> Create(MedicoCreateDTO dto)
    {
        if (!ValidarEmail(dto.Email))
            throw new Exception("Email inválido.");

        if (!ValidarTelefone(dto.Telefone))
            throw new Exception("Telefone inválido.");

        var medico = _mapper.ToModel(dto);
        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();
        return _mapper.ToReadDTO(medico);
    }

    public async Task<MedicoReadDTO> Update(int id, MedicoUpdateDTO dto)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) throw new Exception("Médico não encontrado.");

        if (dto.Email != null)
        {
            if (!ValidarEmail(dto.Email))
                throw new Exception("Email inválido.");
            medico.Email = dto.Email;
        }

        if (dto.Telefone != null)
        {
            if (!ValidarTelefone(dto.Telefone))
                throw new Exception("Telefone inválido.");
            medico.Telefone = dto.Telefone;
        }

        if (dto.Nome != null) medico.Nome = dto.Nome;
        if (dto.CRM != null) medico.CRM = dto.CRM;

        await _context.SaveChangesAsync();
        return _mapper.ToReadDTO(medico);
    }

    public async Task Delete(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) throw new Exception("Médico não encontrado.");

        var temConsultasFuturas = await _context.Consultas
            .AnyAsync(c => c.MedicoId == id && c.DataHora > DateTime.Now);

        if (temConsultasFuturas)
            throw new Exception("Não é possível excluir o médico pois ele possui consultas futuras."); 

        _context.Medicos.Remove(medico);
        await _context.SaveChangesAsync();
        
    }

    private static bool ValidarEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }

    private static bool ValidarTelefone(string telefone)
    {
        var digits = new string(telefone.Where(char.IsDigit).ToArray());
        return digits.Length == 10 || digits.Length == 11;
    }
}