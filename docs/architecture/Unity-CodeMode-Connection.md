# Unity Code Mode Connection

Integração do projeto Unity aberto no editor com ferramentas via **Code Mode** (uma ferramenta de código) sobre o **Unity Pipeline CLI** (`unity`). Substitui o caminho de cliente MCP. Referência geral: [[architecture/Overview]] | Índice: [[MOC]] | Decisão: [[architecture/Decisions/ADR-0005-codemode-em-vez-de-mcp]].

## Estado verificado (2026-11-17)

- **Projeto:** `C:\Users\Educação\ProjetoAction`
- **Editor Unity:** em execução (PID 14040, Unity 6000.6.4f1)
- **Servidor Pipeline:** **acessível e pronto** — porta **7801**
- **`editor status`:** `status: "ready"`, `compiling: false`, `domainReloadInProgress: false`, `playMode: "stopped"`, instância `Main Editor`
- **`com.unity.pipeline`:** 1.0.0 | **CLI:** `unity` 1.0.0 (`C:\Users\Educação\AppData\Local\Unity\bin\unity.exe`)
- **Catálogo de comandos do editor:** 196 comandos
- **MCP:** não é mais o caminho de integração (nenhuma configuração de cliente MCP no repositório). `unity mcp` continua existindo no CLI, porém não é usado por este projeto.

> Se o estado mudar (editor fechado, porta diferente, domínio recarregando, pacote ausente), atualize esta seção com nova data/resultado.

## O modelo mental (Code Mode)

Ao invés de uma chamada de ferramenta por operação, o agente recebe **uma única ferramenta** (`codemode`) que executa um programa JavaScript. O programa chama métodos do conector `unity`, encadeia resultados, filtra e devolve apenas o valor final. Um round trip do modelo = um plano executável.

- **Entrada:** `{ code: string }`
- **Saída:** `{ status: "completed" | "error", executionId, result?, error?, logs?, calls? }`
- **Conector:** `unity` — cada método é uma invocação do Pipeline CLI.

Referência do padrão: [Code Mode — Cloudflare Docs](https://developers.cloudflare.com/agents/tools/codemode/).

## Contrato do conector `unity`

| Método | Efeito |
|---|---|
| `unity.status()` | Instâncias de editor conectadas (porta, projeto, versão, pid, estado) |
| `unity.pipeline()` | Servidores/pacote Pipeline alcançáveis |
| `unity.commands({ query?, tag?, limit?, groupBy?, detail? })` | Lista/busca os comandos do editor (catálogo real) |
| `unity.command(name, args?)` | Executa um comando do editor, ex. `'scene get_hierarchy'`, `{ path: 'Assets/Scenes/SampleScene.unity' }` |
| `unity.recompile({ path? })` / `unity.recompileStatus()` | Recompila scripts e consulta o resultado |
| `unity.console({ tail?, level? })` / `unity.consoleStatus()` | Console do editor (entradas, contadores, `compilationFailed`) |
| `unity.tests({ mode? })` | Lista testes EditMode/PlayMode |
| `unity.playmode('start' | 'stop' | 'pause')` | Controla play mode |
| `unity.saveAll()` / `unity.setAutoTick({ enabled })` | Salva cenas abertas / mantém o editor tiquetaqueando sem foco |
| `unity.cwd()` | Raiz do projeto usada nas chamadas |

## Descoberta progressiva

O catálogo completo não entra no contexto: o `unity.commands()` e o `codemode.search()` localizam o que é necessário.

```bash
# Declarações TypeScript do conector (o que o modelo vê)
node tools/unity-codemode/run.mjs --describe

# Busca por método
node tools/unity-codemode/run.mjs --search "console"
```

Dentro do programa: `codemode.search("hierarquia da cena")`, `codemode.describe("unity.command")` e `codemode.step("nome", () => ...)` para trabalho não determinístico.

## Como executar

```bash
# Programa inline (corpo de função async; `return` devolve o resultado)
node tools/unity-codemode/run.mjs --code "return unity.status()"

# Programa em arquivo, ou por stdin
node tools/unity-codemode/run.mjs --file plan.js
echo "return unity.command('editor status')" | node tools/unity-codemode/run.mjs

# Forma de arrow function também é aceita
node tools/unity-codemode/run.mjs --code "async () => (await unity.status())[0].state"
```

Opções: `--timeout <ms>` (orçamento de execução, padrão 180000), `--allow-destructive`, `--project <path>`.

Exemplo composto (uma execução, quatro chamadas, resultado filtrado):

```js
const [instances, editor, scenes] = await Promise.all([
  unity.status(),
  unity.command('editor status'),
  unity.command('scene list_open'),
]);

return {
  editor: { status: editor.status, compiling: editor.compiling, playMode: editor.playMode },
  instances: instances.map(i => ({ port: i.port, pid: i.pid, state: i.state })),
  scenes: (Array.isArray(scenes) ? scenes : []).map(s => ({ path: s.path, dirty: s.isDirty })),
};
```

## Garantias do runtime

- **Sandbox `node:vm`:** sem `require`, `process`, `fetch` ou acesso ao sistema de arquivos; apenas `console`, `codemode` e o conector `unity`.
- **Serialização:** o Pipeline atende um comando por vez. `Promise.all` é permitido no programa, mas as chamadas ao CLI entram em fila — sem falhas de "Editor busy".
- **Orçamento de tempo:** cada execução tem deadline; chamadas ao CLI recebem o tempo restante.
- **Bloqueio de comandos destrutivos:** comandos marcados `destructive` no catálogo do editor são recusados, a menos que se rode com `--allow-destructive` após aprovação explícita. Ex.: `asset delete`, `gameobject delete`, `editor quit`.
- **Erros como dado:** falhas retornam `status: "error"` com mensagem do editor (`errors[].message`), sem derrubar o runtime.

## Troubleshooting rápido

- **Servidor não alcançável / instância sem Pipeline:** garantir o pacote `com.unity.pipeline` instalado, focar a janela do Unity, aguardar domain reload e repetir `unity status --until-ready`.
- **Comandos lentos com editor em background:** `unity.setAutoTick({ enabled: true })` (ou `unity command editor set_auto_tick --enabled true`).
- **Erro "has no parameter --…":** opções do CLI precisam vir antes do nome do comando; parâmetros do comando do editor vão depois. O conector já respeita essa ordem.
- **Estado ambíguo:** consultar `unity.command('editor status')` e `unity.pipeline()` (porta, versão, erros).

## Relações

- Substitui [[architecture/Unity-MCP-Connection]] (nota histórica do caminho MCP).
- Permite inspecionar/controlar elementos de [[notes/World-Scenes]] e validar mudanças em [[notes/Player]], [[notes/Inventory]], [[notes/Enemy]] e [[notes/Feedback-GameFeel]].
- Resultados de verificação/playtest devem ser registrados em [[notes/Playtest-Log]].
- Parte das diretrizes operacionais do projeto (skills `unity-pipeline` / `unity-*`).
