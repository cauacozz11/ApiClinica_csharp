namespace ApiClinica.DTOs;

// DTO usado no POST: define os campos obrigatórios para cadastrar um médico
public class MedicoCreateDTO
{
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Telefone { get; set; }
    public required string CRM { get; set; }
}