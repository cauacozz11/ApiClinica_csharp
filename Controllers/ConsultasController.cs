using ApiClinica.DTOs;
using ApiClinica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsultasController : ControllerBase
{
    private readonly IConsultaService _service;

    public ConsultasController(IConsultaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var consultas = await _service.GetAll();
        return Ok(consultas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var consulta = await _service.GetById(id);
            return Ok(consulta);
        }
        catch (Exception ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConsultaCreateDTO dto)
    {
        try
        {
            var consulta = await _service.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = consulta.Id }, consulta);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ConsultaUpdateDTO dto)
    {
        try
        {
            var consulta = await _service.Update(id, dto);
            return Ok(consulta);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
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