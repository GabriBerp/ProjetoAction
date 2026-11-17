# Unity MCP Connection (substituída)

- **Status:** **Substituída** em 2026-11-17 por [[architecture/Unity-CodeMode-Connection]].
- **Motivo:** a integração passou a usar **Code Mode** (uma ferramenta de código compondo comandos do Pipeline CLI), em vez de um cliente MCP com uma chamada por ferramenta. Ver [[architecture/Decisions/ADR-0005-codemode-em-vez-de-mcp]].
- **Histórico preservado:** a conexão via Pipeline CLI (`com.unity.pipeline`, porta 7800) foi verificada em 2026-11-14 e está registrada em [[notes/Playtest-Log]]. Em 2026-11-17 a porta observada é **7801** (editor PID 14040), estado `ready`.

## Estado atual

Não usar esta nota como procedimento. Para conexão, comandos, descoberta e troubleshooting, ver [[architecture/Unity-CodeMode-Connection]].

O comando `unity mcp` do CLI continua disponível (servidor/cliente MCP do Editor), porém **não é o caminho adotado** neste projeto e não há configuração de cliente MCP no repositório.
