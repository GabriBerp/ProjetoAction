# ADR-0001: Inventário como sacola LIFO (tensão cômica)

- **Status:** Aceito
- **Data:** 2026-11-14
- **Relacionado:** [[MOC]], [[architecture/Overview]], [[architecture/Contract-Map]], [[notes/Inventory]], [[notes/Player]], [[notes/Feedback-GameFeel]], [[notes/Playtest-Log]]

## Contexto
O projeto possui `Assets/Scripts/Backpack.cs` com armazenamento em `Stack<IPickable>` (LIFO), integrado a `Assets/Scripts/PlayerHand.cs` (guardar/recuperar com `F`) e a itens coletáveis (`Pickable`/`IPickable`). O jogador carrega itens encontrados na aventura e precisa escolher o momento de uso.

Deseja-se uma dinâmica de decisão durante a exploração: recuperar um item específico pode exigir abrir mão temporariamente de outros.

## Decisão
- O inventário é uma **sacola** com semântica **LIFO** (Last-In, First-Out).
- **Guardar:** o item vai para o topo da sacola.
- **Recuperar:** sempre retorna o item do **topo** (último guardado). Não há seleção direta de slots no fluxo padrão.
- A dificuldade/atrito é **intencional e cômica**: um item valioso guardado cedo pode ficar no fundo, forçando o jogador a descartar/gerenciar itens do topo para acessá-lo, criando tensão leve.
- Consultas/eventos de estado (contagem, capacidade, topo, mudanças) podem existir para feedback, mas **não devem quebrar** o comportamento LIFO como padrão.

## Alternativas consideradas
- **Inventário com seleção livre (hotbar/lista):** acesso rápido e menos fricção, mas elimina a tensão cômica desejada.
- **Fila (FIFO):** previsível de outra forma, porém não comunica a metáfora de “sacola” empilhada.
- **LIFO interno com “view” selecionável:** flexível, mas arrisca diluir a mecânica se a seleção virar padrão; só aceitável se LIFO permanecer o comportamento de recuperar padrão.

## Consequências
### Positivas
- Mecânica simples de entender e com identidade (humor + pressão de escolha).
- Implementação alinhada ao `Stack` atual e de baixo custo cognitivo.

### Negativas / Riscos
- Pode frustrar se a legibilidade for baixa (jogador não sabe o que está no topo/fundo).
- Exige feedback claro (ações de guardar/recuperar/jogar, indicação de topo/capacidade) — ver [[architecture/Decisions/ADR-0003-feedback-world-space]].

## Impacto técnico
- **Scripts centrais:** `Assets/Scripts/Backpack.cs`, `Assets/Scripts/PlayerHand.cs`, `Assets/Scripts/Pickable.cs`.
- **Contratos atuais:** `IPickable`, `IHoldListener`, `IUsable`, `IReloadable` em `Assets/Scripts/Contracts.cs`.
- **Evoluções planejadas/aprovadas:** eventos/consultas de inventário (ex.: item guardado/recuperado, topo, cheio/vazio) e metadados de item para prompts, mantendo LIFO; enums de feedback/categoria em `Assets/Scripts/Enuns.cs` (`FeedbackCue`, `ItemCategory`, `UseResult`) como suporte a sinalização.
- **Compatibilidade:** preservar `TryStore`/`TryRetrieve` e semântica de topo.

## Plano de validação
- Playtest: guardar A, B, C → recuperar devolve C, depois B, depois A; sacola cheia bloqueia guardar com feedback claro; recuperar em sacola vazia falha com feedback.
- Verificar que feedback 3D/world-space comunica topo/resultado das ações (ver [[notes/Playtest-Log]]).
