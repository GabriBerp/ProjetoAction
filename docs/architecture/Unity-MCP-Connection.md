# Unity MCP / Pipeline Connection

Integração do projeto Unity aberto no editor com ferramentas via **Unity Pipeline CLI** (`unity`).

## Estado verificado (nesta máquina)

- Projeto: `C:\Users\Educação\ProjetoAction`
- Editor Unity em execução: **sim** (PID 24428, Unity 6000.6.4f1).
- Pacote `com.unity.pipeline`: instalado no projeto (`unity pipeline install` executado).
- Servidor do Pipeline acessível: **ainda não** (nenhuma instância pronta em `unity status`).

## Como conectar

1. Confirmar que o Unity Editor está aberto neste projeto.
2. Instalar/atualizar o pacote:
   ```bash
   unity pipeline install
   ```
3. Fazer o editor recarregar o pacote (normalmente ao ganhar foco na janela do Unity).
4. Aguardar o servidor ficar pronto:
   ```bash
   unity status --until-ready
   unity pipeline list
   unity command editor status
   ```

> Se `status` ficar em estado pendente de carregamento, traga a janela do Unity para frente e rode `unity status --until-ready` novamente.

## Comandos úteis após conexão

```bash
unity command editor set_auto_tick --enabled true
unity command script recompile
unity command script recompile_status
unity command test list --mode editor
```

## Relações

- Permite inspecionar/controlar elementos de [[notes/World-Scenes]] e validar mudanças nos domínios [[notes/Player]], [[notes/Inventory]] e [[notes/Enemy]].
- Índice geral: [[MOC]].