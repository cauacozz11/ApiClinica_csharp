using ApiClinica.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Data;

// AppDbContext é a ponte entre o código C# e o banco de dados (padrão do EF Core)
public class AppDbContext : DbContext
{
    // Recebe as configurações de conexão via injeção de dependência (registrado no Program.cs)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Cada DbSet representa uma tabela no banco de dados
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Medico> Medicos { get; set; }
    public DbSet<Consulta> Consultas { get; set; }

    // Configura os relacionamentos entre as tabelas (chamado automaticamente pelo EF Core)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Uma Consulta pertence a um Paciente; um Paciente pode ter várias Consultas
        modelBuilder.Entity<Consulta>()
            .HasOne(c => c.Paciente)
            .WithMany(p => p.Consultas)
            .HasForeignKey(c => c.PacienteId);

        // Uma Consulta pertence a um Médico; um Médico pode ter várias Consultas
        modelBuilder.Entity<Consulta>()
            .HasOne(c => c.Medico)
            .WithMany(m => m.Consultas)
            .HasForeignKey(c => c.MedicoId);
    }
}
