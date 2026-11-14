# Playtest Log

Registro **append-only** de testes, observações e aprendizados do **ProjetoAction**. Use para validar game feel, legibilidade e regressões. Índice: [[MOC]].

## Como usar
- Adicionar uma entrada datada por sessão (ou por hipótese testada).
- Manter fatos observados separados de interpretações; linkar ações/notas/ADRs quando houver.
- Focar em loops principais: interação, sacola LIFO, uso de itens, combate/percepção e feedback.
- Relacionar achados a [[notes/Feedback-GameFeel]], [[notes/Inventory]], [[notes/Player]], [[notes/Enemy]], [[architecture/Contract-Map]].

## Checklist de smoke test (rápido)

- [ ] **Conexão/Editor:** `unity status --until-ready` e `unity command editor status` OK (ver [[architecture/Unity-MCP-Connection]]).
- [ ] **Compilação:** recompilação sem erros; console sem erros novos relevantes.
- [ ] **Play Mode:** entra/sai sem travar; cena `SampleScene` carrega.
- [ ] **Interação/boca:** foca e pega item no alcance (`E`) com 1 item ativo por vez; não pega 2 simultaneamente.
- [ ] **Sacola LIFO:** guardar (`F`) empilha; recuperar (`F`) devolve o **topo**; sequência A,B,C → C,B,A; sacola cheia/vazia gera resultado claro.
- [ ] **Uso de itens:** usar, falhar (ex.: vazio/cooldown/bloqueio) e recarregar produzem resposta perceptível.
- [ ] **Dano/estado:** dano aplica corretamente; vida baixa e dano têm sinalização visível (jogador/inimigos quando aplicável).
- [ ] **Inimigos:** patrulha/navegação/ataque básicos funcionam sem regressão óbvia.

## Template de entrada

```md
## YYYY-MM-DD — Título curto (build/cena/contexto)
- **Cena/build:** SampleScene / commit ou estado local
- **Objetivo:** o que validar
- **Passos:** ações executadas
- **Esperado:** comportamento/feedback esperado
- **Observado:** o que aconteceu de fato
- **Achados:** bugs, confusões, notas de feel/legibilidade
- **Ações:** ajustes propostos, links ([[architecture/Decisions/ADR-XXXX]], notas, scripts)
- **Status:** aberto | mitigado | resolvido
```

## Entradas

### 2026-11-14 — Conexão Unity Pipeline/MCP confirmada
- **Cena/build:** ProjetoAction, Unity 6000.6.4f1 (PID 24428)
- **Objetivo:** validar acesso via CLI/Pipeline após recarregar pacote com editor em foco.
- **Passos:** `unity status --until-ready`, `unity pipeline list`, `unity command editor status`.
- **Esperado:** servidor pronto e alcançável.
- **Observado:** `7800 ready`, Pipeline 1.0.0 acessível (`Server Reachable: true`), `editor status: ready`, `compiling: false`, `playMode: stopped`, `Main Editor`.
- **Achados:** conexão operacional; recomendável `set_auto_tick` para automação em background.
- **Ações:** estado documentado em [[architecture/Unity-MCP-Connection]].
- **Status:** resolvido

### 2026-11-14 — Baseline de sistemas conhecidos (documental)
- **Cena/build:** estado atual do repositório (`SampleScene`)
- **Objetivo:** registrar baseline antes de novos incrementos de feedback/inventário.
- **Passos:** inspeção de scripts principais (`Health`, `Backpack` LIFO, `PlayerHand`, itens, inimigos).
- **Esperado:** n/a (baseline).
- **Observado:** loop básico de pegar/guardar(LIFO)/recuperar/usar/recarregar/jogar implementado; enums de vocabulário de feedback (`UseResult`, `ItemCategory`, `FeedbackCue`) presentes em `Enuns.cs`; contratos e feedback avançado descritos como evoluções aprovadas em ADRs.
- **Achados:** legibilidade/feedback world-space e sinalização 3D são prioridades aprovadas para validar em próximos playtests.
- **Ações:** acompanhar via [[architecture/Contract-Map]], [[notes/Feedback-GameFeel]], ADR-0001..0004.
- **Status:** aberto (para validação em playtest)