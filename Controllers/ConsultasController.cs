using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Mappers;
using ApiClinica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")] // Rota base: api/Consultas
public class ConsultasController : ControllerBase
{
    // AppDbContext injetado pelo ASP.NET Core (configurado no Program.cs)
    private readonly AppDbContext _context;

    public ConsultasController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/Consultas — retorna todas as consultas como lista de ReadDTOs
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var consultas = await _context.Consultas.ToListAsync();
        return Ok(consultas.Select(ConsultaMapper.ToReadDTO));
    }

    // GET api/Consultas/{id} — retorna uma consulta específica ou 404
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null) return NotFound();
        return Ok(ConsultaMapper.ToReadDTO(consulta));
    }

    // POST api/Consultas — agenda uma nova consulta após validações de conflito
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConsultaCreateDTO dto)
    {
        // Verifica se paciente e médico existem no banco antes de continuar
        if (!await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId))
            return BadRequest(new { mensagem = "Paciente não encontrado." });

        if (!await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId))
            return BadRequest(new { mensagem = "Médico não encontrado." });

        if (dto.DataHora <= DateTime.Now)
            return BadRequest(new { mensagem = "A data/hora da consulta não pode ser no passado." });

        // Busca todas as consultas existentes do médico para checar sobreposição
        var consultasMedico = await _context.Consultas
            .Where(c => c.MedicoId == dto.MedicoId)
            .ToListAsync();

        if (TemSobreposicao(dto.DataHora, consultasMedico))
            return BadRequest(new { mensagem = "O médico já possui uma consulta neste horário." });

        // Busca todas as consultas existentes do paciente para checar sobreposição
        var consultasPaciente = await _context.Consultas
            .Where(c => c.PacienteId == dto.PacienteId)
            .ToListAsync();

        if (TemSobreposicao(dto.DataHora, consultasPaciente))
            return BadRequest(new { mensagem = "O paciente já possui uma consulta neste horário." });

        var consulta = ConsultaMapper.ToModel(dto);
        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();

        // 201 Created com o header Location apontando para o novo recurso
        return CreatedAtAction(nameof(GetById), new { id = consulta.Id }, ConsultaMapper.ToReadDTO(consulta));
    }

    // PATCH api/Consultas/{id} — atualiza apenas os campos enviados e revalida conflitos
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ConsultaUpdateDTO dto)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null) return NotFound();

        // Usa o valor enviado ou mantém o atual caso o campo não tenha sido enviado
        var novaDataHora = dto.DataHora ?? consulta.DataHora;
        var novoPacienteId = dto.PacienteId ?? consulta.PacienteId;
        var novoMedicoId = dto.MedicoId ?? consulta.MedicoId;

        // Valida os IDs apenas se foram alterados
        if (dto.PacienteId.HasValue && !await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId.Value))
            return BadRequest(new { mensagem = "Paciente não encontrado." });

        if (dto.MedicoId.HasValue && !await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId.Value))
            return BadRequest(new { mensagem = "Médico não encontrado." });

        if (novaDataHora <= DateTime.Now)
            return BadRequest(new { mensagem = "A data/hora da consulta não pode ser no passado." });

        // Exclui a própria consulta da verificação para não bloquear atualização sem mudança de horário
        var consultasMedico = await _context.Consultas
            .Where(c => c.MedicoId == novoMedicoId && c.Id != id)
            .ToListAsync();

        if (TemSobreposicao(novaDataHora, consultasMedico))
            return BadRequest(new { mensagem = "O médico já possui uma consulta neste horário." });

        var consultasPaciente = await _context.Consultas
            .Where(c => c.PacienteId == novoPacienteId && c.Id != id)
            .ToListAsync();

        if (TemSobreposicao(novaDataHora, consultasPaciente))
            return BadRequest(new { mensagem = "O paciente já possui uma consulta neste horário." });

        consulta.DataHora = novaDataHora;
        consulta.PacienteId = novoPacienteId;
        consulta.MedicoId = novoMedicoId;

        await _context.SaveChangesAsync();
        return Ok(ConsultaMapper.ToReadDTO(consulta));
    }

    // DELETE api/Consultas/{id} — cancela uma consulta
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null) return NotFound();

        _context.Consultas.Remove(consulta);
        await _context.SaveChangesAsync();
        return NoContent(); // 204: sucesso sem corpo de resposta
    }

    // Verifica se o novo horário sobrepõe alguma consulta existente
    // Cada consulta dura 30 minutos: sobreposição ocorre quando os intervalos se cruzam
    // Condição: novaInício < existenteFim  E  novaFim > existenteInício
    private static bool TemSobreposicao(DateTime novaDataHora, IEnumerable<Consulta> consultas)
    {
        var novaFim = novaDataHora.AddMinutes(30);
        return consultas.Any(c => novaDataHora < c.DataHora.AddMinutes(30) && novaFim > c.DataHora);
    }
}