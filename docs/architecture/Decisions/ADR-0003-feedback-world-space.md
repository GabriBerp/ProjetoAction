# ADR-0003: Feedback prioritariamente world-space, cena e câmera

- **Status:** Aceito
- **Data:** 2026-11-14
- **Relacionado:** [[MOC]], [[notes/Feedback-GameFeel]], [[notes/Player]], [[notes/Inventory]], [[notes/Enemy]], [[architecture/Contract-Map]], [[architecture/Decisions/ADR-0001-inventario-sacola-lifo]], [[architecture/Decisions/ADR-0002-interacao-boca-um-item]]

## Contexto
O projeto precisa comunicar melhor estado e ações ao jogador: vida baixa, resultado/efeito de uso de itens, interações disponíveis e mudanças de inventário. A experiência alvo é imersiva em 3D (perspectiva de rato), e a sacola LIFO exige legibilidade para não gerar frustração opaca.

## Decisão
- **Prioridade de feedback:** world-space/billboards, elementos na cena e efeitos de câmera (ex.: vinheta/intensidade/pulso, shake/impulso, marcadores no mundo).
- **HUD 2D:** mínimo ou trivial apenas quando necessário; não é o foco.
- **Placeholders visuais simples são permitidos** para validar comportamento e legibilidade (primitivos, marcadores, textos/billboards simples), podendo ser refinados depois.
- **Sinalização 3D de dano/estado:** aplicar a jogador e inimigos (flash/tint/marcador/pulso conforme intensidade/estado), aproveitando `Health` e eventos existentes quando possível.
- **Casos cobertos:**
  - Vida baixa/recuperação/morte (progressivo, proporcional à %).
  - Uso de item: sucesso, falha e estados relevantes (ex.: vazio/sem munição, recarregando, toggle on/off, cooldown/bloqueio).
  - Interação: alvo focado destacável, prompt contextual de ação, resultado de pegar/guardar/recuperar/jogar e inventário cheio/vazio.

## Alternativas consideradas
- **HUD 2D completo:** claro, mas desloca atenção para UI e aumenta escopo/asset; pode ser adicionado depois se necessário.
- **Apenas áudio/logs:** insuficiente para informação espacial e acessibilidade visual imediata.

## Consequências
### Positivas
- Informação contextual no espaço da ação; reforça imersão e leitura da cena.
- Facilita validar game feel cedo com placeholders.

### Negativas / Riscos
- Poluição visual se não houver hierarquia (foco > ação > estado persistente).
- Dependência de câmera/posicionamento para legibilidade de billboards.

## Impacto técnico
- **Scripts atuais relacionados:** `Assets/Scripts/Health.cs`, `Assets/Scripts/CameraEffects.cs`, `Assets/Scripts/PlayerHand.cs`, `Assets/Scripts/Backpack.cs`, itens em `Assets/Scripts/Itens/`.
- **Evoluções planejadas/aprovadas:** componentes/roteadores de feedback (vida baixa, cues de interação, apresentação de ações, indicadores 3D de dano/estado) e eventos de uso/munição/toggle; enums `UseResult` e `FeedbackCue` em `Enuns.cs` como vocabulário de feedback.
- **Cena/render:** `Assets/Settings/DefaultVolumeProfile.asset` e `SampleScene.unity` como pontos prováveis de integração.

## Plano de validação
- Checklist em [[notes/Playtest-Log]]: cada ação relevante produz resposta visual distinguível; vida baixa escala e some ao curar; interagível focado é identificável; inimigo e jogador sinalizam dano/estado.
- Validar em Play Mode com Pipeline conectado ([[architecture/Unity-MCP-Connection]]).
