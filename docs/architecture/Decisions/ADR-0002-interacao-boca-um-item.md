# ADR-0002: Interação pela boca, um item por vez

- **Status:** Aceito
- **Data:** 2026-11-14
- **Relacionado:** [[MOC]], [[architecture/Overview]], [[architecture/Contract-Map]], [[notes/Player]], [[notes/Inventory]], [[notes/Feedback-GameFeel]]

## Contexto
`Assets/Scripts/PlayerHand.cs` gerencia pegar (`E`), usar (mouse), recarregar (`R`), guardar/recuperar (`F`) e arremessar (`Q`) com um `holdPoint` (filho da câmera). No design do jogo, o personagem é um rato que **carrega itens com a boca**, o que reforça a fantasia e limita naturalmente a carga ativa.

## Decisão
- O jogador **pega e segura itens com a boca**. A nomenclatura de código (`PlayerHand`, “mão”, `holdPoint`) pode permanecer por compatibilidade, mas a semântica de design é **boca**.
- **Limite estrito:** apenas **1 item ativo** segurado por vez.
- Guardar na sacola libera a boca; recuperar pega o topo da sacola (ver [[architecture/Decisions/ADR-0001-inventario-sacola-lifo]]). Trocas exigem guardar/jogar antes de pegar outro.
- Interações devem priorizar alvos focados no alcance (raycast) e fornecer indicação clara do alvo/ação no mundo 3D (ver [[architecture/Decisions/ADR-0003-feedback-world-space]]).

## Alternativas consideradas
- **Duas “mãos”/slots ativos:** aumenta complexidade de input/UI e enfraquece a identidade do rato.
- **Segurar múltiplos itens visíveis:** custo de animação/estado alto e ambiguidade de uso.

## Consequências
### Positivas
- Regras simples e legíveis; reforça tema “rato”.
- Sinergia com sacola LIFO: decisões de troca ficam mais significativas.

### Negativas / Riscos
- Pode parecer restritivo sem feedback adequado (por que não pegou, o que está segurando, como trocar).
- Dependência de animações/posicionamento (`heldLocalPosition/Rotation` em `Pickable`, drivers de animação) para comunicar “boca”.

## Impacto técnico
- **Scripts:** `Assets/Scripts/PlayerHand.cs`, `Assets/Scripts/Pickable.cs`, `Assets/Scripts/PlayerAnimationDriver.cs`, `Assets/Scripts/PlayerMoveInput.cs` (contexto de input).
- **Contratos:** `IPickable` (`IsHeld`, `PickUp`, `Drop`), `IHoldListener`.
- **Evoluções planejadas/aprovadas:** camada de detecção de foco/interagível e eventos de interação/inventário para prompts e feedback 3D (sem alterar a regra de 1 item).

## Plano de validação
- Não é possível segurar 2 itens simultaneamente; tentar pegar com boca ocupada não duplica nem perde estado.
- Guardar/recuperar/jogar atualizam corretamente o item ativo e disparam feedback visível.
- Prompt/highlight indica o item focado quando a boca está livre.
