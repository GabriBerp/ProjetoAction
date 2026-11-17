# Guia do Second Brain (uso e manutenção)

Como manter o Second Brain do **ProjetoAction** útil, confiável e enxuto. Índice: [[MOC]].

## Objetivo
- Centralizar conhecimento de **alto nível**: domínios, fluxos, decisões, contratos e aprendizados.
- Evitar *doc rot*: não espelhar implementação linha a linha; documentar conceitos estáveis e “porquês”.

## Estrutura e responsabilidades

| Área | Conteúdo | Quando mexer |
|---|---|---|
| `docs/MOC.md` | Hub, estado atual, mapa de navegação e grafo | Sempre que criar/mover notas relevantes ou mudar foco/estado |
| `docs/architecture/Overview.md` | Visão de módulos, fluxos e princípios | Mudanças estruturais de domínio/fluxo |
| `docs/architecture/Contract-Map.md` | Interfaces/enums, implementadores/consumidores, estado (atual vs planejado) | Qualquer mudança em `Contracts.cs`/`Enuns.cs` ou integrações |
| `docs/architecture/Decisions/` | ADRs e template | Decisões com trade-off ou impacto multi-sistema |
| `docs/architecture/Unity-CodeMode-Connection.md` | Estado e uso do Code Mode (conector `unity` + Pipeline CLI) | Mudança no estado de conexão/ferramenta ou procedimento |
| `docs/architecture/Unity-MCP-Connection.md` | Nota substituída do caminho MCP (histórico) | Só para preservar histórico/contexto |
| `docs/notes/*.md` | Conceitos por domínio (Player, Inventory, Enemy, World, Feedback) | Novos sistemas/fluxos estáveis ou mudanças de semântica |
| `docs/notes/Playtest-Log.md` | Evidências datadas de testes e achados | Após sessões de playtest/smoke relevantes |
| `docs/notes/Glossary.md` | Termos canônicos do domínio | Surgimento de termo ambíguo ou importante |
| `docs/ops/` | Guias operacionais do conhecimento | Ajustes de processo |

## Rotina de atualização (gatilhos)

1. **Mudou `Assets/Scripts/Contracts.cs` ou `Enuns.cs`:**
   - Atualizar [[architecture/Contract-Map]] (tabelas, estado, grafo).
   - Se houver mudança de semântica/assinatura/regra, criar ou referenciar ADR em [[architecture/Decisions/README]].
2. **Tomou decisão de design/arquitetura:**
   - Criar ADR a partir de [[architecture/Decisions/_template]] e indexar no README.
   - Linkar notas afetadas ([[notes/Player]], [[notes/Inventory]], [[notes/Feedback-GameFeel]], etc.).
3. **Implementou ou alterou fluxo estável:**
   - Atualizar a nota de domínio correspondente e o grafo do [[MOC]] se necessário.
4. **Rodou playtest/smoke:**
   - Adicionar entrada em [[notes/Playtest-Log]] (objetivo, passos, esperado, observado, achados, ações).
   - Se revelar decisão pendente, abrir ADR `Proposto`.
5. **Mudou estado de ferramentas/editor:**
   - Atualizar [[architecture/Unity-CodeMode-Connection]] (data, porta, pronto/não, versão).

## Convenções

- **Wikilinks:** usar `[[pasta/Nota]]` ou `[[MOC]]` para navegação conceitual (ex.: `[[architecture/Contract-Map]]`, `[[notes/Inventory]]`).
- **Mermaid:** diagramas para fluxos/dependências principais (não para cada método); preferir `flowchart`/`sequenceDiagram` simples.
- **Estado explícito:** marcar `Atual` vs `Planejado/Aprovado/Concluído` em mapas e notas quando houver diferença entre código e intenção.
- **Append-only onde importa:** ADRs aceitos preservam histórico (revisões via novo ADR/atualização datada); Playtest Log só adiciona entradas.
- **Linguagem:** fatos observáveis, termos do [[notes/Glossary]], sem adjetivação vaga.

## Checklist de saúde (periódico)

- [ ] `MOC` aponta para todas as notas ativas e reflete o foco/estado atual.
- [ ] Todos os `[[wikilinks]]` usados no `MOC` e notas principais têm arquivo correspondente.
- [ ] `Contract-Map` alinhado a `Contracts.cs`/`Enuns.cs` (e mudanças planejadas marcadas).
- [ ] ADRs relevantes indexados no `Decisions/README` com status/data corretos.
- [ ] `Unity-CodeMode-Connection` reflete o último estado verificado.
- [ ] `Playtest-Log` tem registro das validações recentes/importantes.
- [ ] Notas permanecem de alto nível (sem duplicar código volátil).

## Relações
- Hub: [[MOC]]
- Arquitetura: [[architecture/Overview]], [[architecture/Contract-Map]]
- Decisões: [[architecture/Decisions/README]]
- Domínios: [[notes/Player]], [[notes/Inventory]], [[notes/Enemy]], [[notes/World-Scenes]], [[notes/Feedback-GameFeel]]
- Aprendizado: [[notes/Playtest-Log]], [[notes/Glossary]]