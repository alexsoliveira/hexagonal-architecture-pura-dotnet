Perfeito. Vou te entregar um **laboratório completo (hands-on)** + **roadmap progressivo** + **checklist diário técnico** + **setup no Azure DevOps**, tudo **fiel ao paper do Alistair Cockburn**.

---

# 🎯 1. NOME DO PROJETO (Azure DevOps)

### 📌 Nome:

**HexagonalLab.NET10**

### 📌 Descrição:

Laboratório prático para implementação da Arquitetura Hexagonal (Ports & Adapters), focado na construção de aplicações desacopladas, testáveis e independentes de infraestrutura, seguindo o conceito original de separação entre “inside” (core) e “outside” (adapters), permitindo execução isolada sem UI ou banco de dados. ([Alistair Cockburn][1])

---

# 🧪 2. OBJETIVO DO LAB

Você vai construir uma aplicação que:

* Funciona **sem banco**
* Funciona **sem API**
* Pode ser executada via:

  * API
  * Worker
  * Teste automatizado

👉 Exatamente como o paper define:

> A aplicação deve rodar isolada de UI e banco ([Alistair Cockburn][1])

---

# 🧭 3. ROADMAP DE APRENDIZADO (Hands-on)

## 🔹 Fase 1 — Fundamentos (Dia 1)

Objetivo: Entender o “inside vs outside”

* Criar solução .NET 10
* Criar projeto Core (hexágono)
* Criar primeiro UseCase
* Criar Input Port

📌 Entregável:

* Core rodando via teste unitário

---

## 🔹 Fase 2 — Output Ports (Dia 2)

Objetivo: Isolar dependências externas

* Criar Output Port (ex: repositório)
* Criar implementação fake (in-memory)
* Testar UseCase com mock

📌 Entregável:

* Aplicação funcionando SEM banco

---

## 🔹 Fase 3 — Input Adapter (Dia 3)

Objetivo: Conectar o mundo externo

* Criar API (.NET Minimal API ou Controller)
* Conectar ao Input Port
* Executar fluxo via HTTP

📌 Entregável:

* API funcionando sem alterar o core

---

## 🔹 Fase 4 — Output Adapter Real (Dia 4)

Objetivo: Plug real infrastructure

* Criar adapter de banco (ex: EF ou Dapper)
* Substituir fake via DI

📌 Entregável:

* Troca de adapter sem impacto no core

---

## 🔹 Fase 5 — Segundo Input Adapter (Dia 5)

Objetivo: Demonstrar plugabilidade

* Criar Worker (BackgroundService)
* Reutilizar UseCase

📌 Entregável:

* Mesmo core sendo usado por API + Worker

---

## 🔹 Fase 6 — Testabilidade (Dia 6)

Objetivo: Validar conceito do paper

* Criar testes automatizados
* Usar mocks nos ports

📌 Entregável:

* Testes sem infraestrutura

---

## 🔹 Fase 7 — Evolução Arquitetural (Dia 7)

Objetivo: Consolidar

* Adicionar novo adapter (ex: API externa)
* Validar múltiplos adapters no mesmo port

📌 Entregável:

* Sistema extensível

---

# 🧱 4. PASSO A PASSO DO LAB (EXECUÇÃO)

## 🔧 PASSO 1 — Criar Solution

```bash
dotnet new sln -n HexagonalLab
```

---

## 🔧 PASSO 2 — Criar Core (HEXÁGONO)

```bash
dotnet new classlib -n HexagonalLab.Core
```

Criar:

* Models
* Ports (Input/Output)
* UseCases

---

## 🔧 PASSO 3 — Criar Adapter de Entrada

```bash
dotnet new webapi -n HexagonalLab.Adapters.In.Api
```

---

## 🔧 PASSO 4 — Criar Adapter de Saída

```bash
dotnet new classlib -n HexagonalLab.Adapters.Out.Database
```

---

## 🔧 PASSO 5 — Criar Bootstrap

```bash
dotnet new web -n HexagonalLab.Bootstrap
```

👉 Aqui acontece:

* DI
* Wiring
* Startup

---

## 🔧 PASSO 6 — Teste SEM infraestrutura

```bash
dotnet new xunit -n HexagonalLab.Tests
```

👉 Esse é o ponto-chave do paper:

* Rodar sem DB
* Rodar sem API

---

# 📋 5. CHECKLIST TÉCNICO DIÁRIO (Azure DevOps Tasks)

## 📅 DIA 1

* [ ] Criar solution .NET 10
* [ ] Criar projeto Core
* [ ] Criar primeiro UseCase
* [ ] Criar Input Port
* [ ] Criar teste unitário básico

---

## 📅 DIA 2

* [ ] Criar Output Port
* [ ] Criar Fake Adapter (in-memory)
* [ ] Injetar dependência no UseCase
* [ ] Validar execução sem DB

---

## 📅 DIA 3

* [ ] Criar API Adapter
* [ ] Mapear endpoint → UseCase
* [ ] Testar fluxo HTTP

---

## 📅 DIA 4

* [ ] Criar Adapter real (DB ou API externa)
* [ ] Configurar DI
* [ ] Validar troca de adapter

---

## 📅 DIA 5

* [ ] Criar Worker Adapter
* [ ] Reutilizar UseCase
* [ ] Executar fluxo automático

---

## 📅 DIA 6

* [ ] Criar testes unitários completos
* [ ] Mockar Output Ports
* [ ] Garantir isolamento total

---

## 📅 DIA 7

* [ ] Adicionar novo adapter (ex: mensageria)
* [ ] Validar múltiplos adapters
* [ ] Revisar arquitetura

---

# 🛠️ 6. COMO ORGANIZAR NO AZURE DEVOPS

## 🔹 Tipo de Work Items

Use:

* **Epic**

  * Hexagonal Architecture Lab

* **Features**

  * Core Application
  * Input Adapters
  * Output Adapters
  * Testing Strategy

* **User Stories**

  * Criar UseCase
  * Criar Port
  * Criar Adapter

* **Tasks (Checklist diário)**

  * Cada item do checklist vira uma task

---

## 🔹 Exemplo de User Story

**Título:**
Criar UseCase de Processamento

**Descrição:**
Implementar lógica central da aplicação desacoplada de infraestrutura.

**Critérios de Aceite:**

* Não depende de framework
* Usa apenas ports
* Pode ser testado isoladamente

---

# 🧠 7. O QUE VOCÊ VAI INTERNALIZAR

Ao final desse laboratório, você vai dominar:

* Diferença real entre:

  * Arquitetura Hexagonal vs Clean
* Conceito verdadeiro:

  * Inside vs Outside ([alistaircockburn.com][2])
* Testabilidade real (sem infra)
* Plugabilidade de adapters
* Design orientado a ports (não camadas)

---

# 🚀 PRÓXIMO PASSO

Posso evoluir isso para:

👉 Um **template pronto (.NET 10)** com:

* Solution estruturada
* DI configurado
* Exemplo real (seu worker JSON → XML)

Ou:

👉 Transformar esse lab em um **projeto portfólio profissional (GitHub + README + diagramas)** para você usar como autoridade técnica.

Qual direção você quer seguir?

[1]: https://alistair.cockburn.us/hexagonal-architecture?utm_source=chatgpt.com "hexagonal-architecture"
[2]: https://alistaircockburn.com/hexarch%20v1.1b%20DIFFS%2020250420-1012%20paper%2Bepub.docx.pdf?utm_source=chatgpt.com "The updated pages to…"
