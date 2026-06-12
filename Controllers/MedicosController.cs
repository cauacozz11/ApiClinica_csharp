using ApiClinica.DTOs;
using ApiClinica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicosController : ControllerBase
{
    private readonly IMedicoService _service;

    public MedicosController(IMedicoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var medicos = await _service.GetAll();
        return Ok(medicos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var medico = await _service.GetById(id);
            return Ok(medico);
        }
        catch (Exception ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MedicoCreateDTO dto)
    {
        try
        {
            var medico = await _service.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = medico.Id }, medico);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] MedicoUpdateDTO dto)
    {
        try
        {
            var medico = await _service.Update(id, dto);
            return Ok(medico);
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