# Glossário — ProjetoAction

Definições curtas para padronizar linguagem no design, código e documentação. Índice: [[MOC]].

| Termo | Definição | Onde aparece / relações |
|---|---|---|
| **Sacola (Backpack)** | Inventário do jogador com semântica **LIFO**: o último item guardado fica no topo e é o primeiro a ser recuperado. Mecânica intencional de tensão cômica. | `Assets/Scripts/Backpack.cs`, [[notes/Inventory]], [[architecture/Decisions/ADR-0001-inventario-sacola-lifo]] |
| **LIFO (topo/fundo)** | Last-In, First-Out. Guardar empilha no topo; recuperar/remove o topo. Item valioso guardado cedo tende a ficar no fundo. | [[notes/Inventory]], [[architecture/Contract-Map]] |
| **Boca (hold point)** | Ponto onde o jogador segura o item ativo. No design é a **boca** do rato (1 item por vez); no código pode aparecer como “Hand”/`holdPoint`. | `Assets/Scripts/PlayerHand.cs`, `Pickable`, [[notes/Player]], [[architecture/Decisions/ADR-0002-interacao-boca-um-item]] |
| **Pickable (coletável)** | Objeto que pode ser pego, segurado e solto/lançado; implementa `IPickable` (e reage a listeners). | `Assets/Scripts/Pickable.cs`, [[notes/Inventory]], [[notes/World-Scenes]] |
| **Interagível / Foco** | Alvo no mundo que pode ser acionado quando em alcance e sob mira (raycast). Foco deve ser indicado visualmente (planejado/aprovado). | [[notes/Player]], [[notes/Feedback-GameFeel]], [[architecture/Decisions/ADR-0003-feedback-world-space]] |
| **Prompt world-space** | Indicação textual/visual no espaço 3D (billboard/marcador) sobre ação possível ou estado. | [[notes/Feedback-GameFeel]], [[architecture/Contract-Map]] |
| **Feedback de uso** | Resposta visual/espacial ao tentar usar item: sucesso, falha, vazio, cooldown, recarga, toggle etc. | `UseResult`/`FeedbackCue` em `Enuns.cs`, [[notes/Feedback-GameFeel]], [[architecture/Decisions/ADR-0004-contratos-evolucao-compativel]] |
| **Sinalização 3D de dano/estado** | Marcadores visuais no mundo (flash, tint, pulso, indicador) para dano, vida baixa ou estados de entidades (jogador/inimigos). | `Assets/Scripts/Health.cs`, [[notes/Player]], [[notes/Enemy]], [[notes/Feedback-GameFeel]] |
| **Rat size / perfil** | Variação de tamanho/escala e atributos associados ao rato. | `Assets/Scripts/RatSizeProfile.cs`, `RatSize` em `Enuns.cs`, [[notes/Player]], [[architecture/Contract-Map]] |
| **Contrato** | Interface/tipo compartilhado que define fronteiras entre sistemas (ex.: `IUsable`, `IPickable`, `IHealth`). | `Assets/Scripts/Contracts.cs`, [[architecture/Contract-Map]] |
| **ADR** | Architecture Decision Record — registro curto de contexto, decisão e consequências. | [[architecture/Decisions/README]] |
| **Playtest log** | Registro datado de sessões de teste, resultados observados e aprendizados/ações. | [[notes/Playtest-Log]] |
| **Pipeline/MCP CLI** | Ferramenta `unity` para conectar e operar o editor Unity (status, recompile, testes, comandos). | [[architecture/Unity-MCP-Connection]] |