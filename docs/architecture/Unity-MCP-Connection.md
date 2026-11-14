# Unity MCP / Pipeline Connection

Integração do projeto Unity aberto no editor com ferramentas via **Unity Pipeline CLI** (`unity`). Referência geral: [[architecture/Overview]] | Índice: [[MOC]].

## Estado verificado (confirmado)

- **Projeto:** `C:\Users\Educação\ProjetoAction`
- **Editor Unity:** em execução (PID 24428, Unity 6000.6.4f1)
- **Pacote:** `com.unity.pipeline` **1.0.0** instalado no projeto
- **Servidor Pipeline:** **acessível e pronto** — porta **7800** (`Server Reachable: true`)
- **`editor status`:** `status: "ready"`, `compiling: false`, `domainReloadInProgress: false`, `playMode: "stopped"`, instância `Main Editor`
- **Última confirmação:** 2026-11-14 (após foco na janela do Unity para recarregar o pacote)

> Se o estado mudar (editor fechado, domínio recarregando, pacote ausente), atualize esta seção com nova data/resultado.

## Como conectar / reconectar

1. Confirmar que o Unity Editor está aberto neste projeto.
2. Instalar/atualizar o pacote (se necessário):
   ```bash
   unity pipeline install
   ```
3. Trazer a janela do Unity para o foco para o editor recarregar o pacote.
4. Aguardar e verificar prontidão:
   ```bash
   unity status --until-ready
   unity pipeline list
   unity command editor status
   ```

## Comandos úteis (após conexão)

```bash
# Recomendado para automação/headless (evita throttling com janela em segundo plano)
unity command editor set_auto_tick --enabled true

# Compilação
unity command script recompile
unity command script recompile_status

# Testes
unity command test list --mode editor
```

## Troubleshooting rápido
- **Servidor não alcançável / instância sem Pipeline:** garantir pacote instalado, focar janela do Unity, aguardar domain reload e repetir `unity status --until-ready`.
- **Comandos lentos/travando com editor em background:** habilitar `set_auto_tick`.
- **Estado ambíguo:** consultar `unity pipeline list` e `unity command editor status` para detalhes (porta, versão, erros).

## Relações
- Permite inspecionar/controlar elementos de [[notes/World-Scenes]] e validar mudanças em [[notes/Player]], [[notes/Inventory]], [[notes/Enemy]] e [[notes/Feedback-GameFeel]].
- Resultados de verificação/playtest devem ser registrados em [[notes/Playtest-Log]].
- Parte das diretrizes operacionais do projeto (skill `unity-pipeline`).