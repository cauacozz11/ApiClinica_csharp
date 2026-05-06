namespace ApiClinica.DTOs;

// DTO usado no POST: define exatamente quais campos o cliente deve enviar para criar um paciente
// "required" garante que o campo é obrigatório na requisição
public class PacienteCreateDTO
{
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Telefone { get; set; }
    public required DateOnly DataNasc { get; set; }
    public required string Cpf { get; set; }
}