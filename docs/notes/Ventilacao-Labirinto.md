# Ventilação — plano de adaptação do labirinto final

> **Tipo:** plano de design/técnica (nada foi implementado ainda). **Data:** 2026-11-27.
> **Cena:** `Assets/Scenes/SampleScene.unity` → raiz `"AREA_VENTILAÇÕES"` → `Labirinto`.
> **Contexto:** área final antes do chefe. O `TransitionDoor` de entrada só deve funcionar depois dos enigmas da **Geladeira** e do **Fogão**.
> Índice: [[MOC]] · Cena: [[notes/World-Scenes]] · Feedback: [[notes/Feedback-GameFeel]].

## 1. Objetivo

Transformar a área existente em um labirinto de alavancas onde:

1. existe **uma** entrada (vinda da cozinha) e **uma** saída para o chefe;
2. cada sala tem **uma alavanca**;
3. alavancas alternam portas de metal, e **duas ou mais alavancas podem controlar a mesma porta**;
4. a travessia exige uma sequência específica — inclusive **ativar uma alavanca, ativar outra e depois desativar a primeira**;
5. os outros túneis com `TransitionDoor` continuam existindo para confundir, mas sem permitir pular o enigma;
6. inimigos ficam atrás das portas fechadas, guardando alavancas.

O blueprint anexo tem a gramática certa (salas pequenas + dutos longos + portas), mas desenha ~8×8 salas. A cena atual tem **4×4**. A recomendação (§4) é **não quadruplicar agora**: densidade de enigma, não quantidade de salas, é o que faz a área parecer grande.

---

## 2. Raio-X do que já existe (medido na cena)

Coordenadas abaixo são **locais ao `Labirinto`** (a raiz da área está deslocada para ≈ `63,5 / 5,1 / 117,9` no mundo).

### 2.1 Volumetria

| Item | Medida real | Observação |
|---|---|---|
| Grade | 4 colunas × 4 linhas = **16 salas** | Passo de **14,75** em X e **15,5** em Z |
| Centros das salas | X = −45,18 / −29,68 / −14,18 / 1,57 · Z = 5,31 / 20,81 / 36,56 / 52,06 | C1..C4 (oeste→leste), R1..R4 (norte→sul) |
| Sala livre | ≈ **5,25 × 5,25** | Dela saem 4 dutos |
| Duto | malha **5 × 10 × 5** | Seção útil 5 de largura; comprimento ≈ 9,5 entre salas |
| Porta (`Door` ×64) | **5 × 5 × 0,63**, `MeshCollider` não-trigger | Uma porta em cada ponta de cada ligação |
| Casca | `Ground` e `Roof` 80×80, topo do chão **−3,99**, base do teto **−1,49** | **Só 2,5 de pé-direito** |
| Paredes laterais | 4 peças `Wall` 15×0,5×80 horizontais em y ≈ 3,26 | **Não são paredes verticais** — a casca está aberta nos lados |
| Luzes | **nenhuma** na raiz da área | A área depende da directional global |

> ⚠️ **Conflito geométrico medido:** as portas têm 5 de altura (de −4,5 a +0,5), mas o vão livre é 2,5. Elas já atravessam o teto. Isso precisa ser resolvido antes de animar qualquer porta (§4.1).

### 2.2 Portas e dutos

- **64 portas** `Door`: 2 por ligação interna (24 ligações) + 1 por abertura de perímetro (16).
- **40 dutos** `Pipe`: 24 ligações internas + 16 stubs de perímetro.
- **Todas as 64 portas estão fechadas e são estáticas.** Hoje o labirinto não é navegável a pé: só se anda pelos `TransitionDoor`.
- Nenhuma alavanca, nenhum inimigo, nenhum script de área na raiz (só `Ground`, `Roof`, 4 `Wall` e `Labirinto`).

### 2.3 Portas de transição (16)

| `doorId` | Posição | Sala / lado | Par |
|---|---|---|---|
| `ventilacao` | (−14, −7,75) | **R1C3 norte — ENTRADA** | porta na cozinha (`Armario/Nicho_A3/Nicho_Lateral_C`) |
| `chefao` | (−45,25, 64,5) | **R4C1 sul — CHEFE** | **sem par** (beco sem saída hoje) |
| `ventilacao1` | (−45,25, −7,75) | R1C1 norte | (1,50, −7,75) R1C4 norte |
| `ventilacao2` | (−29,50, −7,75) | R1C2 norte | (−58, 52) R4C1 oeste |
| `ventilacao3` | (14,25, 36,5) | R3C4 leste | (−58, 36,5) R3C1 oeste |
| `ventilacao4` | (14,25, 21) | R2C4 leste | (14,25, 52) R4C4 leste |
| `ventilacao5` | (−58, 20,75) | R2C1 oeste | (14,25, 5,25) R1C4 leste |
| `ventilacao6` | (−58, 5,25) | R1C1 oeste | (−14, 64,5) R4C3 sul |
| `ventilacao7` | (−29,75, 64,5) | R4C2 sul | (1,75, 64,5) R4C4 sul |

O `chefao` não tem parceiro: ou se cria a arena do chefe, ou o `TransitionDoor` vira um gatilho de fim de nível. Não deixar como está.

---

## 3. Lacunas entre o pedido e a cena

| Pedido | Estado atual | Ação |
|---|---|---|
| Entrada única após 2 enigmas | porta `ventilacao` funciona sempre; cozinha tem 4 aberturas para o labirinto | escolher **R1C3** como entrada canônica e exigir `geladeira` + `fogao` |
| Saída única para o chefe | `chefao` sem par e sem porteiro | criar porta de metal na frente + parear a arena |
| Alavanca em toda sala | 0 alavancas | 16 alavancas (§6) |
| Portas que abrem/fecham | 64 portas estáticas | 19–20 portais animados; o resto vira parede ou passagem fixa |
| 2+ alavancas na mesma porta | — | regra de **paridade** + regra de **combinação exata** (§5) |
| Sequência com “desativar a primeira” | — | solução validada em §5.3 |
| Outros túneis confundem | 14 teleportes cruzam o mapa e **furam** qualquer enigma | portões por **camada/tier** (§7) |
| Inimigos atrás das portas | 0 inimigos na área | 10–12 inimigos + nós de patrulha (§8) |

---

## 4. Decisões de adaptação

### 4.1 Fase 0 obrigatória: sanear a casca

1. **Subir o `Roof` em +3,5** (base do teto ≈ +2,0). A área é uma caixa isolada no mundo, então isso não afeta a cozinha. Sem isso, porta de 5 não cabe, a câmera raspa no teto e a porta não pode subir.
2. Fechar as laterais com 4 paredes verticais de 2 de espessura (as `Wall` atuais são faixas horizontais).
3. Materiais: `Iron` / `DarkIron` para portas e alavancas, `Rust` para detalhes, `InteriorDark` para o fundo dos dutos. Já existem em `Assets/Materials`.
4. Luz: 1–2 luzes frias por quadrante + uma luz vermelha de emergência. A área hoje não tem nenhuma.
5. Rebake do `NavMeshSurface` com as portas **abertas** (ou desabilitadas durante o bake) e usar `NavMeshObstacle` com carve nas portas animadas.

### 4.2 Manter a grade 4×4

O blueprint 8×8 vira 64 salas e ~64 alavancas — o triplo de trabalho de balanceamento sem ganho de diversão. A grade 4×4 já permite **4 anéis de 4 salas**, um por quadrante, que é a unidade de enigma usada aqui. Expansão para 6×6 fica como fase opcional (§10).

**Quadrantes (2×2 cada):**

```
          C1        C2        C3        C4
   R1  [R1C1]──[R1C2]──[R1C3]──[R1C4]     Q2 = noroeste (opcional)
        │  Q2   │       │  Q1   │         Q1 = nordeste (entrada)
   R2  [R2C1]──[R2C2]──[R2C3]──[R2C4]     Q4 = sudeste
        │       │       │       │         Q3 = sudoeste (chefe)
   R3  [R3C1]──[R3C2]──[R3C3]──[R3C4]
        │  Q3   │       │  Q4   │
   R4  [R4C1]──[R4C2]──[R4C3]──[R4C4]
```

### 4.3 Rota principal

```
ENTRADA (R1C3) → Q1: R1C3 → R1C4 → R2C4 → R2C3 → volta R1C3 → R2C3
               → Q4: R3C3 → R3C4 → R4C4 → R4C3 → volta R3C3 → R4C3
               → Q3: R4C2 → R3C2 → R3C1 → R4C1 → volta R4C2 → R4C1
               → porta de metal → CHEFE
```

Cada quadrante é um **anel**: a saída só abre depois de dar a volta no anel e voltar à sala de entrada do quadrante para desligar a primeira alavanca.

Q2 (noroeste) é **opcional**: acessível desde o início pela porta oeste da entrada, com seu próprio anel, 4 alavancas e um cofre com recompensa. Serve de pista falsa e de respiro.

### 4.4 O que fazer com as 64 portas

| Grupo | Qtd. | Tratamento |
|---|---|---|
| Arestas internas dos 4 quadrantes | 16 portais (32 portas) | **animadas**, uma alavanca por sala |
| `Q1-Saída` (R2C3↔R3C3) e `Q4-Saída` (R4C3↔R4C2) | 2 portais (4 portas) | **animados** (elo entre quadrantes) |
| Acesso a Q2 (R1C3↔R1C2) | 1 portal (2 portas) | **fixo aberto** (convite à exploração) |
| Arestas entre quadrantes fora da rota | 5 portais (10 portas) | **fixo fechado** (parede soldada; impede atalho) |
| Perímetro | 16 | são os `TransitionDoor` — não são `Door` |

As 5 ligações soldadas são o que impede o jogador de atravessar de Q1 para Q3 sem passar por Q4. Se um dia a expansão 6×6 acontecer, elas são os pontos naturais de encaixe.

---

## 5. Regra alavanca ↔ porta

### 5.1 Modelo

Cada porta é um **portal** com duas folhas (uma em cada sala). Cada portal tem **uma regra** composta por uma lista de alavancas:

- **`Ímpar` (XOR):** abre quando uma quantidade ímpar das alavancas listadas está ligada. Duas alavancas = “exatamente uma ligada”. É a regra que faz uma alavanca **fechar** a porta que outra abriu.
- **`Exata`:** abre somente quando **todas** as alavancas listadas estão no estado pedido (ligada/desligada). É a regra do portão final: cria a exigência de *desligar* uma alavanca específica.

Toda vez que uma alavanca muda, o controlador reavalia **todos** os portais da área. Portanto o estado é sempre determinístico e reproduzível.

### 5.2 Padrão de um anel (usado em Q1, Q4 e Q3)

Para um anel com salas **E → A → B → C → E** e alavancas `L_E, L_A, L_B, L_C`:

| Portal | Entre | Regra | Efeito |
|---|---|---|---|
| `EA` | E ↔ A | **Ímpar(L_E, L_C)** | abre com `L_E`; fecha quando `L_C` liga |
| `AB` | A ↔ B | **Exata(L_A = ON)** | abre com `L_A` |
| `BC` | B ↔ C | **Exata(L_B = ON)** | abre com `L_B` |
| `CE` | C ↔ E | **Exata(L_C = ON)** | abre o retorno |
| `SAÍDA` | C ↔ próximo | **Exata(L_E=OFF, L_A=ON, L_B=ON, L_C=ON)** | só abre no fim |

Duas portas recebem **duas ou mais** alavancas (`EA` e `SAÍDA`). É exatamente o pedido.

### 5.3 Solução validada (passo a passo)

Estado inicial: tudo desligado, todas as portas fechadas.

| # | Ação | Onde | Resultado |
|---|---|---|---|
| 1 | `L_E` **ON** | E | `EA` abre |
| 2 | E → A; `L_A` **ON** | A | `AB` abre |
| 3 | A → B; `L_B` **ON** | B | `BC` abre; `EA` **fecha** atrás |
| 4 | B → C; `L_C` **ON** | C | `CE` abre; `SAÍDA` continua fechada (L_E ainda ON) |
| 5 | C → E pelo anel; `L_E` **OFF** | E | `SAÍDA` abre; `EA` reabre |
| 6 | E → C (`CE` segue aberta); atravessa `SAÍDA` | — | próximo quadrante |

O passo 5 é o “ativar uma, ativar outra, desativar a primeira”: sem voltar para E e desligar `L_E`, a `SAÍDA` nunca abre.

**Por que não trava:** em qualquer sala há sempre uma porta aberta.
- Em A: `AB` abre com `L_A`; `EA` pode fechar, mas A continua ligada a B.
- Em B: `BC` abre com `L_B`.
- Em C: `CE` abre com `L_C`; se `L_C` for desligada, `BC` reabre.
- Em E: `L_E` está na própria sala; desligar cedo só fecha `EA`, que reabre ao religar.

### 5.4 Segurança extra contra softlock

1. O controlador **nunca fecha** a última porta aberta da sala onde o jogador está; adia o fechamento até ele sair.
2. Porta fechando em cima do jogador: checar `Physics.OverlapBox` no vão e esperar desocupar.
3. Uma **válvula de reset** na sala de entrada (E do Q1) devolve todas as alavancas ao estado inicial e reabre os anéis. Serve também para testes.
4. Se o jogador morrer, as alavancas ficam como estão (não é preciso resetar o enigma).

---

## 6. Alavancas: 16, uma por sala

| Alavanca | Sala | Papel no anel | Portais que altera |
|---|---|---|---|
| `L01` | R1C3 (Q1-E) | primeira | `Q1-EA`, `Q1-SAÍDA` |
| `L02` | R1C4 (Q1-A) | segunda | `Q1-AB`, `Q1-SAÍDA` |
| `L03` | R2C4 (Q1-B) | terceira | `Q1-EA`, `Q1-BC`, `Q1-SAÍDA` |
| `L04` | R2C3 (Q1-C) | quarta | `Q1-CE`, `Q1-SAÍDA` |
| `L05` | R3C3 (Q4-E) | primeira | `Q4-EA`, `Q4-SAÍDA` |
| `L06` | R3C4 (Q4-A) | segunda | `Q4-AB`, `Q4-SAÍDA` |
| `L07` | R4C4 (Q4-B) | terceira | `Q4-EA`, `Q4-BC`, `Q4-SAÍDA` |
| `L08` | R4C3 (Q4-C) | quarta | `Q4-CE`, `Q4-SAÍDA` |
| `L09` | R4C2 (Q3-E) | primeira | `Q3-EA`, `Q3-CHEFE` |
| `L10` | R3C2 (Q3-A) | segunda | `Q3-AB`, `Q3-CHEFE` |
| `L11` | R3C1 (Q3-B) | terceira | `Q3-EA`, `Q3-BC`, `Q3-CHEFE` |
| `L12` | R4C1 (Q3-C) | quarta | `Q3-CE`, `Q3-CHEFE` |
| `L13` | R1C1 (Q2-E) | opcional | `Q2-EA`, `Q2-COFRE` |
| `L14` | R1C2 (Q2-A) | opcional | `Q2-AB`, `Q2-COFRE` |
| `L15` | R2C2 (Q2-B) | opcional | `Q2-EA`, `Q2-BC`, `Q2-COFRE` |
| `L16` | R2C1 (Q2-C) | opcional | `Q2-CE`, `Q2-COFRE` |

Regras por quadrante (substituindo as letras):

| Quadrante | `EA` | `AB` | `BC` | `CE` | Saída |
|---|---|---|---|---|---|
| Q1 | Ímpar(L01, L03) | L02 ON | L03 ON | L04 ON | L01 OFF + L02/L03/L04 ON |
| Q4 | Ímpar(L05, L07) | L06 ON | L07 ON | L08 ON | L05 OFF + L06/L07/L08 ON |
| Q3 | Ímpar(L09, L11) | L10 ON | L11 ON | L12 ON | L09 OFF + L10/L11/L12 ON |
| Q2 | Ímpar(L13, L15) | L14 ON | L15 ON | L16 ON | L13 OFF + L14/L15/L16 ON |

### 6.1 Leitura visual (sem HUD)

- **Alavanca:** base de ferro, haste que gira 60°, luz vermelha (OFF) / verde (ON), placa com o número da sala. Prompt world-space `[E] Acionar válvula` no padrão de `RatSkinStation`/`NpcDialogue`.
- **Fios:** cabo `Cabo` correndo do teto da alavanca até cada portal que ela afeta. É a pista de leitura: o jogador consegue seguir o cabo e prever o efeito sem tutorial.
- **Porta:** lâmpada no batente (vermelha fechada / âmbar em movimento / verde aberta) e um “clunk” pesado. Portas que têm regra de 2+ alavancas recebem **duas** canaletas de cabo no batente.
- **Saída do anel:** placa de pressão/letreiro que acende quando a combinação exata é satisfeita. Não mostrar a combinação — mostrar que “algo destravou” (som + luz).

---

## 7. `TransitionDoor`: confundir sem furar o enigma

Os 14 teleportes atuais cruzam o mapa e hoje levariam o jogador de Q2 direto para a sala do chefe. Regra nova: **cada par só funciona quando a camada correspondente foi concluída** (flag `vent_q1`, `vent_q4`, `vent_q3`). A porta continua sólida e responde com feedback de “sem pressão”.

| Par | Ligação | Liberar quando | Motivo |
|---|---|---|---|
| `ventilacao` | cozinha ↔ R1C3 | **Geladeira + Fogão** | pré-requisito do pedido |
| `chefao` | R4C1 ↔ arena | **Q3 concluído** (porta de metal aberta) | saída final |
| `ventilacao1` | R1C1 ↔ R1C4 | Q1 concluído | impede cair na sala A de Q1 |
| `ventilacao2` | R1C2 ↔ R4C1 | Q3 + Q4 concluídos | é o atalho mais quebrado; se continuar abusável, soldar |
| `ventilacao3` | R3C4 ↔ R3C1 | Q4 concluído | impede pular o miolo de Q4 |
| `ventilacao4` | R2C4 ↔ R4C4 | Q1 concluído | impede pular de Q1 para Q4 |
| `ventilacao5` | R2C1 ↔ R1C4 | Q1 concluído | impede entrar em Q1 pela porta dos fundos |
| `ventilacao6` | R1C1 ↔ R4C3 | Q4 concluído | impede pular para o fim de Q4 |
| `ventilacao7` | R4C2 ↔ R4C4 | Q4 concluído | impede pular de Q3 para Q4 |

**Implementação:** campos opcionais em `TransitionDoor` (`requiredPuzzles: string[]`, `lockedPrompt`). Sem requisito, o componente se comporta exatamente como hoje — evolução compatível ([[architecture/Decisions/ADR-0004-contratos-evolucao-compativel]]).

**Pareamento:** manter os pares atuais na primeira versão. Se o teste mostrar que um par específico ainda fura a sequência, trocar o `doorId` de uma das pontas é uma edição de uma string, sem mover geometria.

---

## 8. Entrada: exigir Geladeira + Fogão

### 8.1 Estado real dos enigmas hoje

- **Geladeira:** existem `CuboGelado` (13 `Pickable`) e **10 `CuboGrabber`** espalhados pela cozinha, mas **nenhum `CuboPuzzleManager` na cena** — o enigma não tem como disparar conclusão. Há também `IcePuzzle`/`IceCrystal` (“PuzzleGeloEngrenagem”) que pode ser o segundo enigma.
- **Fogão:** a geometria tem 4 bocas (`Boquinha`), forno e prateleiras, mas **não há script de enigma do fogão** na cena.

Ou seja: o *gancho* de conclusão precisa ser criado para os dois, mesmo que a lógica interna já exista em outro lugar. O plano não assume que os eventos já existam.

### 8.2 Contrato mínimo

```csharp
// Assets/Scripts/Puzzles/PuzzleProgress.cs
public static class PuzzleProgress
{
    public const string Geladeira = "geladeira";
    public const string Fogao     = "fogao";

    public static event Action<string> Completed;
    public static bool IsComplete(string id);
    public static bool HasAll(params string[] ids);
    public static void Complete(string id);
    public static void Reset(); // RuntimeInitializeOnLoadMethod, como TransitionDoor
}
```

Um componente genérico `PuzzleFlagEmitter` (`flagId`, `UnityEvent`/evento do manager) liga cada enigma ao `PuzzleProgress`:

- no `CuboPuzzleManager.PuzzleCompleted` → `Complete("geladeira")`;
- no `IcePuzzle.onPuzzleCompleted` → `Complete(...)` conforme o enigma escolhido;
- no enigma do fogão (a criar) → `Complete("fogao")`.

Instanciar **um** `CuboPuzzleManager` na cena e arrastar os 10 grabbers. Sem isso a geladeira nunca conclui.

### 8.3 Porta de entrada

Na ponta da cozinha do par `ventilacao` (dentro de `Armario/Nicho_A3/Nicho_Lateral_C`):

```
requiredPuzzles: [geladeira, fogao]
lockedPrompt: "Trancada — as válvulas da Geladeira e do Fogão ainda estão frias"
```

- Enquanto faltar um: a porta não teleporta, toca um “clunk” seco e mostra no mundo **qual** enigma falta (`[E] ...` ou placa).
- Com os dois: destrava com luz verde + chiado de pressão, e o `TransitionDoor` funciona normalmente.
- A ponta do labirinto não precisa de requisito — quem já entrou, entrou.

---

## 9. Inimigos

Hoje há **0 inimigos** na área. Distribuição proposta (guarda de alavanca, atrás de porta fechada):

| Sala | Inimigos | Papel |
|---|---|---|
| Q1-B (R2C4) | 2 | guarda `L03` |
| Q1-C (R2C3) | 1 + 1 voador | guarda `L04` |
| Q4-B (R4C4) | 2 | guarda `L07` |
| Q4-C (R4C3) | 2 | guarda `L08` |
| Q3-B (R3C1) | 2 | guarda `L11` |
| Q3-C (R4C1) | 2 + mini-chefe | antes da porta do chefe |
| Q2 (opcional) | 3 | pista falsa com recompensa |
| **Total** | **13–15** | |

- Usar `Assets/Prefabs/Enemy.prefab` + `PatrolNode` (já existe, 431 instâncias na cena).
- Cada sala recebe 2–3 nós de patrulha; o inimigo só “acorda” quando o portal daquela sala abre (`VentRoom.Entered`), economizando CPU e evitando que ele atravesse portas antes da hora.
- Com as portas como `NavMeshObstacle`, o inimigo não passa por porta fechada. Rebake com portas abertas.
- Não é preciso travar a porta do chefe na morte de todos os inimigos; a exigência do pedido é só a sequência de alavancas. Se quiser um “quarto limpo”, o `VentLabirinto` já tem o registro das salas para checar.

---

## 10. Implementação técnica

### 10.1 Scripts novos (pasta `Assets/Scripts/Ventilacao/`)

| Arquivo | Responsabilidade |
|---|---|
| `VentLever.cs` | foco por câmera + tecla E (padrão `NpcDialogue`), anima haste, troca lâmpada, chama `VentLabirinto.Toggle(this)` |
| `VentDoor.cs` | uma folha de porta: `SetOpen(bool, instant)`, anima subindo 5,2, desliga/liga `Collider`, atualiza `NavMeshObstacle`, checa ocupação antes de fechar |
| `VentPortal.cs` | agrupa as 2 folhas + a regra (`Ímpar`/`Exata`), avalia e aplica |
| `VentLabirinto.cs` | dono das alavancas/portais, reavalia tudo a cada toggle, flags `vent_q1/q4/q3`, reset, eventos |
| `VentRoom.cs` | volume/trigger da sala, sabe quem está dentro, usado pela segurança de softlock e pelo “acordar” dos inimigos |
| `PuzzleProgress.cs` | flags globais (na raiz de `Puzzles/`) |
| `PuzzleFlagEmitter.cs` | adaptador de cada enigma para `PuzzleProgress` |

`TransitionDoor.cs` ganha apenas os campos opcionais de requisito, sem mudar o fluxo atual.

### 10.2 Estrutura de dados do enigma

```csharp
[Serializable] public class LeverDoorLink
{
    public string leverId;
    public string portalId;
    public bool expectedOn;      // usado no modo Exato
}

[Serializable] public class PortalRule
{
    public string portalId;
    public Modo modo;            // Impar | Exato
    public LeverDoorLink[] links;
}
```

No Inspector, a tabela do §6 é a fonte da verdade. Um botão `Validar` no editor (ou um teste de editor) percorre todos os estados possíveis (4 alavancas = 16 estados) e confirma que:

1. a saída só abre no estado final;
2. nenhuma sala fica sem porta aberta em estado alcançável;
3. a rota E→A→B→C→E→C é fisicamente possível com as portas abertas.

### 10.3 Cena — passos mecânicos

1. Renomear as 64 `Door` para `D_<sala>_<lado>` (ex.: `D_R1C3_E`) — hoje são todas “Door”.
2. Para cada portal do §4.4: criar um `VentPortal` num objeto vazio no centro do vão, arrastar as duas folhas.
3. Soldar as 5 ligações entre quadrantes fora da rota (deixar porta + collider; sem script).
4. Abrir permanentemente o acesso a Q2 (remover collider / desabilitar as duas folhas).
5. Instalar as 16 alavancas na parede de cada sala, a ~0,9 do piso, longe dos vãos.
6. Passar os cabos (`Cabo`) da alavanca até os batentes das portas afetadas.
7. Colocar a porta de metal do chefe (`Q3-CHEFE`) no corredor sul de R4C1, antes do `TransitionDoor chefao`.
8. Configurar os requisitos dos 16 `TransitionDoor`.
9. Inimigos + nós de patrulha + bake.

### 10.4 Performance

- 19–20 portais animados (≈40 folhas) é barato; animar só quando o estado muda, nunca em `Update` contínuo.
- `NavMeshObstacle` com carve apenas nos portais (não nas 64 portas): menos carving.
- Inimigos desativados até a sala abrir.

---

## 11. Fases e critérios de aceite

| Fase | Entrega | Aceite |
|---|---|---|
| **0** | teto, paredes, luz, materiais, rename das portas, bake | andar de ponta a ponta sem clipping de câmera; portas não atravessam o teto |
| **1** | `PuzzleProgress`, gate no `TransitionDoor`, `PuzzleFlagEmitter`, managers ligados | porta da cozinha recusa e informa; com as 2 flags, entra |
| **2** | `VentLever`, `VentDoor`, `VentPortal`, `VentLabirinto` + validador | teste do §5.3 no Q1, sem softlock em nenhum estado |
| **3** | Q4 e Q3 ligados; porta de metal do chefe | sequência completa entrada→chefe em ≤ 6 min |
| **4** | Q2 opcional + tier gates dos 14 teleportes | nenhum teleporte permite chegar ao chefe sem Q1/Q4/Q3 |
| **5** | inimigos, cabos, feedback, playtest | 13–15 inimigos, nenhum atravessa porta fechada; registro em [[notes/Playtest-Log]] |
| **6 (opcional)** | expansão 6×6 ou arena do chefe | decidir só depois do playtest da fase 5 |

---

## 12. Riscos e decisões abertas

1. **Repetição:** os três anéis usam o mesmo padrão. Mitigar com dressagem diferente (Q4 com corredor alagado, Q3 com calor/fumaça), inimigos distintos e uma alavanca “quebrada” que exige duas interações no Q4.
2. **Teleporte ainda fura:** o par `ventilacao2` (R1C2 ↔ sala do chefe) é o mais perigoso. Se o tier gate não bastar, soldar.
3. **Qual enigma é qual:** a cena não nomeia “Geladeira” e “Fogão” nos scripts. Confirmar com o design antes de ligar os `PuzzleFlagEmitter`.
4. **Enigma do fogão inexistente:** precisa ser construído (ou stub temporário) para a entrada funcionar. É a maior dependência externa deste plano.
5. **Chefe sem arena:** o `TransitionDoor chefao` não tem par. Sem arena, ele deve virar gatilho de fim de área, não teleporte.
6. **Blueprint 8×8:** a decisão de expandir deve vir do playtest, não do desenho. A 4×4 com 3 anéis já entrega a sequência completa.

---

## 13. Resumo executivo

- **Não reconstruir:** a grade, as salas e os dutos ficam.
- **Subir o teto** e fechar as laterais (a casca atual tem 2,5 de altura e nenhuma parede vertical).
- **16 alavancas**, uma por sala, com 19–20 portais animados; o resto vira parede ou passagem fixa.
- **Três anéis de enigma** (Q1 → Q4 → Q3) com a sequência `L_E ON → L_A ON → L_B ON → L_C ON → L_E OFF`.
- **Q2 opcional** como pista falsa com cofre.
- **Entrada** só com `geladeira` + `fogao`; **saída** com a porta de metal do chefe.
- **14 teleportes** viram atalhos liberados por camada, nunca atalhos para o chefe.
- **Inimigos** guardam as alavancas atrás das portas.
