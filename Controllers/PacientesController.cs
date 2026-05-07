using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")] // Rota base: api/Pacientes
public class PacientesController : ControllerBase
{
    // AppDbContext injetado pelo ASP.NET Core (configurado no Program.cs)
    private readonly AppDbContext _context;

    public PacientesController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/Pacientes — retorna todos os pacientes como lista de ReadDTOs
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _context.Pacientes.ToListAsync();
        return Ok(pacientes.Select(PacienteMapper.ToReadDTO));
    }

    // GET api/Pacientes/{id} — retorna um paciente específico ou 404
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();
        return Ok(PacienteMapper.ToReadDTO(paciente));
    }

    // POST api/Pacientes — cria um novo paciente após validações
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PacienteCreateDTO dto)
    {
        if (!ValidarEmail(dto.Email))
            return BadRequest(new { mensagem = "Email inválido." });

        if (!ValidarTelefone(dto.Telefone))
            return BadRequest(new { mensagem = "Telefone inválido." });

        if (dto.DataNasc > DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { mensagem = "Data de nascimento não pode ser futura." });

        if (!ValidarCpf(dto.Cpf))
            return BadRequest(new { mensagem = "CPF inválido." });

        // Armazena apenas os dígitos do CPF para garantir consistência na comparação
        var cpfDigits = new string(dto.Cpf.Where(char.IsDigit).ToArray());

        // Verifica unicidade do CPF no banco antes de inserir
        if (await _context.Pacientes.AnyAsync(p => p.Cpf == cpfDigits))
            return BadRequest(new { mensagem = "Já existe um paciente com este CPF." });

        var paciente = PacienteMapper.ToModel(dto);
        paciente.Cpf = cpfDigits; // Salva CPF somente com dígitos

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync(); // Persiste no banco

        // 201 Created com o header Location apontando para o novo recurso
        return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, PacienteMapper.ToReadDTO(paciente));
    }

    // PATCH api/Pacientes/{id} — atualiza apenas os campos enviados (não-nulos)
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PacienteUpdateDTO dto)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();

        // Cada campo só é atualizado se vier preenchido na requisição
        if (dto.Email != null)
        {
            if (!ValidarEmail(dto.Email))
                return BadRequest(new { mensagem = "Email inválido." });
            paciente.Email = dto.Email;
        }

        if (dto.Telefone != null)
        {
            if (!ValidarTelefone(dto.Telefone))
                return BadRequest(new { mensagem = "Telefone inválido." });
            paciente.Telefone = dto.Telefone;
        }

        if (dto.DataNasc.HasValue)
        {
            if (dto.DataNasc.Value > DateOnly.FromDateTime(DateTime.Today))
                return BadRequest(new { mensagem = "Data de nascimento não pode ser futura." });
            paciente.DataNasc = dto.DataNasc.Value;
        }

        if (dto.Nome != null)
            paciente.Nome = dto.Nome;

        await _context.SaveChangesAsync();
        return Ok(PacienteMapper.ToReadDTO(paciente));
    }

    // DELETE api/Pacientes/{id} — remove o paciente se não tiver consultas futuras
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();

        // Impede exclusão se existir ao menos uma consulta agendada no futuro
        var temConsultasFuturas = await _context.Consultas
            .AnyAsync(c => c.PacienteId == id && c.DataHora > DateTime.Now);

        if (temConsultasFuturas)
            return BadRequest(new { mensagem = "Não é possível excluir o paciente pois ele possui consultas futuras." });

        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();
        return NoContent(); // 204: sucesso sem corpo de resposta
    }

    private static bool ValidarCpf(string cpf)
    {
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return digits.Length == 11;
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