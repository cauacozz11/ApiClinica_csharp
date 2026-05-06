namespace ApiClinica.DTOs;

// DTO usado nas respostas GET: representa os dados do médico retornados pela API
public class MedicoReadDTO
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Telefone { get; set; }
    public required string CRM { get; set; }
}