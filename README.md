# 🏛️ HexagonalLab.NET10

> Um laboratório prático para implementação da **Arquitetura Hexagonal (Ports & Adapters)** em .NET 10, seguindo os princípios originais de **Alistair Cockburn**.

## 📋 Visão Geral

**HexagonalLab.NET10** é um projeto educacional e de referência que demonstra como construir aplicações .NET completamente desacopladas, testáveis e independentes de infraestrutura, seguindo rigorosamente os princípios da Arquitetura Hexagonal.

### 🎯 Objetivo Principal

Construir uma aplicação que:
- ✅ Funciona **sem banco de dados**
- ✅ Funciona **sem API** (inicialmente)
- ✅ Pode ser executada via:
  - API REST
  - Worker/Background Job
  - Teste automatizado
  - CLI

> **Princípio Central**: A aplicação core é independente de qualquer framework ou tecnologia externa. A "mágica" está em separar o "inside" (núcleo) do "outside" (adaptadores).

---

## 🏗️ Princípios Arquiteturais

### Separação: Inside vs Outside

```
┌─────────────────────────────────────┐
│         OUTSIDE (Adapters)          │
│  ┌──────────────────────────────┐  │
│  │   INPUT ADAPTERS             │  │
│  │  • API / REST Controllers    │  │
│  │  • Worker / Background Jobs  │  │
│  │  • CLI / Console             │  │
│  └──────────────────────────────┘  │
│                                     │
│  ┌──────────────────────────────┐  │
│  │   INSIDE (Core/Hexagon)      │  │
│  │  • UseCases                  │  │
│  │  • Ports (Interfaces)        │  │
│  │  • Models                    │  │
│  │  • Business Logic            │  │
│  │  ZERO Dependencies!          │  │
│  └──────────────────────────────┘  │
│                                     │
│  ┌──────────────────────────────┐  │
│  │   OUTPUT ADAPTERS            │  │
│  │  • Database (EF Core, etc)   │  │
│  │  • External APIs             │  │
│  │  • Message Queue / Event Bus │  │
│  └──────────────────────────────┘  │
└─────────────────────────────────────┘
```

### ✋ O que NÃO fazer

| ❌ Proibido | ✅ Permitido |
|-----------|-----------|
| DDD (Aggregates, Entities, Value Objects) | Simple Models + UseCase + Ports |
| EF Core no Core | EF Core em Adapters.Out |
| ASP.NET no Core | ASP.NET em Adapters.In |
| HTTP no Core | HTTP em Adapters.In |
| Banco no Core | Banco em Adapters.Out |

### Regra de Dependência

```
Core ← Adapters.In (dependem do Core)
Core ← Adapters.Out (dependem do Core)
Core ← NADA (apena packages, sem frameworks)
```

---

## 📁 Estrutura de Pastas

```
HexagonalLab.NET10/
├── src/
│   ├── HexagonalLab.Core/                 # ❤️ O Hexágono (Inside)
│   │   ├── UseCases/                     # Lógica de negócio
│   │   │   └── CreateOrderUseCase.cs
│   │   ├── Ports/                        # Interfaces (contratos)
│   │   │   ├── Input/
│   │   │   │   └── ICreateOrderPort.cs
│   │   │   └── Output/
│   │   │       └── IOrderRepository.cs
│   │   ├── Models/                       # DTOs, Request/Response
│   │   │   └── Order.cs
│   │   └── HexagonalLab.Core.csproj
│   │
│   ├── HexagonalLab.Adapters.In/         # 📥 Input Adapters (Outside)
│   │   ├── API/
│   │   │   ├── Controllers/
│   │   │   │   └── OrderController.cs
│   │   │   └── Program.cs
│   │   ├── Worker/
│   │   │   └── OrderProcessor.cs
│   │   └── HexagonalLab.Adapters.In.csproj
│   │
│   ├── HexagonalLab.Adapters.Out/        # 📤 Output Adapters (Outside)
│   │   ├── Persistence/
│   │   │   ├── OrderRepository.cs        # Implementa IOrderRepository
│   │   │   ├── DbContext/
│   │   │   │   └── ApplicationDbContext.cs
│   │   │   └── Migrations/
│   │   ├── ExternalServices/
│   │   │   └── NotificationService.cs
│   │   ├── Fakes/                        # Para testes do core
│   │   │   └── FakeOrderRepository.cs
│   │   └── HexagonalLab.Adapters.Out.csproj
│   │
│   └── HexagonalLab.Bootstrap/           # 🔧 DI Configuration
│       ├── ServiceCollectionExtensions.cs
│       └── HexagonalLab.Bootstrap.csproj
│
├── tests/
│   ├── HexagonalLab.Core.Tests/          # 🧪 Testa o hexágono
│   │   ├── UseCases/
│   │   │   └── CreateOrderUseCaseTests.cs
│   │   └── HexagonalLab.Core.Tests.csproj
│   │
│   └── HexagonalLab.Integration.Tests/   # 🔗 Testa os adapters
│       ├── API/
│       │   └── OrderControllerTests.cs
│       └── HexagonalLab.Integration.Tests.csproj
│
├── docs/
│   ├── ARCHITECTURE.md                   # Detalhamento da arquitetura
│   ├── DECISIONS.md                      # Architecture Decision Records
│   └── SETUP.md                          # Como configurar
│
├── .github/
│   ├── copilot-instructions.md           # Instruções para IA
│   └── workflows/                        # CI/CD
│
├── README.md                             # Este arquivo
├── LICENSE
└── HexagonalLab.NET10.sln
```

---

## 🚀 Roadmap de Aprendizado (5 Fases)

### 🔹 Fase 1: Fundamentos (Dia 1)
**Objetivo**: Entender "inside vs outside"

- [ ] Criar solução .NET 10
- [ ] Criar projeto Core (hexágono)
- [ ] Criar primeiro UseCase (ex: CreateOrder)
- [ ] Criar Input Port interface
- [ ] Criar modelos simples

**Entregável**: Core rodando via teste unitário ✅

### 🔹 Fase 2: Output Ports (Dia 2)
**Objetivo**: Isolar dependências externas

- [ ] Criar Output Port (ex: IOrderRepository)
- [ ] Criar implementação fake (in-memory)
- [ ] Testar UseCase com mock
- [ ] Validar que Core não conhece banco

**Entregável**: Aplicação funcionando SEM banco 🎯

### 🔹 Fase 3: Input Adapter (Dia 3)
**Objetivo**: Conectar o mundo externo

- [ ] Criar API (.NET Minimal API ou Controller)
- [ ] Conectar ao Input Port
- [ ] Executar fluxo via HTTP
- [ ] Validar isolamento do Core

**Entregável**: API funcionando sem alterar Core 🌐

### 🔹 Fase 4: Output Adapter Real (Dia 4)
**Objetivo**: Plug real infrastructure

- [ ] Criar adapter de banco (EF Core ou Dapper)
- [ ] Substituir fake via Dependency Injection
- [ ] Migrations
- [ ] Testes de integração

**Entregável**: Persistência de verdade funcionando 💾

### 🔹 Fase 5: Polimento (Dia 5)
**Objetivo**: Production-ready

- [ ] Logging
- [ ] Error Handling
- [ ] Validação
- [ ] Tests coverage
- [ ] Documentation

**Entregável**: Aplicação completa e testada 🚀

---

## 💻 Como Começar

### Pré-requisitos
- .NET 10 SDK
- Visual Studio 2022 ou VS Code
- Git
- Docker (opcional, para rodar containers)
- SQL Server (local ou Docker)

### 🚀 Executar Localmente (Simples e Rápido)

#### 1️⃣ **Clonar o repositório**

```bash
git clone https://github.com/seu-usuario/HexagonalLab.NET10.git
cd HexagonalLab.NET10
```

#### 2️⃣ **Restaurar dependências**

Instala todos os pacotes NuGet necessários:

```bash
dotnet restore
```

#### 3️⃣ **Executar os testes (verificar que está ok)**

Testa o Core sem banco, sem API:

```bash
dotnet test
```

**Resultado esperado:**
```
✅ Passed HexagonalLab.Core.Tests
✅ All tests passed!
```

---

### 🐳 Rodar com Docker (Recomendado)

Se preferir rodar tudo containerizado (API + Worker + SQL Server):

#### 1️⃣ **Certifique-se que Docker está rodando**

```bash
docker --version
```

#### 2️⃣ **Inicie todos os containers**

```bash
docker-compose up -d
```

Isso vai:
- ✅ Criar container SQL Server 2022
- ✅ Criar container API (.NET)
- ✅ Criar container Worker (Background Job)
- ✅ Executar migrations automaticamente

#### 3️⃣ **Aguarde 30-45 segundos** 

SQL Server leva um tempo para iniciar. Verifique o status:

```bash
docker-compose ps
```

**Resultado esperado:**
```
NAMES                STATUS
hexagonal-sqlserver  Up 30s (healthy)
hexagonal-api        Up 25s
hexagonal-worker     Up 25s
```

#### 4️⃣ **Verifique os logs**

**Logs da API:**
```bash
docker logs hexagonal-api -f
```

**Logs do Worker:**
```bash
docker logs hexagonal-worker -f
```

---

### 📊 Operações Comuns

#### 📝 Inserir dados de teste

```bash
.\scripts\insert-test-data.ps1 -Type sample
```

Isso insere 5 itens de teste:
- ITEM-001 (Pedido 001)
- ITEM-002 (Pedido 002)
- ... e mais 3

#### 🔍 Consultar itens no banco

```bash
$DbUser = 'sa'
$DbPassword = 'Hexagon123'
$DbName = 'HexagonalLab'

docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U $DbUser -P $DbPassword -d $DbName -C `
    -Q "SELECT Id, Name, Status, ProcessedAt FROM Items ORDER BY CreatedAt DESC"
```

#### 🧹 Limpar dados

```bash
$DbUser = 'sa'
$DbPassword = 'Hexagon123'
$DbName = 'HexagonalLab'

docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U $DbUser -P $DbPassword -d $DbName -C `
    -Q "DELETE FROM Items"
```

#### 📋 Ver logs completos do Worker

```bash
type logs\worker\worker.log
```

#### 🛑 Parar tudo

```bash
docker-compose down
```

---

### 🧪 Testar a API (via HTTP)

Após iniciar via Docker, a API fica disponível em `http://localhost:5000`

#### Criar um item:

```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "id": "ITEM-TEST-001",
    "name": "Teste Manual",
    "status": "Pending"
  }'
```

#### Obter todos os itens:

```bash
curl http://localhost:5000/api/items
```

#### Processar um item:

```bash
curl -X POST http://localhost:5000/api/items/ITEM-TEST-001/process \
  -H "Content-Type: application/json"
```

---

### 📱 Fluxo Completo (Passo a Passo)

**O que acontece quando você executa:**

1. **Docker Compose inicia**
   ```
   SQL Server → Aguarda 30-45s
         ↓
   Migrations → Cria schema
         ↓
   API → Inicia e aguarda conexão
         ↓
   Worker → Inicia com timer de 20 segundos
   ```

2. **Você insere dados**
   ```
   INSERT → 5 itens com status "Pending"
   ```

3. **Worker processa automaticamente**
   ```
   A cada 20 segundos:
   ├── Busca itens "Pending"
   ├── Processa cada um
   ├── Atualiza status para "Processed"
   └── Registra timestamp
   ```

4. **Você verifica resultados**
   ```
   Via SQL:
   SELECT * FROM Items → Todos com status "Processed"
   
   Via Logs:
   docker logs hexagonal-worker → Mostra todas as operações
   ```

---

### ⚠️ Troubleshooting Comum

#### ❌ "Connection refused - SQL Server não está respondendo"
**Solução**: Aguarde mais tempo (SQL Server pode levar 60s para iniciar)
```bash
# Verificar saúde
docker-compose ps
# Aguardar até ficar "healthy"
```

#### ❌ "Port 1433 already in use"
**Solução**: Outra instância está rodando
```bash
docker-compose down  # Para containers
# ou
Stop-Process -Name sqlservr  # Mata SQL Server local
```

#### ❌ "Migrations failed"
**Solução**: Limpe e recrie
```bash
docker-compose down -v  # Remove volumes
docker-compose up -d    # Recria do zero
```

#### ❌ "Worker não está processando"
**Solução**: Verifique logs
```bash
docker logs hexagonal-worker --tail 50
# Procure por erros na conexão ou configuração
```

---

### ✅ Checklist de Verificação

Após executar `docker-compose up -d`:

- [ ] SQL Server está saudável: `docker-compose ps` mostra `healthy`
- [ ] API iniciou: `docker logs hexagonal-api` mostra "Application started"
- [ ] Worker iniciou: `docker logs hexagonal-worker` mostra "HEXAGONAL LAB WORKER"
- [ ] Banco tem tabela Items: `docker exec hexagonal-sqlserver ... SELECT COUNT(*) FROM Items`
- [ ] Worker está processando: `docker logs hexagonal-worker | grep "Processing items"`

---

## 🔑 Conceitos-Chave

### UseCase (Caso de Uso)
Implementa uma regra de negócio isolada. Recebe Input Port, usa Output Ports.

```csharp
public class CreateOrderUseCase
{
    private readonly IOrderRepository _repository;
    
    public CreateOrderUseCase(IOrderRepository repository)
    {
        _repository = repository;
    }
    
    public OrderResponse Execute(CreateOrderRequest request)
    {
        // Lógica pura, sem dependências de framework
        var order = new Order { /* ... */ };
        _repository.Save(order);
        return new OrderResponse { /* ... */ };
    }
}
```

### Input Port (Interface)
Define contrato público do UseCase.

```csharp
public interface ICreateOrderPort
{
    OrderResponse Execute(CreateOrderRequest request);
}
```

### Output Port (Interface)
Define contrato para dependência externa.

```csharp
public interface IOrderRepository
{
    void Save(Order order);
    Order GetById(int id);
}
```

### Adapter (Implementação)
Implementa Output Port com tecnologia real.

```csharp
public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    
    public void Save(Order order)
    {
        _context.Orders.Add(order);
        _context.SaveChanges();
    }
}
```

---

## 🧪 Testabilidade

### Teste do Core (sem banco, sem API)

```csharp
[Fact]
public void UseCase_ShouldBeIndependentOfFramework()
{
    // Apenas o Core, sem EF, sem HTTP
    var fakeRepo = new FakeOrderRepository();
    var useCase = new CreateOrderUseCase(fakeRepo);
    
    var result = useCase.Execute(new CreateOrderRequest { /* ... */ });
    
    Assert.True(result.Success);
}
```

### Teste de Adapter (com framework)

```csharp
[Fact]
public async Task API_Should_Call_UseCase_HappyPath()
{
    // Aqui sim, pode usar EF, HTTP, etc
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var response = await client.PostAsync("/orders", /* ... */);
    
    Assert.True(response.IsSuccessStatusCode);
}
```

---

## 📚 Referências e Recursos

### Papers Originais
- [Alistair Cockburn - Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [The Ports and Adapters Pattern](https://www.drdobbs.com/database/the-ports-and-adapters-architecture/232010200)

### .NET 10 Resources
- [Microsoft Learn - .NET 10](https://learn.microsoft.com/en-us/dotnet/)
- [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)

### Arquitetura
- [Best Practices](./docs/ARCHITECTURE.md)
- [Decision Records](./docs/DECISIONS.md)
- [Setup Guide](./docs/SETUP.md)

---

## 🤝 Convenções do Projeto

- **Naming**: PascalCase para classes, interfaces com `I` prefix
- **Folders**: Espelhar namespaces
- **DI**: Constructor Injection apenas
- **Tests**: Suffix `.Tests`, 1 test file por classe
- **Git**: Feature branches, PR com descrição

---

## 📝 Azul DevOps Integration

Este projeto usa **Azure DevOps** para gestão de trabalho:

- **Epic**: Fase (Ex: "Fundamentos")
- **Feature**: Objetivo (Ex: "Isolar dependências externas")
- **User Story**: Entrega (Ex: "Criar Output Port")
- **Task**: Subtarefa (Ex: "Implementar IOrderRepository")

---

## 📄 Licença

MIT License - Veja [LICENSE](LICENSE) para detalhes.

---

## ✍️ Autor

Laboratório prático criado seguindo os princípios de **Alistair Cockburn** e as melhores práticas de Arquitetura Hexagonal em .NET.

---

**Pronto para começar? 🚀**

Leia o [SETUP.md](./docs/SETUP.md) para instruções detalhadas de configuração!
