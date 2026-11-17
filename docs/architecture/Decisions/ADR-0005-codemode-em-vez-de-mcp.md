# ADR-0005: Integração com o Editor Unity via Code Mode (em vez de MCP)

- **Status:** Aceito
- **Data:** 2026-11-17
- **Relacionado:** [[MOC]], [[architecture/Overview]], [[architecture/Unity-CodeMode-Connection]], [[architecture/Unity-MCP-Connection]], [[notes/Playtest-Log]]

## Contexto
A integração com o Editor usava o caminho MCP (`unity mcp`): uma ferramenta por comando do editor. O catálogo atual tem **196 comandos**. Nesse formato, cada operação é um round trip do modelo, o catálogo completo ocupa contexto e cada resultado intermediário volta para o modelo antes da próxima decisão. Verificações reais do projeto (status do editor, cena aberta, console, hierarquia) exigem várias operações encadeadas.

O padrão [Code Mode](https://developers.cloudflare.com/agents/tools/codemode/) troca isso por **uma ferramenta** que recebe código: o programa chama os métodos, encadeia, filtra e devolve só o valor final.

## Decisão
- Adotar **Code Mode** como caminho de integração com o Editor: ferramenta única `codemode` com entrada `{ code }` e saída `{ status, executionId, result?, error?, logs?, calls? }`.
- Expor o Editor como **conector `unity`** (`tools/unity-codemode/`), com métodos tipados que invocam o **Pipeline CLI** (`unity`) — sem transporte MCP.
- Manter **descoberta progressiva**: `--describe`/`--search` e `codemode.search()/describe()` em vez de carregar 196 definições por requisição.
- Manter segurança equivalente à aprovação de MCP: comandos marcados `destructive` no catálogo do editor são bloqueados por padrão (`--allow-destructive` só com aprovação explícita).
- Manter `unity mcp` disponível no CLI, mas fora do fluxo do projeto.

## Alternativas consideradas
- **Manter MCP:** simples, porém um round trip por comando, catálogo grande no contexto e resultados intermediários consumindo contexto.
- **MCP dentro do runtime Code Mode (`McpConnector` + Worker):** é o caminho indicado para agentes hospedados na Cloudflare; aqui não há Worker e o Editor é local (loopback), então o ganho seria só de empacotamento, com custo de infraestrutura.
- **Scripts ad-hoc soltos por tarefa:** funcionam, mas sem contrato, sem descoberta e sem guarda de comandos destrutivos.

## Consequências
### Positivas
- Uma execução cobre várias operações (status + cena + console) com um único round trip.
- Resultados intermediários são filtrados no programa; o contexto recebe só o necessário.
- Fluxos repetíveis viram programas legíveis, não sequências de chamadas de ferramenta.
- Guarda de comandos destrutivos e orçamento de tempo explícitos.

### Negativas / Riscos
- Runtime local próprio (Node) para manter, em vez de transporte padronizado.
- O Pipeline atende um comando por vez: o conector serializa as chamadas (concorrência no programa não vira concorrência no editor).
- Sem aprovação durável/replay como no runtime hospedado do Cloudflare; a guarda é por catálogo + flag.

## Impacto técnico
- **Novo:** `tools/unity-codemode/` (`run.mjs`, `runtime.mjs`, `connectors/unity.mjs`, `package.json`), sem dependências externas.
- **Novo:** [[architecture/Unity-CodeMode-Connection]]; [[architecture/Unity-MCP-Connection]] passa a nota substituída.
- **Não muda:** código do jogo em `Assets/Scripts`, pacote `com.unity.pipeline`, cena `SampleScene`.

## Plano de validação
- `--describe` e `--search` retornam declarações e métodos do conector.
- Uma execução composta (`unity.status`, `editor status`, `scene list_open`, `console status`) devolve o resultado filtrado com `status: "completed"`.
- Loop + filtro sobre o catálogo (`unity.commands({ query: 'scene' })`) devolve os comandos esperados.
- Comando destrutivo (`asset delete`) é recusado sem `--allow-destructive`.
- Registrar o smoke em [[notes/Playtest-Log]] e manter o estado em [[architecture/Unity-CodeMode-Connection]].
