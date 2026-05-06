# Documentação — ConsultasController.cs

## O que é este arquivo?

`ConsultasController.cs` é o **controller** responsável por gerenciar o agendamento de consultas na API.
Ele recebe as requisições HTTP, aplica as regras de negócio e se comunica com o banco de dados via **Entity Framework Core**.

---

## Estrutura de pastas do projeto

```
ApiClinica/
├── Models/          → Define as tabelas do banco de dados
├── Data/            → Conexão com o banco via Entity Framework Core
├── DTOs/            → Define o que entra e sai da API (segurança e controle)
├── Mappers/         → Converte entre Model e DTO
├── Controllers/     → Recebe requisições HTTP e aplica as regras de negócio
├── Migrations/      → Histórico de criação e alteração das tabelas (gerado automaticamente)
├── Program.cs       → Ponto de entrada da aplicação — configura e inicia tudo
└── appsettings.json → Configurações da aplicação (ex: string de conexão com o banco)
```

### Models/
Contém as classes que representam as **tabelas do banco de dados**.
Cada propriedade vira uma coluna. O Entity Framework lê essas classes para criar e manipular o banco.

```
Paciente.cs  →  tabela Pacientes
Medico.cs    →  tabela Medicos
Consulta.cs  →  tabela Consultas
```

### Data/
Contém o `AppDbContext.cs`, que é a **ponte entre o código C# e o banco de dados**.
É aqui que informamos ao Entity Framework quais tabelas existem e como elas se relacionam.

### DTOs/
DTO significa **Data Transfer Object** — objeto que define exatamente quais dados trafegam entre o cliente e a API.

Há três tipos para cada entidade:

| Tipo | Usado quando | Por que existe |
|---|---|---|
| `CreateDTO` | POST | Define os campos obrigatórios para criar |
| `ReadDTO` | Resposta do GET/POST/PATCH | Controla o que o cliente recebe — evita expor campos internos |
| `UpdateDTO` | PATCH | Todos os campos opcionais — só atualiza o que for enviado |

### Mappers/
Faz a conversão manual entre **Model** (estrutura do banco) e **DTO** (estrutura da API).
Sem essa camada, o controller precisaria fazer essa conversão diretamente, misturando responsabilidades.

```
Model → ReadDTO   (usado nas respostas)
CreateDTO → Model (usado ao salvar no banco)
```

### Controllers/
É onde as **requisições HTTP chegam**. Cada controller cuida de um recurso:

```
PacientesController.cs  →  api/Pacientes
MedicosController.cs    →  api/Medicos
ConsultasController.cs  →  api/Consultas  ← este arquivo
```

O controller recebe o DTO, aplica as validações, usa o `AppDbContext` para acessar o banco, converte com o Mapper e retorna a resposta.

### Migrations/
Pasta gerada automaticamente pelo Entity Framework ao rodar `dotnet ef migrations add`.
Contém o **histórico de versões do banco** — cada migration é um snapshot de como o banco estava em determinado momento.
Nunca deve ser editada manualmente.

### Program.cs
Ponto de entrada da aplicação. É onde:
- Os serviços são registrados (EF Core, Swagger, Controllers)
- O banco de dados é conectado via string de conexão
- A aplicação é configurada e iniciada

### appsettings.json
Arquivo de configuração. Guarda a **string de conexão** com o banco SQLite:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=clinica.db"
}
```
O `Program.cs` lê esse valor para configurar o EF Core.

---

## Onde ele se encaixa na arquitetura

```
Cliente (Postman / Frontend)
        ↓  requisição HTTP
  ConsultasController        ← este arquivo
        ↓  usa
  AppDbContext (EF Core)     ← acessa o banco SQLite
        ↓  converte
  ConsultaMapper             ← transforma Model ↔ DTO
        ↓  retorna
  ConsultaReadDTO            ← o que o cliente recebe de volta
```

---

## Conceitos utilizados

### Controller
Classe que mapeia rotas HTTP para métodos C#.
O atributo `[ApiController]` ativa comportamentos automáticos como validação de modelo.
O atributo `[Route("api/[controller]")]` define que a rota base é `api/Consultas`.

### Injeção de Dependência
O `AppDbContext` não é criado manualmente — o ASP.NET Core o injeta automaticamente no construtor.
Isso é possível porque ele foi registrado no `Program.cs` com `builder.Services.AddDbContext<AppDbContext>(...)`.

```csharp
public ConsultasController(AppDbContext context)
{
    _context = context; // o framework entrega o contexto pronto para uso
}
```

### DTO (Data Transfer Object)
Em vez de receber e retornar o Model diretamente, usamos DTOs:

| DTO | Quando é usado |
|---|---|
| `ConsultaCreateDTO` | Recebido no POST (o que o cliente envia) |
| `ConsultaUpdateDTO` | Recebido no PATCH (campos opcionais) |
| `ConsultaReadDTO` | Retornado ao cliente (GET, POST, PATCH) |

### async / await
Todos os métodos são assíncronos para não bloquear o servidor enquanto aguarda o banco de dados responder.

---

## Endpoints

### GET /api/Consultas
Retorna todas as consultas cadastradas.

```
Resposta 200 OK:
[
  { "id": 1, "pacienteId": 2, "medicoId": 3, "dataHora": "2026-05-10T14:00:00" }
]
```

---

### GET /api/Consultas/{id}
Retorna uma consulta específica pelo ID.

```
Resposta 200 OK  → consulta encontrada
Resposta 404 Not Found → ID não existe no banco
```

---

### POST /api/Consultas
Agenda uma nova consulta. Aplica **5 validações** antes de salvar:

#### Validações (em ordem)

**1. Paciente existe?**
```csharp
if (!await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId))
    return BadRequest("Paciente não encontrado.");
```
Consulta o banco para verificar se o `PacienteId` enviado existe.

**2. Médico existe?**
```csharp
if (!await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId))
    return BadRequest("Médico não encontrado.");
```

**3. Data/hora não está no passado?**
```csharp
if (dto.DataHora <= DateTime.Now)
    return BadRequest("A data/hora da consulta não pode ser no passado.");
```

**4. Médico já tem consulta nesse horário?**
Busca todas as consultas do médico e verifica sobreposição de 30 minutos (ver seção `TemSobreposicao`).

**5. Paciente já tem consulta nesse horário?**
Mesma lógica aplicada ao paciente.

#### Se tudo passar:
```
Resposta 201 Created
Header Location: /api/Consultas/7   ← aponta para o novo recurso criado
Body: { "id": 7, "pacienteId": 1, "medicoId": 2, "dataHora": "..." }
```

---

### PATCH /api/Consultas/{id}
Atualiza parcialmente uma consulta. Apenas os campos enviados são alterados.

#### Como funciona a atualização parcial
Os campos do `ConsultaUpdateDTO` são todos `nullable` (tipo `int?` e `DateTime?`).
O operador `??` mantém o valor atual se o campo não for enviado:

```csharp
var novaDataHora    = dto.DataHora   ?? consulta.DataHora;    // usa novo ou mantém atual
var novoPacienteId  = dto.PacienteId ?? consulta.PacienteId;
var novoMedicoId    = dto.MedicoId   ?? consulta.MedicoId;
```

#### Revalidação completa
Se qualquer campo mudar, todas as regras de conflito são revalidadas com os novos valores.

#### Detalhe importante: excluir a própria consulta da verificação
```csharp
.Where(c => c.MedicoId == novoMedicoId && c.Id != id)
```
Sem o `&& c.Id != id`, a consulta bloquearia a si mesma — o médico já "tem" aquele horário ocupado pela própria consulta sendo editada.

```
Resposta 200 OK  → atualizado com sucesso
Resposta 404     → consulta não encontrada
Resposta 400     → alguma validação falhou
```

---

### DELETE /api/Consultas/{id}
Remove (cancela) uma consulta.

```
Resposta 204 No Content → removido com sucesso (sem corpo na resposta)
Resposta 404 Not Found  → consulta não existe
```

---

## A lógica mais complexa: TemSobreposicao

```csharp
private static bool TemSobreposicao(DateTime novaDataHora, IEnumerable<Consulta> consultas)
{
    var novaFim = novaDataHora.AddMinutes(30);
    return consultas.Any(c => novaDataHora < c.DataHora.AddMinutes(30) && novaFim > c.DataHora);
}
```

### Por que essa condição?

Cada consulta ocupa um **intervalo de 30 minutos**.
Para detectar sobreposição entre dois intervalos `[A, B]` e `[C, D]`, a condição matemática é:

```
A < D  E  B > C
```

Traduzindo para o código:

| Variável | Significado |
|---|---|
| `novaDataHora` | Início da nova consulta (A) |
| `novaFim` | Fim da nova consulta = início + 30min (B) |
| `c.DataHora` | Início de uma consulta existente (C) |
| `c.DataHora.AddMinutes(30)` | Fim de uma consulta existente (D) |

### Exemplos visuais

**Caso 1 — Sobreposição (bloqueado)**
```
Existente:  10:00 ████████████ 10:30
Nova:             10:15 ████████████ 10:45
                  ↑ início < fim existente E fim > início existente → BLOQUEADO
```

**Caso 2 — Sem sobreposição (permitido)**
```
Existente:  10:00 ████████████ 10:30
Nova:                           10:30 ████████████ 11:00
                                ↑ início = fim existente → NÃO há sobreposição → PERMITIDO
```

---

## Respostas HTTP utilizadas

| Código | Método | Significado |
|---|---|---|
| 200 OK | GET, PATCH | Operação bem-sucedida, retorna dados |
| 201 Created | POST | Recurso criado com sucesso |
| 204 No Content | DELETE | Removido com sucesso, sem corpo |
| 400 Bad Request | POST, PATCH | Dados inválidos ou regra violada |
| 404 Not Found | GET, PATCH, DELETE | Recurso não encontrado |