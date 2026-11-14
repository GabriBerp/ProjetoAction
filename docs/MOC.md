# MOC — ProjetoAction (Second Brain)

> Índice central do conhecimento do projeto Unity **ProjetoAction**.  
> Navegue pelos conceitos via [[wikilinks]] e pelo grafo de dependências abaixo.

## Visão geral

- **Engine**: Unity 6000.6.4f1
- **Cena principal**: `Assets/Scenes/SampleScene.unity`
- **Entrada**: `Assets/InputSystem_Actions.inputactions`
- **Domínio**: jogo em terceira pessoa com movimentação de personagem, inventário/mochila, itens coletáveis e inimigos com patrulha.

## Mapa de notas

### Núcleo do jogador
- [[notes/Player]] — movimento, câmera, olhar, mão, animação e perfil de tamanho.
- [[notes/Inventory]] — mochila, coleta e contratos de itens.

### IA e mundo
- [[notes/Enemy]] — comportamento e patrulha de inimigos.
- [[notes/World-Scenes]] — cenas, prefabs, materiais e assets.

### Arquitetura e integração
- [[architecture/Overview]] — contratos, enums, organização de scripts e fluxo de dados.
- [[architecture/Unity-MCP-Connection]] — conexão do editor Unity via Unity Pipeline / MCP CLI.

## Grafo (MOC)

```mermaid
flowchart TD
    MOC[[MOC — ProjetoAction]]

    subgraph Player[Jogador]
        P1[[notes/Player]]
        P2[[notes/Inventory]]
    end

    subgraph World[Mundo & IA]
        W1[[notes/Enemy]]
        W2[[notes/World-Scenes]]
    end

    subgraph Arch[Arquitetura]
        A1[[architecture/Overview]]
        A2[[architecture/Unity-MCP-Connection]]
    end

    MOC --> P1
    MOC --> P2
    MOC --> W1
    MOC --> W2
    MOC --> A1
    MOC --> A2

    P1 -->|usa contratos/enums| A1
    P2 -->|usa contratos/enums| A1
    W1 -->|usa contratos/enums| A1
    P2 -->|interage com| W2
    W1 -->|atua em| W2
    A2 -->|controla editor/cena| W2
```

## Convenções deste Second Brain

- Notas em `docs/notes/` capturam conceitos estáveis (alto nível), não micro-funções.
- Notas em `docs/architecture/` registram decisões, fluxos e integrações.
- Links usam `[[wikilinks]]` para navegação bidirecional conceitual.
- Diagramas Mermaid representam fluxos principais para localizar dependências rápido.
