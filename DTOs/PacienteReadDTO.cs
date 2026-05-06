namespace ApiClinica.DTOs;

// DTO usado nas respostas GET: define o que a API retorna ao cliente
// Separar do Model evita expor campos internos ou sensíveis do banco
public class PacienteReadDTO
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Telefone { get; set; }
    public required DateOnly DataNasc { get; set; }
    public required string Cpf { get; set; }
}