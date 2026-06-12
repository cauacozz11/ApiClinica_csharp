using ApiClinica.DTOs;
using ApiClinica.Services; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PacientesController : ControllerBase
{

    private readonly IPacienteService _service;

    public PacientesController(IPacienteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _service.GetAll();
        return Ok(pacientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var paciente = await _service.GetById(id);
            return Ok(paciente);
        }
        catch (Exception ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PacienteCreateDTO dto)
    {
        try 
        {
            var paciente = await _service.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, paciente);
        } 
        catch (Exception ex) 
        {
            return NotFound(new { mensagem = ex.Message});
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] PacienteUpdateDTO dto)
    {
        try 
        {
            var paciente = await _service.Update(id, dto);
            return Ok(paciente);
        } 
        catch (Exception ex) 
        {
            return NotFound(new { mensagem = ex.Message});
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.Delete(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
