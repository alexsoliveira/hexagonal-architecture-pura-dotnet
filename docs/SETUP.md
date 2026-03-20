# 🔧 SETUP.md — Guia de Configuração

> Instruções passo a passo para configurar o **HexagonalLab.NET10** em sua máquina.

## 📋 Índice

1. [Pré-requisitos](#pré-requisitos)
2. [Instalação Inicial](#instalação-inicial)
3. [Configuração do Ambiente](#configuração-do-ambiente)
4. [Configuração Azure DevOps (MCP)](#configuração-azure-devops-mcp)
5. [Primeiro Teste](#primeiro-teste)
6. [Executar a Aplicação](#executar-a-aplicação)
7. [Troubleshooting](#troubleshooting)
8. [Próximos Passos](#próximos-passos)

---

## 🎯 Pré-requisitos

Antes de começar, certifique-se de ter:

### Sistema Operacional
- ✅ Windows 10/11, macOS 12+, ou Linux (Ubuntu 20.04+)

### Software Obrigatório

| Ferramenta | Versão | Link |
|-----------|--------|------|
| **.NET SDK** | 10.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| **Git** | 2.40+ | [git-scm.com](https://git-scm.com/) |
| **Visual Studio** ou **VS Code** | 2022+ ou Latest | [visualstudio.microsoft.com](https://visualstudio.microsoft.com/) |
| **PowerShell** | 5.1+ (Windows) | Ambiente padrão |

### Verificar Instalação

```bash
# Verificar .NET SDK
dotnet --version

# Deverá retornar algo como: 10.0.0 (ou superior)

# Verificar Git
git --version

# Deverá retornar algo como: git version 2.40.0
```

---

## 💻 Instalação Inicial

### 1️⃣ Clonar o Repositório

```bash
# Opção A: HTTPS
git clone https://github.com/seu-usuario/HexagonalLab.NET10.git

# Opção B: SSH
git clone git@github.com:seu-usuario/HexagonalLab.NET10.git

# Entrar no diretório
cd HexagonalLab.NET10
```

### 2️⃣ Restaurar Dependências

```bash
# Restaurar todos os pacotes NuGet
dotnet restore

# Saída esperada:
# Determining projects to restore...
# Restored HexagonalLab.NET10.sln (XXX ms)
```

### 3️⃣ Build da Solução

```bash
# Fazer build em modo Debug
dotnet build

# Ou em modo Release para testes de performance
dotnet build -c Release

# Saída esperada:
# Build succeeded. X warning(s), X error(s)
```

### 4️⃣ Executar Testes

```bash
# Rodar todos os testes
dotnet test

# Ou com verbose
dotnet test --verbosity detailed

# Ou um projeto de testes específico
dotnet test tests/HexagonalLab.Core.Tests/
```

---

## ⚙️ Configuração do Ambiente

### 1️⃣ Arquivo `.env`

O projeto usa variáveis de ambiente para configuração. Copie o arquivo de exemplo:

```bash
# Copiar arquivo de exemplo
cp env.exemplo .env

# Editar o arquivo .env com seus valores
# No Windows:
notepad .env

# No macOS/Linux:
nano .env
```

### 2️⃣ Variáveis de Ambiente

Preencha o `.env` com seus valores:

```env
# Azure DevOps (opcional, apenas se usar Azure DevOps)
AZURE_DEVOPS_PAT=seu_personal_access_token
AZURE_DEVOPS_ORG=sua_organizacao_azure_devops
AZURE_DEVOPS_PROJECT=seu_projeto

# MCP Server (opcional, para comunicação com IA)
MCP_TRANSPORT=stdio|sse|http
MCP_SERVER_COMMAND=/caminho/para/servidor

# Ambiente de Execução
ASPNETCORE_ENVIRONMENT=Development|Staging|Production

# Banco de Dados (quando implementado em Fase 4)
DB_CONNECTION_STRING=Server=localhost;Database=HexagonalLab;User Id=sa;Password=YourPassword;
```

### 3️⃣ Estrutura de Pastas (Criar se necessário)

```bash
# Criar pastas base se não existirem
mkdir -p src tests docs

# A estrutura esperada é:
# HexagonalLab.NET10/
# ├── src/
# │   ├── HexagonalLab.Core/
# │   ├── HexagonalLab.Adapters.In/
# │   ├── HexagonalLab.Adapters.Out/
# │   └── HexagonalLab.Bootstrap/
# ├── tests/
# │   ├── HexagonalLab.Core.Tests/
# │   └── HexagonalLab.Integration.Tests/
# └── docs/
```

---

## 🔌 Configuração Azure DevOps (MCP)

### ⚠️ Pré-requisito
Este laboratório integra com **Azure DevOps MCP Server** para facilitar o gerenciamento de work items. Se você não tem Azure DevOps, pode pular esta seção.

### 1️⃣ Criar Projeto no Azure DevOps

1. Acesse [dev.azure.com](https://dev.azure.com)
2. Crie uma nova organização (se ainda não tiver)
3. Crie um novo projeto chamado **HexagonalLab**
4. Escolha processo **Basic** ou **Scrum**

### 2️⃣ Gerar Personal Access Token (PAT)

1. Clique em **User Settings** (ícone de engrenagem) → **Personal access tokens**
2. Clique em **New Token**
3. Preencha:
   - **Name**: `HexagonalLab-Token`
   - **Organization**: Selecione sua organização
   - **Expiration**: 1 ano
   - **Scopes**: No campo "Scopes", selecione:
     - ✅ Work Items (Read & Write)
     - ✅ Project & Team (Read)
4. Copie o token gerado

### 3️⃣ Configurar Variáveis de Ambiente

Atualize o arquivo `.env`:

```env
AZURE_DEVOPS_PAT=seu_pat_aqui
AZURE_DEVOPS_ORG=sua_organizacao
AZURE_DEVOPS_PROJECT=HexagonalLab
```

### 4️⃣ Criar Épica Inicial

Use o terminal para criar a primeira épica (ou use a UI do Azure DevOps):

```bash
# Criar épica via MCP (se disponível)
# Isso depende da implementação do MCP no projeto
```

Ou manualmente no Azure DevOps:
1. Acesse **Boards** → **Backlogs**
2. Clique em **New Epic**
3. Crie: **[EPIC] Fundamentos - Fase 1**

---

## 🧪 Primeiro Teste

### 🎯 Objetivo
Validar que o ambiente está funcionando corretamente.

### 📝 Passo 1: Criar Teste do Core

Crie o arquivo `tests/HexagonalLab.Core.Tests/HelloWorldTest.cs`:

```csharp
using Xunit;

namespace HexagonalLab.Core.Tests;

public class HelloWorldTest
{
    [Fact]
    public void TestEnvironment_ShouldPass()
    {
        // Arrange
        var expected = 2;
        
        // Act
        var actual = 1 + 1;
        
        // Assert
        Assert.Equal(expected, actual);
    }
}
```

### 📝 Passo 2: Rodas Teste

```bash
# Rodar um teste específico
dotnet test tests/HexagonalLab.Core.Tests/

# Output esperado:
# Passed HelloWorldTest [~50ms]
# 1 passed, 0 failed
```

### ✅ Sucesso!

Se passou, seu ambiente está OK! 🎉

---

## 🚀 Executar a Aplicação

### Opção A: Rodar testes (sem API/DB)

```bash
# Testa apenas o Core
dotnet test tests/HexagonalLab.Core.Tests/

# Todos os testes
dotnet test

# Com coverage
dotnet test /p:CollectCoverage=true
```

### Opção B: Rodar a API (Fase 3+)

```bash
# Entrar no diretório do Adapters.In
cd src/HexagonalLab.Adapters.In

# Rodar a aplicação
dotnet run

# Output esperado:
# info: Microsoft.Hosting.Lifetime[14]
#      Now listening on: https://localhost:5001
# info: Microsoft.Hosting.Lifetime[0]
#      Application started.
```

### Opção C: Rodar Worker (Fase 5+)

```bash
# Entrar no diretório do Worker
cd src/HexagonalLab.Adapters.In/Worker

# Rodar o worker
dotnet run

# Output esperado:
# Worker running at: 2024-03-20 10:00:00
```

---

## 🐛 Troubleshooting

### Problema: "dotnet: command not found"

**Solução**:
```bash
# Verificar instalação do .NET
dotnet --version

# Se não encontrar, instalar em: https://dotnet.microsoft.com/download
```

### Problema: "Restore failed"

**Solução**:
```bash
# Limpar cache de pacotes
dotnet nuget locals all --clear

# Restaurar novamente
dotnet restore --force
```

### Problema: Testes falhando com "Assembly not found"

**Solução**:
```bash
# Build primeiro
dotnet build

# Depois rodar testes
dotnet test
```

### Problema: "Project not found" ao usar dotnet run

**Solução**:
```bash
# Verificar se está no diretório correto
pwd  # Linux/macOS
cd   # Windows (mostra diretório atual)

# Ou especificar o projeto
dotnet run --project src/HexagonalLab.Adapters.In/
```

### Problema: Porta 5001 já em uso

**Solução**:
```bash
# Usar porta diferente
dotnet run -- --urls=https://localhost:5002

# Ou no Windows, encontrar o processo:
netstat -ano | findstr :5001
taskkill /PID <PID> /F
```

### Problema: Variáveis de Ambiente não são reconhecidas

**Solução**:
```bash
# Adicionar à sessão do PowerShell (Windows)
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Ou bash (Linux/macOS)
export ASPNETCORE_ENVIRONMENT=Development

# Ou criar arquivo .env.local (que não é commitado)
```

---

## 📚 Próximos Passos

### 🔹 Fase 1: Fundamentos

Depois de validar o setup, comece a Fase 1:

- [ ] [Ler ARCHITECTURE.md](./ARCHITECTURE.md) para entender os princípios
- [ ] Criar modelos no Core
- [ ] Definir Input Ports
- [ ] Implementar primeiro UseCase

```bash
# Ver roadmap detalhado em:
../README.md  # Seção "Roadmap de Aprendizado"
```

### 🔹 Estrutura de Desenvolvimento

```bash
# Fazer um branch para desenvolvimento
git checkout -b feature/fase-1-fundamentos

# Fazer suas alterações
# Depois fazer commit
git add .
git commit -m "feat: implementar fundamentos"

# Push ao repositório
git push origin feature/fase-1-fundamentos
```

### 🔹 Adicionar Novos Projetos

```bash
# Criar novo projeto classlib
dotnet new classlib -n HexagonalLab.Core

# Adicionar à solução
dotnet sln add src/HexagonalLab.Core/

# Restaurar workspace
dotnet restore
```

### 🔹 Executar Tests Continuamente

```bash
# Usar watch mode (reexecuta tests a cada mudança)
dotnet watch test

# Ou em um projeto específico
cd tests/HexagonalLab.Core.Tests/
dotnet watch test
```

---

## 🎓 Recursos Adicionais

### Documentação Interna
- [README.md](../README.md) — Visão geral do projeto
- [ARCHITECTURE.md](./ARCHITECTURE.md) — Detalhes arquiteturais
- [DECISIONS.md](./DECISIONS.md) — Architecture Decision Records

### Documentação Externa
- [Alistair Cockburn - Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Microsoft .NET 10 Docs](https://learn.microsoft.com/pt-br/dotnet/)
- [xUnit.net Testing Framework](https://xunit.net/)
- [Azure DevOps REST API](https://learn.microsoft.com/en-us/rest/api/azure/devops/)

### Comandos Úteis

```bash
# Ver versão de todos os pacotes
dotnet list package

# Atualizar pacotes
dotnet add package <package-name>

# Remover pacote
dotnet remove package <package-name>

# Ver estrutura da solução
dotnet sln list

# Listar projetos no diretório
dotnet sln HexagonalLab.NET10.sln list

# Executar com configuration específica
dotnet run -c Release

# Ver informações do ambiente
dotnet --info
```

---

## ✅ Checklist Final

Antes de começar o desenvolvimento, valide:

- [ ] .NET 10 SDK instalado (`dotnet --version`)
- [ ] Git configurado (`git config --list`)
- [ ] Repositório clonado
- [ ] Dependências restauradas (`dotnet restore`)
- [ ] Build realizado (`dotnet build`)
- [ ] Testes passando (`dotnet test`)
- [ ] Arquivo `.env` configurado
- [ ] (Opcional) Azure DevOps PAT gerado e configurado
- [ ] IDE aberta (VS 2022 ou VS Code)
- [ ] Leu a seção "Fase 1" do README.md

---

## 🤝 Suporte

Se encontrar problemas:

1. **Verificar Logs**: Procure por mensagens de erro detalhadas
2. **Consultar Troubleshooting**: Seção acima
3. **Ler Documentação**: ARCHITECTURE.md, README.md
4. **GitHub Issues**: Abra uma issue com log de erro completo
5. **Chat com IA**: Use os prompts no `.github/prompts/`

---

**Pronto para começar? 🚀**

Execute:
```bash
dotnet test
```

Se passar, você está pronto para a **Fase 1**!

Boa sorte! 💪
