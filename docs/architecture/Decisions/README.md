# Decisões de Arquitetura (ADRs)

Registro de decisões relevantes do **ProjetoAction**. ADRs capturam *o porquê* das escolhas para evitar re-discussões e orientar mudanças futuras.

- Índice geral: [[MOC]]
- Visão técnica: [[architecture/Overview]]
- Contratos e tipos: [[architecture/Contract-Map]]
- Template: [[architecture/Decisions/_template]]

## Índice

| ID | Título | Status | Data | Notas-chave |
|---|---|---|---|---|
| ADR-0001 | Inventário como sacola LIFO (tensão cômica) | Aceito | 2026-11-14 | LIFO padrão; topo = último guardado; recuperar exige descartar topo |
| ADR-0002 | Interação pela boca, um item por vez | Aceito | 2026-11-14 | Nomenclatura “Hand/holdPoint” no código, semântica de boca no design |
| ADR-0003 | Feedback prioritariamente world-space/cena/câmera | Aceito | 2026-11-14 | HUD 2D mínimo; placeholders visuais simples; sinalização 3D de dano/estado |
| ADR-0004 | Evolução compatível de contratos e feedback de uso | Aceito | 2026-11-14 | Manter `IUsable.Use()`; adicionar canais de resultado/eventos sem quebrar chamadores |
| ADR-0005 | Integração com o Editor via Code Mode (em vez de MCP) | Aceito | 2026-11-17 | Ferramenta única `codemode`; conector `unity` sobre o Pipeline CLI; descoberta progressiva; bloqueio de comandos destrutivos |

## Quando criar/atualizar um ADR
- Escolha de mecânica ou fluxo com impacto em múltiplos sistemas (ex.: inventário, interação, percepção, save).
- Mudança em contratos/enums ou em fronteiras entre Player/Inventory/Enemy/Feedback.
- Trade-offs de UX/game feel (ex.: tipo de feedback, legibilidade, dificuldade intencional).
- Decisões de integração/tooling (ex.: Pipeline/Code Mode, fluxo de validação) quando afetarem o time/processo.

## Convenções
- **Status:** `Proposto` → `Aceito` → `Revisado` (ou `Substituído` com link para o novo ADR).
- **Imutabilidade prática:** ADR aceito não é reescrito para mudar o passado; revisões geram novo ADR ou seção de atualização datada.
- **Rastreabilidade:** cada ADR lista contratos, notas e sistemas afetados via `[[wikilinks]]`.
- **Alinhamento:** mudanças em `Contracts.cs`/`Enuns.cs` devem atualizar [[architecture/Contract-Map]] e citar o ADR.
