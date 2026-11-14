# ADR-0004: Evolução compatível de contratos e feedback de uso

- **Status:** Aceito
- **Data:** 2026-11-14
- **Relacionado:** [[MOC]], [[architecture/Overview]], [[architecture/Contract-Map]], [[notes/Inventory]], [[notes/Player]], [[notes/Enemy]], [[notes/Feedback-GameFeel]]

## Contexto
Os sistemas atuais se integram por interfaces em `Assets/Scripts/Contracts.cs` (`IUsable`, `IReloadable`, `IPickable`, `IDamageable`, `IHealth`, etc.). `EnemyAttack` e `PlayerHand` chamam `IUsable.Use()` (assinatura `void`). Para suportar feedback rico (sucesso/falha/vazio/cooldown), foco/interação e inventário com eventos, é necessário evoluir contratos sem desestabilizar chamadores existentes.

## Decisão
- **Não quebrar** assinaturas existentes em uso, especialmente `IUsable.Use()`.
- **Evolução preferencial não-destrutiva:** adicionar interfaces/eventos/canais paralelos para resultado e estado (ex.: provedor de resultado de uso, eventos `UseAttempted/Used/Failed`, estado de munição/recarga/toggle, eventos de inventário/interação/foco).
- **Vocabulário de feedback:** usar `UseResult`, `ItemCategory` e `FeedbackCue` (presentes em `Assets/Scripts/Enuns.cs`) como base semântica para UI/pistas 3D, expandindo contratos conforme necessário.
- **Mudanças quebrantes** (renomear membros existentes, alterar assinaturas centrais, mudar semântica consolidada) exigem novo alinhamento/ADR específico antes de aplicar.
- **Alinhamento documental:** toda adição relevante deve refletir em [[architecture/Contract-Map]] e nas notas de domínio.

## Alternativas consideradas
- **Alterar `IUsable.Use()` para retornar `UseResult/bool`:** mais direto, porém quebrante para todos os implementadores/chamadores (ex.: `EnemyAttack`, itens, player).
- **Somente eventos ad-hoc por classe concreta:** evita tocar contratos, mas aumenta acoplamento e dificulta feedback genérico.

## Consequências
### Positivas
- Reduz risco de regressão e permite migração incremental.
- Facilita que feedback e IA consumam sinais de forma uniforme.

### Negativas / Riscos
- Pode introduzir duplicidade temporária (void + resultado/evento) até consolidar.
- Exige disciplina para manter mapa de contratos atualizado.

## Impacto técnico
- **Estado atual:** contratos base inalterados em assinatura; enums de suporte já presentes em `Enuns.cs`.
- **Candidatos de evolução (planejado/aprovado):** `IInteractable`/foco, contratos de inventário e eventos LIFO, `IUseResultProvider`, `IAmmoProvider`, `ITogglable`, `IItemInfo` — a definir/introduzir de forma compatível.
- **Consumidores a preservar:** `Assets/Scripts/PlayerHand.cs`, `Assets/Scripts/Enemy/EnemyAttack.cs`, `Assets/Scripts/Itens/*`, `Assets/Scripts/Backpack.cs`, `Assets/Scripts/Pickable.cs`.

## Plano de validação
- Compilação sem erros após mudanças de contrato; fluxos existentes (pegar, guardar/recuperar, usar, recarregar, arremessar, ataque inimigo) continuam funcionais.
- Feedback de falha/sucesso disponível sem alterar chamadas legadas a `Use()`.
- Atualizar [[architecture/Contract-Map]] e [[notes/Playtest-Log]] a cada incremento.