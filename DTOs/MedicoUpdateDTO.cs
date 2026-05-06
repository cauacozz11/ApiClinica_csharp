namespace ApiClinica.DTOs;

// DTO usado no PATCH: todos os campos são opcionais — só atualiza o que for enviado
public class MedicoUpdateDTO
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? CRM { get; set; }
}