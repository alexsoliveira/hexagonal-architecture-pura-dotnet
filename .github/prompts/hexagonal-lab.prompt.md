# Hexagonal Architecture Lab (.NET 10)

## Objective

Build a complete application using Hexagonal Architecture (Ports & Adapters), following the original concept by Alistair Cockburn.

The application must:
- Run without database
- Run without API
- Be fully testable

---

## Tasks

### Phase 1 — Core

- Create Core project
- Implement:
  - Models
  - Input Ports
  - UseCases

### Phase 2 — Output Ports

- Define Output Ports
- Create Fake Adapters (in-memory)

### Phase 3 — Input Adapter

- Create API adapter
- Connect endpoints to Input Ports

### Phase 4 — Output Adapter

- Implement real adapter (DB or API)

### Phase 5 — Multiple Adapters

- Add Worker adapter
- Reuse UseCases

### Phase 6 — Testing

- Create unit tests
- Mock Output Ports

---

## Expected Structure

/src
 ├── Core
 ├── Adapters.In
 ├── Adapters.Out
 └── Bootstrap

---

## Rules

- Core must not depend on any framework
- All dependencies must go through Ports
- Adapters must be easily replaceable

---

## When generating code

- Always show:
  - Ports
  - UseCase
  - Adapter implementation

- Keep code minimal and clear