using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ApiClinica.Services;

public class PacienteService : IPacienteService
{
    private readonly AppDbContext _context;

    private readonly IPacienteMapper _mapper;

    public PacienteService(AppDbContext context, IPacienteMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PacienteReadDTO>> GetAll()
    {
        var pacientes = await _context.Pacientes.ToListAsync();
        return pacientes.Select(_mapper.ToReadDTO);
    }

    public async Task<PacienteReadDTO> GetById(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) throw new Exception("Paciente não encontrado.");
        return _mapper.ToReadDTO(paciente);
    }

    public async Task<PacienteReadDTO> Create(PacienteCreateDTO dto)
    {
        if (!ValidarEmail(dto.Email))
            throw new Exception("Email inválido.");

        if (!ValidarTelefone(dto.Telefone))
            throw new Exception("Telefone inválido.");

        if (dto.DataNasc > DateOnly.FromDateTime(DateTime.Today))
            throw new Exception("Data de nascimento não pode ser futura.");

        if (!ValidarCpf(dto.Cpf))
            throw new Exception("CPF inválido.");

        var cpfDigits = new string(dto.Cpf.Where(char.IsDigit).ToArray());

        if (await _context.Pacientes.AnyAsync(p => p.Cpf == cpfDigits))
            throw new Exception("Já existe um paciente com este CPF.");

        var paciente = _mapper.ToModel(dto);
        paciente.Cpf = cpfDigits;
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();
        return _mapper.ToReadDTO(paciente);
    }

    public async Task<PacienteReadDTO> Update(int id, PacienteUpdateDTO dto)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) throw new Exception("Paciente não encontrado.");

        if (dto.Email != null)
        {
            if (!ValidarEmail(dto.Email))
                throw new Exception("Email inválido.");
            paciente.Email = dto.Email;
        }

        if (dto.Telefone != null)
        {
            if (!ValidarTelefone(dto.Telefone))
                throw new Exception("Telefone inválido.");
            paciente.Telefone = dto.Telefone;
        }

        if (dto.DataNasc.HasValue)
        {
            if (dto.DataNasc.Value > DateOnly.FromDateTime(DateTime.Today))
                throw new Exception("Data de nascimento não pode ser futura.");
            paciente.DataNasc = dto.DataNasc.Value;
        }

        if (dto.Nome != null)
            paciente.Nome = dto.Nome;

        await _context.SaveChangesAsync();
        return _mapper.ToReadDTO(paciente);
    }

    public async Task Delete(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) throw new Exception("Paciente não encontrado.");

        var temConsultasFuturas = await _context.Consultas
            .AnyAsync(c => c.PacienteId == id && c.DataHora > DateTime.Now);

        if (temConsultasFuturas)
            throw new Exception("Não é possível excluir o paciente pois ele possui consultas futuras.");

        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();
    }

    private static bool ValidarCpf(string cpf)
    {
        var digits = new string(cpf.Where(char.IsDigit).ToArray());

        return Regex.IsMatch(digits, @"^\d{11}$") && !Regex.IsMatch(digits, @"^0{11}$");
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