using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Interfaces;
using ApiClinica.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Services;

public class ConsultaService : IConsultaService
{
    private readonly AppDbContext _context;

    private readonly IConsultaMapper _mapper;

    public ConsultaService(AppDbContext context, IConsultaMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ConsultaReadDTO>> GetAll()
    {
        var consultas = await _context.Consultas.ToListAsync();
        return consultas.Select(_mapper.ToReadDTO);
    }

    public async Task<ConsultaReadDTO> GetById(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null) throw new Exception("Consulta não encontrada.");
        return _mapper.ToReadDTO(consulta);
    }

    public async Task<ConsultaReadDTO> Create(ConsultaCreateDTO dto)
    {
        if (!await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId))
            throw new Exception("Paciente não encontrado.");

        if (!await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId))
            throw new Exception("Médico não encontrado.");

        if (dto.DataHora <= DateTime.Now)
            throw new Exception("A data/hora da consulta não pode ser no passado.");

        var consultasMedico = await _context.Consultas
            .Where(c => c.MedicoId == dto.MedicoId)
            .ToListAsync();

        if (TemSobreposicao(dto.DataHora, consultasMedico))
            throw new Exception("O médico já possui uma consulta neste horário.");

        var consultasPaciente = await _context.Consultas
            .Where(c => c.PacienteId == dto.PacienteId)
            .ToListAsync();

        if (TemSobreposicao(dto.DataHora, consultasPaciente))
            throw new Exception("O paciente já possui uma consulta neste horário.");

        var consulta = _mapper.ToModel(dto);
        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();
        return _mapper.ToReadDTO(consulta);
    }
    public async Task<ConsultaReadDTO> Update(int id, ConsultaUpdateDTO dto)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null) throw new Exception("Consulta não encontrada.");

        var novaDataHora = dto.DataHora ?? consulta.DataHora;
        var novoPacienteId = dto.PacienteId ?? consulta.PacienteId;
        var novoMedicoId = dto.MedicoId ?? consulta.MedicoId;

        if (dto.PacienteId.HasValue && !await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId.Value))
            throw new Exception("Paciente não encontrado.");

        if (dto.MedicoId.HasValue && !await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId.Value))
         throw new Exception("Médico não encontrado.");

        if (novaDataHora <= DateTime.Now)
            throw new Exception("A data/hora da consulta não pode ser no passado.");

        var consultasMedico = await _context.Consultas
            .Where(c => c.MedicoId == novoMedicoId && c.Id != id)
            .ToListAsync();

        if (TemSobreposicao(novaDataHora, consultasMedico))
            throw new Exception("O médico já possui uma consulta neste horário.");

        var consultasPaciente = await _context.Consultas
        .Where(c => c.PacienteId == novoPacienteId && c.Id != id)
        .ToListAsync();

        if (TemSobreposicao(novaDataHora, consultasPaciente))
            throw new Exception("O paciente já possui uma consulta neste horário.");

        consulta.DataHora = novaDataHora;
        consulta.PacienteId = novoPacienteId;
        consulta.MedicoId = novoMedicoId;

        await _context.SaveChangesAsync();
        return _mapper.ToReadDTO(consulta);
    }

    public async Task Delete(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null) throw new Exception("Consulta não encontrada.");

        _context.Consultas.Remove(consulta);
        await _context.SaveChangesAsync();
    }

    private static bool TemSobreposicao(DateTime novaDataHora, IEnumerable<Consulta> consultas)
    {
        var novaFim = novaDataHora.AddMinutes(30);
        return consultas.Any(c => novaDataHora < c.DataHora.AddMinutes(30) && novaFim > c.DataHora);
    }
}