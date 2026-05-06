namespace ApiClinica.DTOs;

// DTO usado no PATCH: todos os campos são opcionais (nullable)
// Campos com null são ignorados — apenas os campos enviados serão atualizados
public class PacienteUpdateDTO
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public DateOnly? DataNasc { get; set; }
    // CPF não está aqui intencionalmente — não pode ser alterado após o cadastro
}