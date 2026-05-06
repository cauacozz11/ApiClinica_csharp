using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")] // Rota base: api/Medicos
public class MedicosController : ControllerBase
{
    // AppDbContext injetado pelo ASP.NET Core (configurado no Program.cs)
    private readonly AppDbContext _context;

    public MedicosController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/Medicos — retorna todos os médicos como lista de ReadDTOs
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var medicos = await _context.Medicos.ToListAsync();
        return Ok(medicos.Select(MedicoMapper.ToReadDTO));
    }

    // GET api/Medicos/{id} — retorna um médico específico ou 404
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) return NotFound();
        return Ok(MedicoMapper.ToReadDTO(medico));
    }

    // POST api/Medicos — cria um novo médico após validações
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MedicoCreateDTO dto)
    {
        if (!ValidarEmail(dto.Email))
            return BadRequest(new { mensagem = "Email inválido." });

        if (!ValidarTelefone(dto.Telefone))
            return BadRequest(new { mensagem = "Telefone inválido." });

        var medico = MedicoMapper.ToModel(dto);
        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();

        // 201 Created com o header Location apontando para o novo recurso
        return CreatedAtAction(nameof(GetById), new { id = medico.Id }, MedicoMapper.ToReadDTO(medico));
    }

    // PATCH api/Medicos/{id} — atualiza apenas os campos enviados (não-nulos)
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MedicoUpdateDTO dto)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) return NotFound();

        // Cada campo só é atualizado se vier preenchido na requisição
        if (dto.Email != null)
        {
            if (!ValidarEmail(dto.Email))
                return BadRequest(new { mensagem = "Email inválido." });
            medico.Email = dto.Email;
        }

        if (dto.Telefone != null)
        {
            if (!ValidarTelefone(dto.Telefone))
                return BadRequest(new { mensagem = "Telefone inválido." });
            medico.Telefone = dto.Telefone;
        }

        if (dto.Nome != null) medico.Nome = dto.Nome;
        if (dto.CRM != null) medico.CRM = dto.CRM;

        await _context.SaveChangesAsync();
        return Ok(MedicoMapper.ToReadDTO(medico));
    }

    // DELETE api/Medicos/{id} — remove o médico se não tiver consultas futuras
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) return NotFound();

        // Impede exclusão se existir ao menos uma consulta agendada no futuro
        var temConsultasFuturas = await _context.Consultas
            .AnyAsync(c => c.MedicoId == id && c.DataHora > DateTime.Now);

        if (temConsultasFuturas)
            return BadRequest(new { mensagem = "Não é possível excluir o médico pois ele possui consultas futuras." });

        _context.Medicos.Remove(medico);
        await _context.SaveChangesAsync();
        return NoContent(); // 204: sucesso sem corpo de resposta
    }

    // Usa a classe MailAddress do .NET para validar o formato do e-mail
    private static bool ValidarEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }

    // Telefone válido: 10 dígitos (fixo) ou 11 dígitos (celular com 9)
    private static bool ValidarTelefone(string telefone)
    {
        var digits = new string(telefone.Where(char.IsDigit).ToArray());
        return digits.Length == 10 || digits.Length == 11;
    }
}