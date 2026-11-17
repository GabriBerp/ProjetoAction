# Playtest Log

Registro **append-only** de testes, observações e aprendizados do **ProjetoAction**. Use para validar game feel, legibilidade e regressões. Índice: [[MOC]].

## Como usar
- Adicionar uma entrada datada por sessão (ou por hipótese testada).
- Manter fatos observados separados de interpretações; linkar ações/notas/ADRs quando houver.
- Focar em loops principais: interação, sacola LIFO, uso de itens, combate/percepção e feedback.
- Relacionar achados a [[notes/Feedback-GameFeel]], [[notes/Inventory]], [[notes/Player]], [[notes/Enemy]], [[architecture/Contract-Map]].

## Checklist de smoke test (rápido)

- [ ] **Conexão/Editor:** `node tools/unity-codemode/run.mjs --code "return unity.status()"` OK e `unity.command('editor status')` com `status: ready` (ver [[architecture/Unity-CodeMode-Connection]]).
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
- **Ações:** estado documentado em [[architecture/Unity-MCP-Connection]] (nota substituída em 2026-11-17).
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

### 2026-11-17 — Integração via Code Mode (substitui MCP)
- **Cena/build:** ProjetoAction, Unity 6000.6.4f1 (PID 14040), Pipeline na porta 7801
- **Objetivo:** validar a conexão com o Editor pela ferramenta única de Code Mode (`tools/unity-codemode`)
- **Passos:** `--describe`; `--search "play mode"`; execução composta com `unity.status()`, `unity.command('editor status')`, `unity.command('scene list_open')` e `unity.consoleStatus()` no mesmo programa; loop + filtro sobre `unity.commands({ query: 'scene' })`; tentativa de `unity.command('asset delete', …)`.
- **Esperado:** declarações e métodos descobertos; uma execução composta devolvendo só o resultado filtrado; comando destrutivo recusado.
- **Observado:** `completed` com `status: ready`, `compiling: false`, `playMode: stopped`, `SampleScene` ativa e não-dirty; 27 comandos de cena encontrados e filtrados; `asset delete` recusado como destrutivo.
- **Achados:** (1) o Pipeline atende um comando por vez — chamadas em `Promise.all` falham com "Editor busy", então o conector serializa na fila; (2) opções do CLI precisam vir antes do nome do comando, pois o que vem depois é interpretado como parâmetro do comando do editor.
- **Ações:** runtime em `tools/unity-codemode`, decisão em [[architecture/Decisions/ADR-0005-codemode-em-vez-de-mcp]], estado em [[architecture/Unity-CodeMode-Connection]].
- **Status:** resolvido