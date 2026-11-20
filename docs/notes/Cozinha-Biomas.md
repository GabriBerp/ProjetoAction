# Cozinha — Plano de Design: Interiores, Pontes e Biomas

> **Tipo:** plano/design (nada foi construído ainda). **Cena:** `Assets/Scenes/SampleScene.unity` → raiz `COZINHA`.
> **Contexto:** ação/aventura em terceira pessoa com rato. Índice: [[MOC]] · Cena: [[notes/World-Scenes]].
> **Plano focado em armários e balcões (medido em 2026-11-20):** [[notes/Cozinha-Armarios-Balcoes]] — receitas ProBuilder, distribuição e ocupação rata só nesses móveis.

## 1. Como usar este documento

Cada sistema abaixo tem: **(a)** o que já existe na cena, **(b)** a proposta com medidas reais, **(c)** onde aplicar (coordenadas), **(d)** custo e critério de aceite. O roadmap do fim ordena as fases por ganho visual x esforço.

---

## 2. Raio-X do que existe hoje

Cena medida objeto por objeto (transform + bounds de malha em espaço de mundo).

> ⚠️ **Tabela superada em 2026-11-19.** A cozinha foi reconstruída em **ProBuilder** e várias medidas mudaram (geladeira 28 de altura, mesa em z = 10, paredes passaram a 35 de altura, vassoura flutuando). Use **§15** como fonte atual; esta tabela fica só como histórico.

### 2.1 Volumetria

| Elemento | Medida real | Observações |
|---|---|---|
| Sala | 88 × 88 (paredes internas em x/z = ±44, 2 de espessura) | **Parede sul (z = +44) não existe** — a cozinha está aberta de um lado |
| Pé-direito | chão y = 0, teto em y = 32,76 (laje `Roof` 90 × 90 × 1) | 32,7 de altura livre |
| Balcão | corpo 10 (altura) × 10,66 (largura) × 10 (prof.) — **tampo em y = 12,52**, base em y = 2,52 | 18 blocos clonados ao longo de um "U" (oeste, norte, leste) |
| Vão bancada → armário | **9,48** de altura livre (12,52 → 22,00) | A "avenida" vertical da cozinha |
| Armário superior | 10 × 10 × 10…70 (comprimento) — y de 22 a 32 | 5 blocos, todos encostados no teto (0,76 de folga) |
| Geladeira | 20 × 32 × 15,5 em (-37,16 / 15 / 5,76) | 2 prateleiras internas (y = 6 e 13), 17 blocos de gelo, **2 portas soltas** encostadas ao lado |
| Fogão | 12 × 14,25 × 16 em (15 / 6,13 / -36,48) | 4 bocas no topo (y = 13,13), 2 prateleiras internas (y = 4 e 8), forno (10 × 10,3 × 1) encostado à frente |
| Pias (2) | 10,66 × 10 × 10 em (37,56 / -5) e (37,56 / -15) | Água = bloco 9,9 × 9 × 10 (y 2,83 → 11,83): **piscina rasa nivelada com o tampo** |
| Micro-ondas | 12,6 × 5 × 12 em (32,8 / 15 / 32) | **Porta aberta + prato dentro**: hoje é o único interior legível da cena |
| Mesa + cadeiras | mesa 20 × 12,5 × 40 (tampo 11,5) no centro | 4 cadeiras (18 de altura) |
| Vassoura | 28,1 de altura em (29,78 / 23,91) | Encostada; base afunda 6,3 abaixo do chão |
| Portas de transição | 2,75 × 1 × 3,75 em (15 / 2 / -42,6) e (-42,68 / 2 / 5) | Duas saídas **no tamanho do rato**: atrás do fogão e atrás da geladeira |

### 2.2 Sistemas e materiais disponíveis

- **Luz:** 1 Directional (intensidade 2, 5000 K). **Nenhuma luz pontual/spot.**
- **Pós-processamento:** 1 `Global Volume` com Bloom, Vignette, Tonemapping, MotionBlur (`Assets/Settings/SampleSceneProfile.asset`). **Sem fog, sem exposição local.**
- **Oclusão:** SSAO está no `Assets/Settings/PC_Renderer.asset`; **não há Decal Renderer Feature**.
- **Materiais:** concreto preto/branco, Quartz/DarkQuartzo, Wood, WallWood, Roof, Floor, Iron, IronBars, Ice/BlueIce/PackedIce, Water, BlackGlass, SnowParticle. **Zero material orgânico** (musgo, terra, folha, ferrugem, fuligem, tecido, papelão).
- **Scripts reaproveitáveis:** `Pickable`/`Backpack` (itens), `WaterVolume` (**existe, mas não está usado na cena**), `Glider`, `Health`, `RatSizeProfile`.
- **Vazios:** nenhum `NavMesh` assado (`m_NavMeshData: 0`), nenhum light probe/reflection probe, nenhum `StaticEditorFlags` marcado (nada de batching), nenhum script de porta/gaveta.

### 2.3 Escala de trabalho (inferida — confirmar no editor)

Cápsula do `Player`: raio 0,16 / altura 0,80 (eixo Z), escala 1. Ou seja: **rato ≈ 0,8 un de comprimento e ~0,32 un de altura de corpo**. Isso dá **1 un ≈ 8 cm**:

| Referência | Unidades | Leitura "de rato" |
|---|---|---|
| Rato (comprimento) | 0,8 | 1 rato |
| Fresta sob o balcão | 2,52 | ~3 ratos empilhados — **já é um túnel** |
| Tampo do balcão | 12,52 | ~15 ratos da altura |
| Vão bancada→armário | 9,48 | ~12 ratos |
| Folga sobre os armários | 0,76 | cabe o rato, **não cabe a câmera** |
| Teto | 32,76 | ~40 ratos |

> Regra derivada: **1 un ≈ 1 "passo de rato"**. Toda peça nova deve ser pensada em múltiplos de 0,5–1 un.

### 2.4 Achados soltos (corrigir ao passar por essas áreas)

- 4 cadeiras: uma flutua 1,5 acima do chão em (-21,63 / 15) e as outras afundam até -1 → base inconsistente.
- 11 `Item` flutuam ~2,9 acima do tampo da mesa (y = 14,41 vs tampo 11,5).
- Vassoura: cabeça 6,3 abaixo do piso.
- `Water` das pias é só malha: **não tem `WaterVolume`** → o rato atravessa a água em vez de nadar.
- Balcões e armários são **malhas únicas e fechadas** (0 filhos): hoje é impossível abrir nada — isso define o trabalho do Sistema 1.

---

## 3. Diagnóstico de design

**Pontos fortes (não mexer):**
1. Escala já é "cozinha-colosso": 88 × 88 com bancadas de 15 ratos de altura. A fantasia de rato está resolvida.
2. Layout em "U" contínuo = circuito natural (corrida, fuga, patrulha) e leitura clara de onde ficam as coisas.
3. Os biomas já estão **latentes nos objetos**: gelo na geladeira, água na pia, fogo nas bocas, sombra sob os balcões. Não é preciso inventar geografia — é preciso **intensificar**.
4. As 2 portas de transição no tamanho do rato dão "bocas" prontas para o mundo.
5. O micro-ondas prova que interior + objeto interno já funciona visualmente.

**Lacunas (o que impede a leitura de "civilização vivendo aqui há muito tempo"):**
1. **Tudo é bloco fechado.** Sem aberturas, o olho não encontra "dentro" — e sem dentro não há história.
2. **Sem escala média.** Entre o chão e o tampo (12,5) não existe nada: falta a camada onde a vila acontece.
3. **Uma única luz.** Sem luz local, biomas não se diferenciam e os interiores ficam ilegíveis (preto chapado).
4. **Zero vegetação/sujeira.** Não há material orgânico nem decals: a "natureza retomando" não tem vocabulário.
5. **Sem tralha.** Nenhum objeto de rato (palito, barbante, rolha): falta a assinatura "construído por bichos".
6. **Sem verticalidade jogável.** Só a mesa e a vassoura permitem subir; a bancada é um penhasco de 12,5.
7. **Nada estático/batchado** e não há NavMesh: qualquer adição de props vai custar e pode travar inimigos.

**Tese do plano:** a cozinha já tem o "palco"; falta **camada de ocupação**. Três camadas narrativas, sempre na mesma ordem visual:

1. **Arquitetura** (cozinha humana, grande, dura, geométrica) — já existe.
2. **Ocupação rata** (aberturas roídas, pontes, tralha, portas entreabertas) — o grosso deste plano.
3. **Natureza retomando** (musgo, trepadeira, poeira, teias) — a cola que data tudo.

---

## 4. Princípios (válidos para todos os sistemas)

1. **Silhueta > textura.** Peças de tralha devem ser reconhecíveis em preto puro (silhueta de pregador, de clipe, de caixa).
2. **Regra do "1 terço".** Nunca abrir tudo: ~⅓ dos módulos abertos, ⅓ com marca de uso (roído/manchado), ⅓ intacto. Contraste é o que faz o abandono parecer acidental, não decorado.
3. **Tudo que é detalhe precisa de 3 níveis de leitura:** a 10 un (silhueta geral), a 2 un (rota/jogada) e a 0,5 un (texto visual: marca de dente, barbante, etiqueta).
4. **Módulo antes de peça única.** 4–6 prefabs bem feitos, repetidos e rotacionados, valem mais que 40 peças únicas.
5. **Cada bioma tem 5 assinaturas:** cor de luz, material dominante, vegetação, tralha típica e som. Sem as 5, não lê como bioma.
6. **Nada de detalhe sem colisão pensada.** O rato precisa poder entrar, subir e passar; ou o objeto é puro cenário fora de rota.

---

## 5. Sistema 1 — Armários e balcões com portas/gavetas entreabertas e interior

### 5.1 O problema técnico

`Balcao` e `Armario` são **uma malha fechada cada** (0 filhos). Não há como "abrir" sem intervir na geometria. Três caminhos:

| Caminho | Como | Prós | Contras | Recomendação |
|---|---|---|---|---|
| **A. Cirurgia no Blender** | Abrir vão no módulo, criar variantes `Balcao_Aberto`, `Armario_Aberto` com nicho interno e batente | Correto, leve, encaixa no pipeline (o projeto já usa `Assets/Models/BlenderFiles`) | Retrabalho nos 18 blocos se o UV/material for único | **Base do padrão final** |
| **B. Fachada aplicada (só Unity)** | Manter o bloco fechado e colar por fora: moldura fina + porta + "nicho" escuro recuado 0,6 | Rápido, zero Blender, funciona hoje | Precisa de cuidado para não parecer adesivo em ângulo rasante; não permite entrar de verdade | **Protótipo imediato** |
| **C. Híbrido (recomendado)** | B para validar 3 pontos na cena atual; A para produzir os 4 módulos definitivos | Aprende com o protótipo e não joga trabalho fora | Duas etapas | **Adotar** |

### 5.2 Especificação dos módulos (em unidades)

- **Batente/moldura:** 0,6 de espessura, 1,2 de largura de perfil; sobressai 0,4 do plano da fachada.
- **Porta de armário:** 1,0 de espessura, folga 0,15 em cada lado; **dobradiça no canto superior ou lateral**, não no meio.
- **Abertura "entreaberta":** 18°–32° (menos que isso lê fechado; mais que 45° vira convite óbvio). Uma porta por armário, no máximo duas.
- **Gaveta:** frente 1,0 de espessura, **puxada 25–40%** do curso (≈ 3–5 un para uma gaveta de 12). Fundo em madeira 0,3. Corrediça visível dos dois lados (2 trilhos de 0,2).
- **Interior:** nicho de **7,4 de profundidade** (bloco tem 10, parede 1,25 + fundo). Altura útil de armário: 8,4 dividida em 1 ou 2 prateleiras de 0,3. Balcão: 1 gaveta alta ou 2 meias.
- **Prateleira interna:** 0,3 de espessura, recuo 0,5 da boca (para a sombra criar profundidade).
- **Luz interna:** 1 ponto fraco (0,4 de intensidade, 3 de alcance) dentro de ⅓ dos módulos abertos — o resto fica em silhueta. Isso é o que faz o "interior" existir de longe.

### 5.3 Catálogo de fachadas (6 prefabs cobrem tudo)

1. `Fachada_PortaEntreaberta` — armário, porta 25°, interior com 1 prateleira e potes.
2. `Fachada_PortaArrancada` — porta pendurada por uma dobradiça (abandono forte), interior vazio com teia.
3. `Fachada_GavetaPuxada` — gaveta 35% fora, com tralha (garfos, tampinhas, barbante).
4. `Fachada_GavetaCaída` — gaveta fora do trilho, encostada no chão embaixo do balcão (vira **rampa** de 12,5? não: vira degrau/plataforma).
5. `Fachada_BuracoRoído` — canto do rodapé roído (0,8 de diâmetro irregular), sem porta: é a **entrada do túnel inferior**.
6. `Fachada_ModuloAbertoVazio` — módulo sem porta, com marcas de fita adesiva e um "nicho-ninho".

### 5.4 Onde aplicar (distribuição no "U")

| Zona | Módulo | Fachada | Papel |
|---|---|---|---|
| Norte, x = -25 | Balcão | Gaveta puxada | **Degrau** para a bancada (com gaveta a 5 de altura) |
| Norte, x = -15 | Balcão | Porta arrancada | Interior com tralha; acesso ao túnel |
| Norte, x = 5 | Balcão | Buraco roído | Entrada do bioma "Úmido" |
| Norte, x = 25 | Balcão | Gaveta caída + encostada | Plataforma baixa e "lixo" |
| Leste, z = 15 | Balcão | Gaveta puxada 40% | Feira/mercado (ver 8.4) |
| Leste, z = -25 (perto da pia) | Balcão | Porta arrancada | Potes, cheiro de umidade |
| Oeste, z = -15 e z = 25 | Balcão | Buraco roído + musgo | Duas bocas do túnel |
| Armário x = -17 (50 de comprimento) | Armário | 2 portas entreabertas + 1 aberta | **Vila principal no alto** |
| Armário x = 32 | Armário | Porta entreaberta | Depósito de tralha |
| Armário x = -37,25 (z = 24 e z = -17) | Armário | 1 aberta cada | Atalhos de ponte |
| Armário x = 37 (70 de comprimento) | Armário | 2 entreabertas | "Prédios" da vila leste |

Meta de cobertura: **8 aberturas de balcão + 6 de armário**, 4 buracos roídos, 6 gavetas.

### 5.5 Interior precisa de conteúdo (senão é buraco preto)

Kit mínimo de 10 props (todos entre 0,6 e 2,5 un, ou seja, "mobília de rato"):

caixa de fósforo · rolha · carretel de linha · tampinha de garrafa · pregador de roupa · clipe · palito de dente · botão · pedaço de barbante enrolado · latinha amassada.

Dentro dos módulos humanos (armário/geladeira/fogão), o conteúdo é **humano**: potes de vidro, latas, sacos de papel, cereais, formigueiro. Dentro dos nichos de rato, o conteúdo é **tralha**. Essa troca de vocabulário é o que conta a história.

### 5.6 Colisão, câmera e custo

- Nicho precisa de **colisor interno** (paredes + fundo) para o rato não atravessar; a porta entreaberta é colisor estático (sem Rigidbody — física de dobradiça só em 2–3 peças "de vitrine").
- **Câmera:** a câmera em 3ª pessoa não entra em nicho de 7,4 de profundidade → criar `CameraVolume` nos 4 nichos onde o jogador entra, puxando o offset para perto do ombro (o projeto já tem `CameraEffects`/Cinemachine).
- **Custo:** módulo aberto ≈ 200–500 tris + 1 material compartilhado. 14 aberturas + 10 props ≈ 8k tris. Irrelevante para o orçamento atual (cena tem ~90 objetos).

**Critério de aceite:** parado a 3 un de um balcão, dá para dizer o que tem dentro dele; e o rato consegue entrar em pelo menos 3 nichos.

---

## 6. Sistema 2 — Pontes e vias verticais ("as pontes do tamanho do ratinho")

### 6.1 Onde exatamente as pontes vivem

O vão livre entre tampo (12,52) e armário (22) tem **9,48 de altura** e é contínuo ao longo dos três lados — é a "avenida suspensa" perfeita: visível do chão como silhueta, inalcançável sem rota, e conecta os armários uns aos outros.

Restrição medida: **sobre os armários há só 0,76 de folga** até o teto. Cabe o rato (0,32), **não cabe a câmera**. Duas saídas:
- **(a)** Aceitar o "sótão" com câmera em modo apertado (bom para tensão, ruim para navegação longa);
- **(b) Recomendada:** rebaixar o forro em faixa ao longo do "U" (recesso/claraboia de 2,5 de profundidade) → 3,26 de folga, câmera confortável e **feixes de luz** entrando pelas frestas. Isso resolve verticalidade **e** atmosfera de abandono de uma vez.

### 6.2 Tipologias de ponte (6 prefabs + 2 de ligação)

| # | Peça | Vão | Largura | Ancoragem | Leitura |
|---|---|---|---|---|---|
| 1 | Ponte de palitos (2 palitos + travessas) | 4–6 | 0,9 | Apoiada nos batentes | Rústica, "fase 1" da vila |
| 2 | Ponte de régua | 6–9 | 1,0 | Corda amarrada nos cantos | Construída, reta, com corrimão de barbante |
| 3 | Ponte de pregadores | 3–5 | 0,8 | Presa por mola no batente | Cômica e reconhecível |
| 4 | Ponte de barbante (pênsil) | 8–12 | 0,5 (corda dupla + tábua) | Nós nos dois lados | Balança; a mais "viva" |
| 5 | Ponte de escova/cabo | 5–7 | 1,1 | Encaixada no vão | Larga, vira "praça" |
| 6 | Ponte quebrada | 4 | — | Uma ponta solta, pendurada | **Abandono**: alguém caiu ou desistiu |
| J1 | Junção 2×2 | — | 2×2 | — | Nó de encontro de 3 pontes |
| J2 | Rampa de espátula/colher de pau | 9–12 de desnível | 0,8 | Apoio no tampo + no armário | Liga bancada ↔ ponte |

### 6.3 Rotas propostas (coordenadas)

1. **Rota Norte (principal):** armário (-17,25 / 27 / -37) → ponte pênsil → armário (32 / 27 / -37). Passa em frente ao vão do fogão: a ponte "sente" o calor (barbante chamuscado perto do fogão).
2. **Rota Leste:** armário (37 / 27 / 3) → 3 pontes curtas → parede leste; desce por corda até o tampo perto da pia.
3. **Rota Oeste (gelada):** geladeira (topo 31) → prancha (uma das portas soltas da geladeira vira ponte!) → armário (-37,25 / 24). **Aproveitamento:** as 2 portas soltas já existentes são material narrativo pronto.
4. **Atalho do fogão:** rampa de espátula do tampo (12,52) até a prateleira interna do fogão (y = 8) → dentro do fogão.
5. **Subida do chão:** 3 caminhos, sem teleporte — (i) gaveta puxada como degrau, (ii) corda pendurada do armário até 6 de altura + caixa de fósforo como apoio, (iii) vassoura encostada (28) como escada natural, com barbante enrolado.
6. **Rota dos fundos:** o túnel sob os balcões (0 → 2,52) conecta os 4 buracos roídos. Iluminação mínima, mas é o caminho seguro (inimigos grandes não passam).

### 6.4 Regras de jogo

- Ponte **sempre** com 1 vão "quebrado" (peça 6) em cada rota: dá ao jogador leitura de perigo e ao cenário a marca de tempo.
- Nada de ponte onde o inimigo patrulha em NavMesh (ver 11.3): pontes são rotas do jogador, não do inimigo — isso dá valor tático à verticalidade.
- `Glider` existente deve ser a recompensa por subir: das pontes o rato planeia até a mesa/pias.

**Critério de aceite:** percorrer do chão até o topo de um armário e atravessar para outro **sem pular no vazio** e sem clipping de câmera.

---

## 7. Sistema 3 — Os Biomas

Ficha única por bioma, com as 5 assinaturas (luz, material, vegetação, tralha, som) + gameplay.

### 7.1 Tropical — as pias (leste, z = -5 e z = -15)

- **Estado atual:** piscina 9,9 × 9 × 10 nivelada ao tampo, sem `WaterVolume`.
- **Luz:** ciano-esverdeado (#7FD3C0), 2 pontos fracos + 1 spot rasante criando cáustica falsa (textura de água animada em plano no fundo).
- **Material:** reusar `Water`; adicionar `MossWet` (verde-escuro brilhante) nas bordas e `TileCracked`.
- **Vegetação:** samambaias penduradas na beirada, aguapé na água, musgo no ralo, trepadeira subindo a torneira.
- **Tralha:** doca de palitos, barquinho de casca de noz, vara de pescar com linha, redes de barbante.
- **Som:** gotejamento irregular, coaxar distante, água corrente baixa.
- **Gameplay:** ativar `WaterVolume` (1 linha de componente no trigger) para o rato nadar/mergulhar; `Submersion` já existe no `CharacterMotor`. Molhado → apaga brasa (ver 7.2).
- **Detalhe-âncora:** torneira pingando com anel de calcário no tampo (prova de anos).

### 7.2 Infernal — o fogão (norte, x = 15)

- **Estado atual:** 4 bocas (2,5–4 de diâmetro) em y = 13,13; prateleiras internas em y = 4 e 8; forno de 10 × 10,3 solto à frente.
- **Luz:** laranja (#FF7A2A) pulsante, 2 pontos dentro das bocas + 1 spot do vão do exaustor. Precisa de **flicker** (script de 12 linhas, ruído em intensidade/posição).
- **Material:** `Charcoal` (preto fosco), `Ash` (cinza claro), `Ember` (emissivo com ruído), metal com `Rust`.
- **Vegetação:** **nenhuma** — a regra é contraste: um raio de 4 un ao redor do fogão é estéril, com fuligem. Vegetação só volta a 6+ un.
- **Tralha:** panelas abandonadas, concha derretida, panela de rato (tampinha) na boca apagada, "fogueira" de rato no ladrilho.
- **Som:** estalo de brasa, zumbido de gás, panela vibrando.
- **Gameplay:** calor = dano leve em contato (usar `Health`/trigger); brasas acesas em 2 das 4 bocas; o forno aberto (10 × 10,3 de boca) é a "caverna" do bioma — interior com 3 níveis.
- **Detalhe-âncora:** a parede atrás do fogão, de x 9 a 21, escurecida em gradiente até y = 22 (fuligem subindo há décadas).

### 7.3 Congelado — a geladeira (oeste, z = 5)

- **Estado atual:** 32 de altura, 17 blocos de gelo, 2 prateleiras, 2 portas soltas, material `Quartz`.
- **Luz:** azul frio (#8FC8FF), luz interna de geladeira (uma lâmpada piscando — "ainda ligada depois de todo esse tempo" é um detalhe excelente de abandono), névoa local.
- **Material:** `Ice`, `BlueIce`, `PackedIce` já existem + `Frost` (branco leitoso) para o vidro.
- **Vegetação:** líquen escuro nas quinas, nada verde.
- **Tralha:** iglus de rato nas prateleiras, ponte de gelo entre y = 6 e y = 13, pingentes, pegadas na geada, "carne" fossilizada num pote.
- **Som:** zumbido de compressor, estalos de gelo, vento baixo.
- **Gameplay:** piso escorregadio (reduzir fricção do `CharacterMotor` dentro do volume), frio causa lentidão; o rato pode atravessar as 2 portas soltas como anteparos.
- **Detalhe-âncora:** geada avançando **2 un além** da geladeira no chão e na parede, com "língua" irregular — o bioma precisa vazar.

### 7.4 Úmido / Subsolo — embaixo dos balcões

- **Estado atual:** fresta de **2,52** sob todos os balcões (o "rodapé" já é um túnel), escura, sem nada.
- **Luz:** praticamente nenhuma — 1 cogumelo bioluminescente a cada 8–10 un (emissivo fraco), poças refletindo.
- **Material:** `MossWet`, `Puddle` (preto espelhado), `Rust` nos pés metálicos.
- **Vegetação:** musgo em 60% do rodapé, cogumelos, fetos pequenos, raízes saindo das juntas.
- **Tralha:** fios, barbantes, ninho de papel, "casas" de caixa de fósforo, túneis roídos.
- **Som:** eco de gota, roer distante, vento em fresta.
- **Gameplay:** rota segura/stealth; 4 buracos roídos como entradas; aqui a câmera precisa baixar (2,5 de teto).
- **Detalhe-âncora:** marcas de dente em semicírculo no rodapé, com serragem no chão embaixo (2 un de raio).

### 7.5 Neutro — centro (mesa, cadeiras, vassoura)

- Poeira, teias nas quinas das cadeiras, marcas de arrasto no chão, itens flutuantes corrigidos.
- Serve de "respiro" entre biomas: sem cor forte, luz branca difusa.
- A mesa (tampo 11,5) é o **ponto de orientação** central: dali se veem os 3 biomas de parede.

### 7.6 Transições (a regra que faz o conjunto parecer um lugar)

- Gradiente de **3–4 un** entre biomas: musgo morre a 4 un do fogão; geada perde força a 4 un da pia; água escorre da pia para o túnel.
- Nenhum bioma começa com linha reta — sempre irregular, seguindo juntas, quinas e ralos.
- Cada bioma tem **1 objeto-âncora** que dá a leitura de longe: anel de calcário (tropical), parede de fuligem (infernal), língua de geada (congelado), cogumelo brilhante (úmido).

Mapa mental:

```mermaid
flowchart LR
  subgraph Oeste
    GEL[Geladeira<br/>congelado]
    ARM1[Armário -37/24]
  end
  subgraph Norte
    FOGAO[Fogão<br/>infernal]
    ARM2[Armário -17 / 32]
  end
  subgraph Leste
    PIA[Pias<br/>tropical]
    ARM3[Armário 37/3]
  end
  subgraph Piso
    TUNEL[Túnel 0–2,5<br/>úmido]
  end
  ARM2 -- ponte pênsil --> ARM3
  ARM1 -- prancha/porta solta --> ARM2
  ARM3 -- corda --> PIA
  TUNEL --> FOGAO
  TUNEL --> GEL
  TUNEL --> PIA
```

---

## 8. Sistema 4 — Mini construções de tralha (a "civilização")

### 8.1 Regras de escala

| Peça | Medida | Comparação |
|---|---|---|
| Casinha | 2,0 (largura) × 1,2 (altura) × 2,5 (fundo) | 2,5 ratos de comprimento |
| Porta de casinha | 0,9 × 1,0 | Passagem confortável |
| Telhado | inclinação 35–40° | **assinatura de silhueta** |
| Fogueira | 0,8 de diâmetro | Menor que o rato |
| Ponte de casa | 0,9 | = largura das pontes |

> Tudo 2 a 3× o rato, nunca 10×: se parecer "cenário de boneca", está grande demais.

### 8.2 Kit de material (10 peças resolvem 80%)

palito de dente (viga) · caixa de fósforo (parede/casa) · rolha (assento/base) · clipe (grampo/gancho) · pregador (estrutura) · barbante (amarração/corrimão) · fita isolante (remendo) · tampinha (telhado/moeda) · papel alumínio (refletor/impermeável) · prego (pilar).

Cada peça precisa de **2 variantes gastas** (torto, mordido) para não repetir.

### 8.3 Os 6 assentamentos

| # | Nome | Local (coords) | O que conta | Função de jogo |
|---|---|---|---|---|
| 1 | **Vila do Vão** | armário x = -17 (50 de comprimento), y 22–32 | 7 casinhas em duas fileiras, escada de garfo, "praça" na frente da porta entreaberta | Hub visual principal, visto do chão como silhueta pontuda |
| 2 | **Porto da Pia** | borda da pia z = -5, y 12,5 | doca de palitos, 2 barquinhos, vara de pescar, redes | Ponto de "pesca" (item), ensina natação |
| 3 | **Forte de Gelo** | dentro da geladeira, prateleiras y = 6 e 13 | 4 iglus, ponte de gelo entre prateleiras, "muralha" de gelo quebrada | Abrigo (sem dano de frio) |
| 4 | **Forja do Forno** | interior do forno (10 × 10,3) e prateleira y = 8 | bigorna de botão, martelo de prego, fogueira de rato, panela-tampinha | Craft/upgrade (gancho futuro) |
| 5 | **Feira da Gaveta** | gaveta puxada do balcão leste z = 15 | barracas de fósforo, tampinhas como moeda, "balança" de clipe | Comércio/narrativa (gancho) |
| 6 | **Vigia da Mesa** | tampo da mesa (11,5) e cadeira (-11 / 5) | torre de carretel, escada de garfo, "bandeira" de barbante | Ponto de vista / mapa do minimapa |

Extra barato e de alto impacto: **"marcas de gerações"** — riscos de altura na parede perto da vila (1 risco por geração), uma casinha **em construção** (palitos sem telhado), e uma **abandonada** (telhado caído). Isso data a ocupação sem precisar de texto.

### 8.4 Sinais de vida (o que faz parecer habitado, não decorado)

- Fumaça fina em 2 chaminés (partícula, não luz).
- 3 luzes acesas (as demais apagadas) — contraste de "alguns ainda vivem aqui".
- Pegadas e trilhas desgastadas no pó ligando os assentamentos.
- "Fiação" de barbante entre casas (poste de prego).
- Estoque: montinhos de sementes/tampinhas empilhados.
- Um "ninho" abandonado com um objeto pessoal (botão azul).

**Critério de aceite:** visto de qualquer canto do chão, pelo menos 2 assentamentos são reconhecíveis como "feitos por alguém".

---

## 9. Sistema 5 — Estética abandonada e vegetação

### 9.1 Vocabulário vegetal (6 prefabs + 2 materiais)

Não existe nenhum asset orgânico hoje. Proposta: 6 prefabs low-poly no Blender (o projeto já usa `.blend` para rato/prego/palito):

1. `Musgo_Patch` — mancha irregular (quad deformado, 3 tamanhos), `MossWet` e `MossDry`.
2. `Feto_Pequeno` — 5–7 frondes (2 triângulos cada), 1,2–2,5 un de altura.
3. `Trepadeira` — módulo de 4 un com ganchos, empilhável até 12 (sobe parede).
4. `Grama_Tufo` — 4–6 lâminas, 0,6–1,2 un; aparece em rachaduras.
5. `Cogumelo` — 3 variantes, 1 emissiva (`Glow`).
6. `Raiz_Aérea` — fita fina pendurada, vem do teto/vão.

Materiais: `MossWet`, `MossDry`, `Foliage` (2-sided, alpha cutout), `Glow` (emissivo), `Rust`, `Ash`, `Charcoal`, `Frost`, `Cardboard`, `Fabric`.

### 9.2 Regra de distribuição (umidade = vegetação)

| Local | Densidade | O que cresce |
|---|---|---|
| Borda da pia e ralo | alta | musgo molhado, feto, aguapé, trepadeira na torneira |
| Túnel sob balcões | alta (mas escura) | musgo, cogumelo brilhante, raiz |
| Juntas de parede e quinas | média | trepadeira, musgo seco, grama em rachadura |
| Paredes longe da água | baixa | musgo seco em mancha, poeira |
| Raio de 4 un do fogão | **zero verde** | fuligem, ash, teia queimada |
| Geladeira (interior) | zero verde | líquen escuro, gelo |
| Sul aberto (z = +44) | alta | o "lado de fora" tomado: trepadeiras descendo do teto, grama no piso |

### 9.3 Sujeira e "tempo" (a cola de tudo)

- **Poeira:** 1 material com máscara nas superfícies horizontais (tampo, mesa, topo de armário) + partículas de poeira no ar (barato, 200 partículas).
- **Teias:** 6 quads triangulares nas quinas altas; sempre em quina, nunca no meio.
- **Marcas de água:** anéis de calcário embaixo de cada ponto de gotejamento.
- **Rachaduras:** 4 quads/texturas no piso ligando rachadura → grama.
- **Ferrugem:** borda inferior dos balcões metálicos e pés do fogão.
- **Mordidas:** semicírculos no rodapé, nas quinas das portas, nas caixas.
- **Decals:** o `PC_Renderer` **não tem** a feature de Decal. Duas opções: (i) adicionar `DecalRendererFeature` (1 clique, custo baixo no PC; checar `Mobile_RPAsset` depois) — melhor para manchas; (ii) malhas finas apoiadas (0,02 acima da superfície) — zero configuração. Recomendo (ii) para o piso e (i) só se quiser mancha orgânica irregular.

**Critério de aceite:** cobrir a metade esquerda da tela e ainda assim saber em qual bioma o jogador está.

---

## 10. Sistema 6 — Luz e atmosfera

### 10.1 Orçamento e estratégia

Hoje: 1 luz direcional, sem probes, sem fog. Proposta realista:

| Camada | Quantidade | Sombra | Uso |
|---|---|---|---|
| Directional | 1 | sim | sol pelas frestas do forro rebaixado (7.1b) |
| Point/Spot por bioma | 3–4 por bioma (≈ 14) | só 4 com sombra | fogo, geladeira, pia, cogumelos |
| Emissivo sem luz | livre | não | brasas, cogumelo, lâmpada da geladeira |
| `Global Volume` local | 4 (um por bioma) | — | fog + grade + exposição, com blend distance 4 |

- **Fog:** ativar Fog no volume local (não global) — tropical: raso e esverdeado; infernal: quente e denso embaixo; congelado: azul e espesso; úmido: quase sem fog, só escuro.
- **Feixes de luz (god rays):** URP não tem volumétrico nativo → usar 3–5 quads com material aditivo em cone (barato) entrando pela claraboia; a poeira em partículas vende o resto.
- **Probes:** 1 `LightProbeGroup` por bioma (a cena não tem nenhum) e 1 Reflection Probe na pia (a água reflete).
- **Flicker:** um único script `LightFlicker` (ruído + envelope) em fogo e na lâmpada da geladeira. É o efeito de maior impacto por linha de código do plano.

### 10.2 Tabela de biomas

| Bioma | Luz (cor / intensidade / alcance) | Fog | Emissivo | Som |
|---|---|---|---|---|
| Tropical | #7FD3C0 / 3 / 14 | raso, verde | água, cogumelo | gotejamento, coaxar |
| Infernal | #FF7A2A / 5 / 12 + flicker | quente, baixo | brasa | estalo, zumbido |
| Congelado | #8FC8FF / 4 / 16 | azul, denso | lâmpada piscando | compressor, gelo |
| Úmido | #6A7A5A / 1 / 6 | quase zero | cogumelo | gota, roer |
| Neutro | branco / 2 / 20 | zero | — | vento distante |

---

## 11. Organização, produção e riscos

### 11.1 Hierarquia-alvo

```
COZINHA
├─ Arquitetura      (paredes, piso, teto, forro rebaixado)
├─ Eletrodomesticos (geladeira, fogao, forno, microondas, pias)
├─ Moveis           (balcoes, armarios, mesa, cadeiras, vassoura)
│   └─ Balcao_B1 … Balcao_B14  (module + Fachada_*)
├─ Biomas
│   ├─ Tropical / Infernal / Congelado / Umido
├─ Assentamentos    (Vila_01 … Vila_06)
├─ Pontes           (Ponte_01 …)
├─ Vegetacao        (Musgo_*, Feto_*, Trepadeira_*)
├─ Iluminacao       (Luz_*, Volumes)
└─ Volumes          (BiomeTrigger, CameraVolume)
```

Convenções: prefixo por sistema (`Ponte_`, `Fachada_`, `Vila_`, `Musgo_`), pivô sempre na base-centro, escala 1.

### 11.2 Assets a criar

- **Blender:** `Balcao_Aberto`, `Armario_Aberto`, `Fachada_Kit` (6), `Ponte_Kit` (6+2), `Tralha_Kit` (10 × 2 variantes), `Planta_Kit` (6).
- **Materiais:** 10 novos (lista em 9.1).
- **Scripts novos (mínimos):** `LightFlicker`, `BiomeZone` (trigger → aplica volume + som), `CameraVolume`. Nada de novo sistema de interação: portas/gavetas são estáticas.

### 11.3 Riscos medidos

| Risco | Efeito | Mitigação |
|---|---|---|
| **NavMesh não assado** | inimigos não andam direito assim que houver props | assar depois de cada fase; props de detalhe fora de rota ou em layer ignorada |
| **Câmera em espaço apertado** (nicho 7,4; túnel 2,5; sótão 0,76) | clipping/náusea | `CameraVolume` com offset curto + near plane pequeno |
| **Sem batching** (0 objetos static) | draw calls sobem com 300 props | marcar `Static` em cenário; material atlas por kit; LOD só nas plantas |
| **Uma luz só + interiores** | interiores viram preto chapado | luzes/cogumelos emissivos dentro dos nichos (5.2) |
| **Pia sem `WaterVolume`** | rato atravessa água | adicionar o componente existente no trigger do `Water` |
| **Ponha tudo aberto** | perde o abandono (vira casa de boneca) | regra do ⅓ (4.2) |
| **Escala do teto** | sótão sem câmera | forro rebaixado (6.1b) |
| **Sul sem parede** | "cozinha" perde fechamento | decidir: fechar parcialmente (parede quebrada + entulho) ou assumir como "quintal" tomado por vegetação |

---

## 12. Roadmap (fases testáveis)

| Fase | Entrega | Ganho | Esforço | Aceite |
|---|---|---|---|---|
| **0 — Base** | Reorganizar hierarquia; marcar static; assar NavMesh; corrigir 4 objetos fora de prumo; adicionar `WaterVolume` nas pias; `LightFlicker` | Mapa de trabalho limpo | 1 sessão | Cena roda igual, nada flutuando |
| **1 — Interiores** | `Fachada_*` (protótipo "aplicado") em 3 balcões + 1 armário; `Tralha_Kit` v1; luz interna | **O salto de leitura maior de todos** | 2–3 sessões | "Dá para ver dentro" de 3 móveis |

> **Status:** protótipo da Fase 1 **executado na cena** em 2026-11-18 (2 balcões + 1 armário). Ver §13 para o que existe, as diferenças em relação às medidas originais e o que falta para fechar a fase.
| **2 — Verticais** | Forro rebaixado no "U"; 6 pontes + 2 rampas; subida do chão (3 rotas) | Transforma o mapa em 3D jogável | 3 sessões | Chao → armário → armário a pé |
| **3 — Assentamentos** | 3 vilas (Vão, Porto, Forte) + sinais de vida | Conta a história | 3 sessões | 2 vilas legíveis do chão |
| **4 — Biomas** | 4 volumes + luzes + fog + emissivos + âncoras de bioma | Diferencia áreas | 3 sessões | Reconhecer bioma com metade da tela coberta |
| **5 — Natureza** | Plantas, musgo, teias, poeira, mordidas, decals | Data o abandono | 2–3 sessões | Manchas irregulares, nada "limpo" |
| **6 — Polimento** | 3 vilas restantes, ponte quebrada, partículas, som, LOD | Fecha | 2 sessões | Passeio completo sem clipping |

> Se for para fazer **só uma coisa**: Fase 1. Aberturas com interior mudam a leitura da cena mais do que qualquer outro item.

---

## 13. Estado de execução — protótipo Fase 1 (2026-11-18)

> ⚠️ **Não está na cena salva (verificado 2026-11-19).** A cena não contém `Prototipo_Aberturas`: `COZINHA` tem 39 filhos, todos móveis/eletrodomésticos. O protótipo foi descartado ou não foi salvo. Manter esta seção como registro de método, **não** como estado.

> Esta seção foi escrita depois das 1–12: é o registro do que já está na cena.

Construído na cena como **fachada aplicada com primitivas** (caminho B do §5.1), dentro de `/COZINHA/Prototipo_Aberturas`. O grupo raiz fica em local `(0, -7,52, 0)` para que **local = mundo** em todos os filhos (facilita o ajuste fino).

| Grupo | Objeto | O que tem | Medidas aplicadas |
|---|---|---|---|
| `B1_Gaveta_x-25` | balcão norte x = -25 | gaveta puxada com fundo, laterais, trilhos e puxador + 10 props de tralha | frente 6,62 × 2,9; puxada **4,0** (≈35%); interior 6,0 × 4,15; trilhos 0,2 |
| `B2_PortaArrancada_x-15` | balcão norte x = -15 | nicho 6,4 × 6,6 + batente 0,7 + prateleira + porta entreaberta **28°** + porta arrancada encostada + 8 props | nicho 1,7 de profundidade aplicada; batente 0,6 |
| `A1_Portas_x-17` | armário (-17,25 / 27 / -37) | cavidade 12,5 × 7,0 em 3 bays (2 montantes), 3 prateleiras, 2 portas entreabertas (**25°** e **20°**), 1 porta solta, 11 props | vão 2,1 de profundidade aplicada; prateleiras 0,3 |

Totais: **63 malhas + 4 luzes** (1 em cada balcão, 2 no armário), luz quente `1 / 0,82 / 0,58`, intensidade **0,4**, alcance **3**, sombras desligadas (nota §5.2).

Materiais reaproveitados: `BlackConcrete` (interior), `Wood` (batente/porta/gaveta/props de madeira), `Iron` (metais, trilhos, dobradiças, latas), `Quartz` (botão/pote).

### Diferenças conhecidas em relação à spec

- **Profundidade do nicho:** 1,7–2,1 (aplicada) contra 7,4 previstos para o módulo do Blender. É o limite do caminho B — não dá para entrar, só olhar. Fecha na conversão para módulo real.
- **Sem material próprio** de interior/tralha (o gate de comandos destrutivos do editor bloqueou `asset create`/`material set_properties`). O contraste vem dos materiais existentes + luz interna.
- **Sem `LightFlicker`** (é item da Fase 0) e sem prefabs (`prefab create` também não rodou): as peças estão como objetos de cena, prontas para virar `Fachada_*` quando o Blender entrar.
- **Colisão:** as primitivas mantêm seus colisores, então a fachada é sólida (bate-se nela). Nicho não é navegável nesta versão.
- **Não verificado visualmente:** captura de Scene/Game view exige comando destrutivo. A conferência foi numérica (hierarquia, transforms, materiais, luz).

### Para fechar a Fase 1

1. Olhar a cena e ajustar: intensidade das luzes (0,4 → 1,0 se estiver escuro demais), ângulo das portas e o quanto a gaveta sai.
2. Criar `InteriorDark` + 3 materiais de tralha e trocar os provisórios.
3. Converter os três grupos em prefabs `Fachada_GavetaPuxada`, `Fachada_PortaArrancada`, `Fachada_PortaEntreaberta`.
4. Aplicar nos pontos restantes da tabela §5.4 (8 balcões + 6 armários).

## 14. Ganchos extras (para depois, não agora)

- Água molha o rato → apaga brasa → abre passagem no fogão (bioma ↔ bioma).

---

## 15. Revisão 2026-11-19 — estado real da cena + plano de execução em ProBuilder

> **Tipo:** diagnóstico + plano (nada foi construído nesta revisão). **Substitui** as medidas de §2.1 e o relatório de §13. As intenções de §5–§12 continuam válidas; **as medidas, o método e a técnica mudam**, porque a cozinha agora é ProBuilder.
> **Como foi medido:** leitura do YAML de `SampleScene.unity` (transforms + vértices dos `ProBuilderMesh`) e da hierarquia do editor aberto (Code Mode, porta 7800). Nada foi visto em imagem — `capture scene_view` é comando destrutivo e não foi executado. Números são confiáveis; **aparência não foi conferida visualmente**.

### 15.1 O que mudou desde a última medição

| Item | Antes (§2 / §13) | Agora (medido) |
|---|---|---|
| Construção | malhas fechadas, vindas do Blender | **87 malhas ProBuilder** em 176 GameObjects, tudo editável na cena |
| Filhos de `COZINHA` | ~90 objetos soltos | **39** (2 `Pia`, 18 `Balcao`, 1 `Mesa`, 4 `Cadeiras`, 1 `Ground`, 1 `Roof`, 1 `Fogao`, 1 `Geladeira`, 1 `Vassoura`, 5 `Armario`, 1 `Microondas`, 3 `Wall`) |
| Protótipo de aberturas | “executado” em §13 | **não existe na cena salva** (cena limpa, `isDirty: false`) |
| Pé-direito | 32,76 (laje em 32,76) | igual: piso 0, teto 32,76; **paredes subiram para 35 de altura** |
| Geladeira | 20 × 32 × 15,5 | **12 × 28 × 12** em (-37 / 13 / 5) → **topo em 27** |
| Mesa | centro, tampo 11,5 | 20 × 12,5 × 40 em **(0 / 11 / 10)**, tampo 11,5 |
| `WaterVolume` nas pias | ausente | **presente nas 2 pias** (componente já ligado) |
| NavMesh | nenhum assado | **`Assets/Scenes/SampleScene/NavMesh-Ground.asset`** assado no `Ground` |
| Objetos estáticos | 0 | **66** com `StaticEditorFlags` |
| Luzes | 1 direcional | 1 direcional (nada mais). **Nenhum `Volume` na cena** |
| Prefabs da cozinha | — | **nenhum**: os 18 balcões e 5 armários são cópias soltas na cena |

### 15.2 Raio-X da construção (medido, espaço de mundo)

| Elemento | Qtd | Posição | Dimensões | Notas |
|---|---|---|---|---|
| `Ground` | 1 | (0 / -0,5 / 0) | 90 × 1 × 90 | piso em y = 0 |
| `Roof` | 1 | (0 / 33,26 / 0) | 90 × 1 × 90 | teto interior em 32,76 |
| `Wall` | 3 | z = -44, x = ±44 | 90 × 35 × 2 | **lado sul (z = +44) aberto** |
| `Balcao` | 18 | ver abaixo | 10,66 × 10 × 10 (corpo) | **tampo em 12,52; base em 2,52** — nenhum toca o chão |
| `Balcao` cantos | 2 | (±37 / 7,52 / -37) | 10,5 × **13,52** × 10,5 | base em 0,76; são as “torres” do U |
| `Armario` | 5 | y = 27 | 10 × 10 × comp. 20/25/30/50/70 | y 22 → 32; folga ao teto **0,76** |
| `Geladeira` | 1 | (-37 / 13 / 5) | 12 × 28 × 12 | topo 27; prateleiras y = 6 e 13; **17 blocos de gelo**, 2 abaixo do piso |
| `Fogao` | 1 | (15 / 6 / -37) | 12 × 14 × 12 | topo 13; 4 bocas em y = 13,13; prateleiras internas y = 4 e 8 |
| `Forno` | 1 | (15 / 5,05 / -29,11) | 10 × 9,65 × 5,03 | solto à frente do fogão, topo 9,88 |
| `Pia` | 2 | (37,56 / 7,52 / -5 e -15) | 10,66 × 10 × 10 | água 9,9 × 9 × 10 com topo em **11,83** (0,69 abaixo do tampo) |
| `Microondas` | 1 | (36,13 / 15 / 32,08) | 6 × 5 × 12 | porta aberta (4,8 × 4) + `Prato` dentro em 13,17 |
| `Mesa` + `Cadeiras` | 1 + 4 | (0 / 11 / 10) | 20 × 12,5 × 40 | tampo 11,5; cadeiras com topo em 17 |
| `Vassoura` | 1 | (29,78 / 10,76 / 23,97) | 16,9 × 15,6 × 1,24 | **base flutua ~2,96** acima do chão |
| `TransitionDoor` | 2 | (15 / 2 / -42,6) e (-42,68 / 2 / 5) | 3,75 × 2,75 × 1 | passagens no tamanho do rato |
| `Water` + `WaterVolume` | 2 | nas pias | 9,9 × 9 × 10 | natação já funciona |

**Layout dos balcões (o “U”):**

| Fila | Posições | Face frontal (para o centro) |
|---|---|---|
| Norte | z = -37,56; x = -25, -15, -5, 5, 25 | **z = -32,23** |
| Oeste | x = -37,56; z = -25, -15, -5, 15, 25, 35 | **x = -32,23** |
| Leste | x = +37,56; z = -25, 5, 15, 25, 35 | **x = +32,23** |
| Cantos | (±37 / -37) | 1,5 mais altos (topo 14,04) |

Os vazios do U são preenchidos por eletrodomésticos, e isso está **coerente**: fogão em x = 15 (entre os balcões de x = 5 e 25), pias em z = -5 e -15 (entre -25 e 5), geladeira em z = 5 (entre -5 e 15). A planta funciona.

### 15.3 Diagnóstico

**O que já está resolvido (não mexer):**

1. **Escala de colosso**: 88 × 88 com bancada de 12,52 de altura (~1,5 m na escala humana) — a fantasia de rato está pronta.
2. **Circuito em U contínuo**: corredor de 9,48 de altura entre tampo e armários, e um **vão livre de 2,52 sob todos os balcões** — o “subsolo” já existe, e é contínuo nos três lados. Nada precisa ser escavado para o bioma úmido.
3. **Verticalidade latente**: tampo (12,52) → armário (22) → topo de armário (32) → teto (32,76). Só falta rota.
4. **Biomas já ancorados nos objetos**: água (pias), gelo (geladeira), fogo (bocas), sombra úmida (vão inferior).
5. **Duas portas de saída no tamanho do rato** — bocas prontas atrás do fogão e atrás da geladeira.
6. **Micro-ondas**: único interior legível hoje (porta aberta + prato). É a prova de que “interior com conteúdo” funciona nesta cena.
7. **Natação** da pia já operacional (`WaterVolume`).

**O que trava a leitura de “civilização vivendo aqui há muito tempo”:**

1. **Nada tem dentro.** Todos os módulos são sólidos: 24 vértices por armário (= um cubo maciço), 488 por balcão. Não existe cavidade, gaveta, porta nem prateleira visível — e sem “dentro” não existe história.
2. **Uma única luz (direcional).** Sem luz local, interior é preto chapado e bioma é indiferenciado.
3. **Zero vocabulário orgânico.** Nenhum material de musgo, folha, ferrugem, fuligem, papelão ou tecido. Nenhuma planta, nenhuma teia, nenhuma poeira.
4. **Zero tralha de rato.** Não há nada construído por bichos: nem palito, nem barbante, nem rolha.
5. **Zero pontes e zero rotas verticais** para o jogador. A bancada é um penhasco de 12,52 sem escada.
6. **Nada é instância.** 18 balcões e 5 armários são cópias independentes; qualquer variante aberta terá de ser feita 20 vezes sem prefab.
7. **Poeira/idade não têm como aparecer**: sem oclusão local, sem contraste de material, sem marcas.

**Achados que precisam de saneamento antes de construir por cima** (baratos, mas visíveis):

| # | Achado | Medida do erro |
|---|---|---|
| 1 | `Vassoura` flutua | base em y ≈ 2,96 (deveria ser 0) |
| 2 | Cadeira em (-21,63 / 8,54 / 15) com escala 1,96 e torcida | deveria sentar no piso, escala 1 |
| 3 | 11 `Item` flutuam sobre a mesa | y = 14,41 contra tampo 11,5 → **2,9 de vazio** |
| 4 | 2 blocos de `Gelo` abaixo do piso | topos em -5,67 e -0,15 |
| 5 | `Geladeira` invade 1,33 os balcões vizinhos | balcão de z = -5 termina em z = 0,33; geladeira começa em z = -1 |
| 6 | NavMesh só no `Ground` | qualquer prop sobre a rota muda a malha; re-assar por fase |

### 15.4 Por que ProBuilder muda o plano

O gargalo do plano original (§5.1) era geométrico: abrir um móvel exigia Blender. **Isso acabou.** Com os módulos já em ProBuilder, um interior é uma operação de dois passos (**Inset + Extrude negativo**) no próprio objeto — com colisor gerado automaticamente e sem sair do editor.

Consequências práticas:

- **Caminho único**: esqueça a comparação Blender × fachada aplicada. Tudo se faz na cena.
- **Profundidade real**: o interior passa de 1,7 (fachada colada) para **6,5–7,5 navegáveis**.
- **Custo dominante deixa de ser triângulo e passa a ser objeto + colisor**: cada `ProBuilderMesh` traz `MeshCollider`, e 200 props com `MeshCollider` pesam. Ver §15.13.
- **Prefab é obrigatório**, não opcional: sem instância, corrigir 20 módulos significa 20 cirurgias.

### 15.5 Sistema A — gavetas e portas entreabertas com interior

#### 15.5.1 Receita 1 — cavidade (o interior) em um módulo existente

Base de tudo. Funciona em `Balcao`, `Armario`, `Mesa` e no `Forno`.

1. Selecionar o módulo (ex.: `Balcao` em x = -15) → `Tools > ProBuilder > ProBuilder Window`.
2. Entrar em **Face** mode e clicar na **face frontal** (a que olha para o centro da sala).
3. **Inset** (`Actions > Inset`) com *Amount* = **0,55** → cria o batente; o miolo continua selecionado.
4. **Extrude** (`Actions > Extrude`) com *Distance* = **-6,5** → empurra o miolo para dentro da normal, gerando paredes laterais e fundo. (negativo = para dentro)
5. Conferir as faces internas: se alguma aparecer preta/invertida, `Actions > Flip Normals` só nela.
6. Com as faces internas selecionadas, atribuir **`InteriorDark`** (material novo, §15.9) — sem isso o nicho é branco e some.
7. Prateleira: `New Shape > Cube` **8,0 × 0,3 × 5,5**, encostar no fundo com recuo de 0,5 da boca. Pode ficar filha (não precisa `Merge Objects`).
8. Colisor: manter o `MeshCollider` (convexo = **off**, é estático) — o rato passa a poder entrar e não atravessar.
9. Renomear (`Balcao_N2_Nicho`) e salvar como **prefab** antes de duplicar.

> **Medida útil por tipo:** balcão 10,66 de profundidade → nicho **6,5** (sobram 2,6 de casca). Armário 10 de profundidade → nicho **6,2** (sobram 1,9 de parede traseira + 0,55 de batente). Não passar de 7,5: o fundo fica a menos de 1,3 da parede da sala.

#### 15.5.2 Receita 2 — gaveta puxada

1. `New Shape > Cube` **4,6 × 1,8 × 4,2** (a caixa da gaveta).
2. Face mode → selecionar a **face superior** → **Inset 0,35** → **Extrude -1,5** (fundo a 0,3).
3. Frente: outro Cube **5,0 × 3,0 × 0,5** colado na boca da caixa; puxador = Cube 1,6 × 0,3 × 0,3 (ou `Cylinder` achatado, 8 lados).
4. Trilhos: 2 Cubes **4,2 × 0,2 × 0,2** nas laterais, recuados 0,3.
5. Posicionar: deslocar **+3,0 a +3,5** no eixo normal da fachada (≈35% do curso) e **1,5 abaixo** da boca do nicho, para ler “puxada”.
6. Grupo `Gaveta_*` com pivô na **base-centro**; prefab.

**Faixa vertical recomendada no balcão** (frente tem 10 de altura, de 2,52 a 12,52): gaveta **8,5 → 11,5** (3 de altura); portas **3,0 → 8,2** (5,2). Assim qualquer módulo aceita gaveta em cima + 2 portas embaixo sem redesenho.

#### 15.5.3 Receita 3 — porta com dobradiça

1. `New Shape > Cube` **largura × altura × 0,45** (folha).
2. Criar um **Empty** `Hinge_*` **na borda** da folha (na face, na altura do meio).
3. Parentear a folha ao Empty e ajustar `localPosition` = (+largura/2, 0, 0) → a folha passa a girar sobre a quina.
4. Rotacionar o **Empty** em Y: **20°–30°** (abaixo disso lê fechada; acima de 45° vira convite óbvio).
5. Dobradiças: 2 cilindros 0,25 × 0,5 no eixo.
6. Porta **arrancada**: mesmo objeto, Empty a 65–80°, folha deslocada −0,5 em Y e 8° em Z; uma dobradiça caída no chão.

> O pivô na borda é o detalhe que faz a diferença: porta que gira pelo centro parece placa solta.

#### 15.5.4 Receita 4 — buraco roído

O balcão **não tem rodapé**: o corpo começa em 2,52. Então o “buraco” é uma abertura na **parte baixa da face frontal** (y 3,0–5,0), não no piso.

1. Face frontal → **Inset 0,9** → **Extrude -5,0** (cavidade rasa, tipo toca).
2. Selecionar a face interna e aplicar **Bevel** pequeno (0,2) para quebrar a circularidade perfeita.
3. Opcional: mover 2–3 vértices internos 0,15 para o buraco ficar irregular.
4. Serragem no chão (plano 2 × 2 com material `MossDry`) + marcas de dente (3 semicírculos de 0,25).

#### 15.5.5 O que vai dentro (senão é buraco preto)

Kit mínimo, tudo entre **0,6 e 2,5** (mobília de rato). Cada peça: 1–3 primitivas ProBuilder.

| Peça | Primitiva e medida | Onde aparece |
|---|---|---|
| Rolha | Cylinder 0,8 × 1,2 | gaveta, feira |
| Tampinha | Cylinder 0,9 × 0,15 | moeda, telhado |
| Caixa de fósforo | Cube 2,0 × 0,7 × 1,3 | parede, casa |
| Carretel | Cylinder 1,0 × 1,4 + 2 discos | torre, mesa |
| Pregador | 2 Cubes 0,2 × 2,0 × 0,3 + mola | ponte, estrutura |
| Clipe | 3 segmentos finos | gancho, grampo |
| Palito | Cube 0,08 × 3,0 × 0,08 | viga, ponte |
| Botão | Cylinder 0,35 × 0,08 | detalhe pessoal |
| Barbante | Cylinder 0,06 × comp., 6 lados | amarração, corrimão |
| Latinha | Cylinder 0,6 × 1,1 | lata, panela |

Dentro dos módulos **humanos** o conteúdo é humano (potes, latas, sacos, cereais); dentro dos nichos **de rato** é tralha. Essa troca de vocabulário é o que conta a história.

#### 15.5.6 Distribuição — 21 intervenções, coordenadas reais

Regra do ⅓ (§4.2): **não abrir tudo**. Plano: 6 gavetas, 8 portas entreabertas, 3 arrancadas, 4 buracos roídos, em 18 balcões + 5 armários.

**Bancadas** (fachadas: norte z = -32,23 · oeste x = -32,23 · leste x = +32,23)

| # | Módulo | Intervenção | Conteúdo | Papel |
|---|---|---|---|---|
| 1 | norte x = -25 | gaveta puxada 3,2 + gaveta caída no chão | tralha, tampinhas | **degrau** da rota de subida |
| 2 | norte x = -15 | porta arrancada | potes, pano, teia | interior legível do saguão |
| 3 | norte x = -5 | buraco roído (y 3–5) | serragem, ninho | entrada do subsolo |
| 4 | norte x = 5 | gaveta 35% | garfos, barbante | leitura a 10 un |
| 5 | norte x = 25 | porta entreaberta 26° | latas | equilíbrio junto ao fogão |
| 6 | oeste z = -25 | gaveta caída + entulho | caixas | plataforma baixa/lixo |
| 7 | oeste z = -15 | porta arrancada | tralha + musgo | abandono do lado gelado |
| 8 | oeste z = -5 | buraco roído + fungo | ninho | boca do subsolo oeste |
| 9 | oeste z = 15 | gaveta puxada 40% | tampinhas empilhadas | **Feira** |
| 10 | oeste z = 25 | porta entreaberta 22° | cereais | ritmo do corredor |
| 11 | oeste z = 35 | buraco roído | raízes | canto esquecido |
| 12 | leste z = -25 | porta entreaberta 30° | potes úmidos | transição pia ↔ armário |
| 13 | leste z = 5 | gaveta puxada 30% | botões | leitura central |
| 14 | leste z = 15 | gaveta puxada 40% | barracas | **Feira da Gaveta** (§15.7) |
| 15 | leste z = 25 | porta arrancada | sucata | abandono leste |
| 16 | leste z = 35 | porta entreaberta 24° | potes | junto ao micro-ondas |
| 17 | canto oeste (-37/-37) | porta entreaberta no nível alto | nada (silhueta) | torre/gate |

**Armários** (fachadas: z = -32 · x = ±32)

| # | Módulo | Intervenção | Conteúdo | Papel |
|---|---|---|---|---|
| 18 | A1 (-17,25 / -37, comp. 50) | 3 bays: 2 portas entreabertas (25° e 20°) + 1 porta solta | potes, caixas, **vila dentro** | interior principal, visto do chão |
| 19 | A2 (32 / -37, comp. 20) | 1 porta entreaberta 30° | tralha | depósito |
| 20 | A3 (-37,25 / -17, comp. 30) | 1 porta 45° (aberta) | vazio com teia | atalho de ponte |
| 21 | A4 (37 / 3, comp. 70) | 2 entreabertas (25° e 30°) + 1 vão sem porta (nicho-ninho) | vila/estoque | “prédios” leste |
| 22 | A5 (-37,25 / 24, comp. 25) | 1 porta arrancada | sucata | abandono no alto |

**Custo estimado:** 21 intervenções ≈ 250–450 tris cada + 10 props por abertura ≈ **12k tris** e **~+120 objetos**. Aceitável em triângulo; o que pesa é colisor (§15.13).

**Aceite:** parado a 3 un de um balcão, dá para dizer o que tem dentro; e o rato entra em pelo menos **3** nichos.

### 15.6 Sistema B — pontes do tamanho do rato

#### 15.6.1 Vãos reais (onde a ponte cabe)

| Vão | Onde | Medida | Leitura |
|---|---|---|---|
| V1 | A1 termina em x = 7,75 → A2 começa em x = 22 | **14,25 de vão**, altura y ≈ 26 | a ponte principal, **sobre o fogão** |
| V2 | A3 termina em z = -2 → topo da geladeira (z = -1 .. 11, y = 27) → A5 começa em z = 11,5 | 2 saltos curtos + 3 de altura | “passagem do Polo Norte” |
| V3 | A4 (x = 32, y 22–32) → micro-ondas (topo 17,5, z = 32) | 4,5 de descida | escada de gancho |
| V4 | cadeira x = -11 (topo 17) ↔ cadeira x = 11 (topo 17) | 22 de vão sobre a mesa (tampo 11,5) | ponte alta da praça |
| V5 | tampo do balcão (12,52) ↔ fundo do armário (22) | **9,48** | drop/rampa — precisa de intermediários |
| V6 | subsolo (0 → 2,52) nos três lados | contínuo | rota segura |

#### 15.6.2 Tipologias e como fazer em ProBuilder

| Peça | Como construir | Largura | Vão |
|---|---|---|---|
| Prancha de palitos | Cube 0,9 × 0,2 × comp. + `Inset` nas pontas; 3 palitos por baixo (0,08) | 0,9 | 4–6 |
| Ponte de régua | Cube plano + 2 corrimãos (Cube 0,1 × 0,6) + barbante | 1,0 | 6–9 |
| Ponte de pregadores | 2 pregadores como apoios + 3 travessas | 0,8 | 3–5 |
| Pênsil de barbante | 2 cordas (Cylinder 0,06, 6 lados) + 6 tábuas 0,5 | 0,5 | 8–14 |
| Escada de garfo | 2 Cabos + 6 dentes (Cube 0,08) | 0,7 | vertical |
| Ponte quebrada | metade da pênsil + ponta pendurada 40° e 2 tábuas caídas | 0,8 | 4 |
| Rampa de espátula | Cube 0,8 × 0,15 × comp. rotacionado | 0,8 | desnível 2–4 |

`New Shape > Stair` serve para os degraus de subida (garfo/caixa de fósforo) — economiza montagem manual.

#### 15.6.3 Rotas propostas

```mermaid
flowchart LR
  subgraph Alto[Armarios y=22..32]
    A1[A1 x-17 / 50]
    A2[A2 x32 / 20]
    A3[A3 z-17 / 30]
    A4[A4 z3 / 70]
    A5[A5 z24 / 25]
  end
  GEL[Geladeira topo y27]
  BAN[Bancada y12,52]
  FOG[Fogao topo y13]
  PIA[Pias agua y11,83]
  MESA[Mesa tampo 11,5]
  SUB[Subsolo 0..2,52]
  A1 -- V1 pensil 14,25 --> A2
  A3 -- V2 rampa --> GEL -- V2b --> A5
  A4 -- V3 escada --> BAN
  BAN -- V5 corda/patamares --> A1
  A2 -- rampa de espatula --> FOG
  BAN -- salto --> PIA
  SUB -- 4 buracos --> BAN
  MESA -- V4 ponte alta --> MESA
```

1. **Rota Norte (principal):** do armário A1 (x = 7,75) à ponte pênsil V1 até A2 (x = 22), y ≈ 26. Dois pontos de suspensão ancorados nos batentes. **Passa em frente ao fogão**: metade das cordas chamuscada, uma tábua queimada — a ponte “sentiu” o calor.
2. **Rota Oeste (gelada):** A3 (y 24) → rampa curta → topo da geladeira (y 27) → rampa → A5. Aproveitar as **2 portas soltas** (`Porta` em -27,5/10/3 e -29/23/4) como pranchas — material narrativo pronto.
3. **Rota Leste:** A4 (y 22) → ponte curta ao tampo da pia (12,52) via 3 patamares + corda. Do micro-ondas (17,5) sobe escada de gancho até A4.
4. **Atalho do forno:** do tampo (12,52) desce rampa de espátula até o **topo do forno** (9,88) e entra na boca (10 × 9,65).
5. **Subida do chão (3 caminhos, sem teleporte):** (i) caixa de fósforo (1,2) → gaveta caída (3,0) → gaveta puxada (6,2) → corda até 12,52 [tudo em x = -25]; (ii) vassoura (após saneada: 0 → 18,56) + barbante até o armário; (iii) porta solta como rampa no canto oeste.
6. **Rota dos fundos:** o vão de 2,52 sob os 18 balcões conecta os 4 buracos roídos e as 2 `TransitionDoor`. É a rota segura (inimigo grande não passa).

**Regra:** cada rota tem **1 vão quebrado** (V1 tem 2 tábuas caídas). Isso dá leitura de perigo e marca de tempo.

**Aceite:** ir do chão ao topo de um armário e atravessar para outro **a pé**, sem pular no vazio e sem clipping de câmera.

### 15.7 Sistema C — mini construções de tralha (a civilização)

**Escala:** casinha 2,0 × 1,2 × 2,5; porta 0,9 × 1,0; telhado 35–40°; fogueira 0,8. Tudo **2–3× o rato**, nunca 10×.

**Como construir em ProBuilder:** caixa de fósforo = Cube 2,0 × 0,7 × 1,3; parede = 4–6 caixas empilhadas com 3–5° de inclinação cada (é o desalinho que vende “feito à mão”); telhado = 2 Cubes finos rotacionados 35°; telhado de tampinha = Cylinder achatado; amarração = Cylinder 0,06. Sempre `Merge Objects` em casas de 6+ peças para não explodir a contagem de objetos.

| # | Assentamento | Onde (coords reais) | O que tem | Função |
|---|---|---|---|---|
| 1 | **Vila do Vão** | dentro de A1: x -40..6, y 22,6..31, z -37 | 7 casas em 2 fileiras, escada de garfo, praça em frente ao vão da porta | hub visual visto do chão |
| 2 | **Porto da Pia** | borda leste da pia z = -5: x 32,6..42, y 12,52..13,4 (água em 11,83) | doca de palitos, 2 barcos de casca, vara de pescar, rede | ponto de pesca; ensina natação |
| 3 | **Forte de Gelo** | topo da geladeira (x -42..-32, z 0..10, y 27) + prateleiras internas y 6 e 13 | 4 iglus, muralha quebrada, bandeira de barbante | abrigo do bioma congelado |
| 4 | **Forja do Forno** | topo do forno (15 / 9,88 / -29) + interior 10 × 9,65 | bigorna de botão, martelo de prego, fogueira de rato | craft/upgrade (gancho) |
| 5 | **Feira da Gaveta** | gaveta puxada do balcão leste z = 15 | barracas de fósforo, tampinhas como moeda, balança de clipe | comércio/narrativa |
| 6 | **Vigia da Mesa** | tampo (0 / 11,5 / 10) + cadeira x = 11 | torre de carretel, escada de garfo, bandeira | ponto de vista |

**Sinais de vida (o que faz parecer habitado, não decorado):** fumaça fina em 2 chaminés (partícula, não luz); 3 luzes acesas de 12; trilhas desgastadas no pó ligando os assentamentos; fiação de barbante entre casas (poste de prego); estoques de tampinhas; 1 ninho abandonado com um botão azul; 1 casa **em construção** (palitos sem telhado) e 1 **abandonada** (telhado caído); 3 riscos de altura na parede (marcas de gerações).

**Aceite:** de qualquer ponto do chão, **2 assentamentos** são reconhecíveis como feitos por alguém.

### 15.8 Sistema D — os biomas, aplicados a estas medidas

As 5 assinaturas de §4.5 continuam valendo (luz, material, vegetação, tralha, som). Abaixo só o que muda com a geometria real.

| Bioma | Área real | Âncora (leitura de longe) | Luz | Vegetação | Borda |
|---|---|---|---|---|---|
| **Tropical** | pias x 32–44, z -22..2, água y 2,83–11,83 | anel de calcário na borda z = -5 + torneira pingando | ciano `#7FD3C0`, 3 pts fracos | samambaia na beirada, aguapé na água, musgo no ralo | perde força a 4 un da pia |
| **Infernal** | fogão x 8..22, z -44..-28, y 12..16 (+ forno) | parede de fuligem de x 9..21 subindo até y 22 | laranja `#FF7A2A`, 2 pts nas bocas + flicker | **zero verde** num raio de 4 | fuligem + cinza; verde só a 6+ |
| **Congelado** | geladeira x -44..-30, z -2..12, y 0..28 | língua de geada 2 un além do móvel, no piso e na parede | azul `#8FC8FF`, 4 + lâmpada piscando | líquen escuro, nada verde | geada irregular, seguindo junta |
| **Úmido** | vão 0..2,52 sob os 18 balcões + 4 buracos | marcas de dente + serragem | quase nenhuma: cogumelo emissivo a cada 8–10 | musgo em 60% das bases, fetos, raízes | escuro; só sobe pelos buracos |
| **Neutro** | mesa (0/11,5/10) + cadeiras | poeira e teias nas quinas | branco difuso | nenhuma | respiro entre biomas |

**Detalhe de ouro do bioma congelado:** o topo da geladeira (27) é uma **praça** de onde se vê tudo; e a lâmpada interna ainda piscando depois de décadas é o melhor “abandono” que a cena pode ter. **Do tropical:** a água está 0,69 abaixo do tampo — dá para fazer uma borda de pedra e o rato entrar pela água.

**Fog:** antes de prometer neblina local, verificar se o URP 17 desta instalação expõe *override* de Fog por `Volume`. Se não expuser, resolver com **partículas de névoa** (3 sistemas) — não vale trocar o pipeline por isso.

### 15.9 Sistema E — vegetação e abandono

**Receitas ProBuilder (não precisa de Blender):**

| Peça | Construção | Medida | Custo |
|---|---|---|---|
| Mancha de musgo | `New Shape > Plane` 3 × 3 com 3 × 3 subdivisions → Vertex mode → mover Y entre -0,12 e +0,12 (borda mais baixa) → material `MossWet`; 3 tamanhos | 2–4 | 18 tris |
| Tufo de grama | Cube 0,06 × 1,1 × 0,02, duplicar 5 em leque; ou Plane 0,4 × 1,2 com topo deslocado | 0,6–1,2 | 10 tris |
| Trepadeira | Cube 0,25 × 4 × 0,08 → `Subdivide` 4 ao longo → mover vértices em X para serpentear; empilhar até 12 | módulo 4 | 60 tris |
| Cogumelo | Cylinder 0,25 × 0,8 (haste) + Cone 1,2 × 0,5 (chapéu) → `Merge Objects` | 0,6–1,4 | 40 tris |
| Raiz aérea | Cylinder 0,12 × 3, 6 lados, ponta entortada | 3–6 | 24 tris |
| Teia | 4 fios finos (Cube 0,02) em quina + 1 triângulo; nunca no meio da parede | 1–2 | 12 tris |

**Materiais a criar (não existe nenhum orgânico hoje):** `InteriorDark`, `MossWet`, `MossDry`, `Foliage`, `Rust`, `Ash`, `Charcoal`, `Frost`, `Cardboard`, `EmberGlow`. URP/Lit com uma textura por material; reaproveitar texturas existentes (`Cabeluda` para musgo, `Wood` para papelão) evita produzir arte nova agora.

**Distribuição (umidade = vegetação):**

| Local | Densidade | O que cresce |
|---|---|---|
| Borda da pia e ralo | alta | musgo molhado, feto, aguapé, trepadeira na torneira |
| Vão sob os balcões | alta (escura) | musgo, cogumelo brilhante, raiz |
| Juntas de parede/quinas | média | trepadeira, musgo seco, grama em rachadura |
| Paredes longe da água | baixa | mancha de musgo seco, poeira |
| Raio de 4 un do fogão | **zero** | fuligem, cinza, teia queimada |
| Interior da geladeira | zero | líquen, gelo |
| **Lado sul aberto (z = +44)** | alta | o “quintal” tomado: trepadeiras descendo, grama no piso |

**A fachada sul aberta é uma oportunidade, não um bug:** transformá-la na borda do mundo — entulho, trepadeira descendo do teto e luz entrando rasante. Se o fechamento for desejado, fazer parede **quebrada** com entulho, nunca lisa.

### 15.10 Luz e atmosfera (orçamento real)

| Camada | Quantidade | Sombra | Uso |
|---|---|---|---|
| Directional | 1 (existe) | sim | sol vindo do lado sul aberto |
| Point por bioma | 12–14 | só 4 | bocas do fogão, pia, geladeira, cogumelos |
| Spot | 3 | 1 com sombra | vão do exaustor, sobre a pia, sobre a vila |
| Emissivo (sem luz) | livre | — | brasa, cogumelo, lâmpada da geladeira, `EmberGlow` |
| `Volume` local | 4 (não há nenhum hoje) | — | ajuste de exposição/contraste por bioma, blend 4–6 |
| Probes | 1 `LightProbeGroup` por bioma + 1 `ReflectionProbe` na pia | — | sem probe, props escuros ficam chapados |

Script mínimo de alto impacto: **`LightFlicker`** (~15 linhas, ruído em intensidade + posição) na lâmpada da geladeira e nas 2 bocas acesas. Nada mais precisa de código: portas e gavetas são estáticas.

**Aceite:** cobrir metade da tela e ainda assim saber em que bioma o jogador está.

### 15.11 Organização e prefabs

**Hierarquia-alvo** (hoje: 39 filhos soltos em `COZINHA`):

```
COZINHA
├─ Arquitetura      (Ground, Roof, Wall, forro rebaixado)
├─ Eletrodomesticos (Geladeira, Fogao, Forno, Microondas, Pias, Water)
├─ Moveis
│   ├─ Balcao_N1 … Balcao_L5   (módulo + Fachada_*)
│   └─ Armario_A1 … A5
├─ Pontes / Assentamentos / Vegetacao
├─ Iluminacao
└─ Volumes         (BiomeTrigger, CameraVolume)
```

**Fluxo ProBuilder + prefab (obrigatório antes de duplicar):**

1. Construir **um** módulo mestre (`Balcao_Mestre`).
2. Arrastar para `Assets/Prefabs/Moveis/` → prefab.
3. Variantes: `Fachada_Gaveta`, `Fachada_Porta`, `Fachada_Buraco` como prefabs próprios, e o módulo recebe-os como filhos.
4. Substituir as 18 cópias atuais por instâncias.
5. **Prefab Variant** para os cantos (13,52) em vez de geometria própria.

### 15.12 Roadmap por fases (revisado para ProBuilder)

| Fase | Entrega | Esforço | Aceite |
|---|---|---|---|
| **0 — Saneamento** | agrupar hierarquia; corrigir vassoura/cadeira/itens/gelo; prefabs dos 18 balcões; re-assar NavMesh | 1 sessão | nada flutuando; cena idêntica à vista |
| **1 — Interiores** | Receitas 1–3 em **6 balcões + 1 armário** (norte x -25 e -15, leste z 5 e 15, oeste z 15, canto oeste, A1) + `InteriorDark` + 3 luzes internas | 2–3 sessões | ver dentro de 6 móveis; entrar em 2 nichos |
| **2 — Verticais** | forro rebaixado sobre o U (0,76 → 3,26 de folga); V1 pênsil; V2 geladeira; 3 subidas do chão | 3 sessões | chão → armário → armário a pé |
| **3 — Assentamentos** | Vilas 1, 2 e 3 + sinais de vida | 3 sessões | 2 vilas legíveis do chão |
| **4 — Biomas** | 4 volumes + 14 luzes + âncoras + `LightFlicker` | 3 sessões | reconhecer bioma com metade da tela coberta |
| **5 — Natureza** | plantas, musgo, teias, poeira, mordidas, decals | 2–3 sessões | manchas irregulares, nada limpo |
| **6 — Polimento** | vilas 4–6, ponte quebrada, partículas, som | 2 sessões | passeio completo sem clipping |

> Se for para fazer **só uma coisa**: Fase 1, começando pelos dois balcões do norte (x = -25 e -15), porque são os que o jogador vê de frente ao entrar.

### 15.13 Riscos específicos de ProBuilder

| Risco | Efeito | Mitigação |
|---|---|---|
| **`MeshCollider` em todo prop** | 200 props ≈ 200 colisores de malha | em peça fora de rota, remover `MeshCollider` e pôr `BoxCollider`; em assentamento, 1 colisor agregado |
| **Sem batching automático** | draw calls sobem com 300 objetos | marcar `Static` (66 já estão), compartilhar ≤3 materiais por kit, `Merge Objects` em casas |
| **Extrude negativo com face não planar** | triângulos tortos/artefato | `Actions > Triangulate` depois de mover vértices |
| **UV esticada nas faces novas** | textura “puxada” no nicho | usar material liso/cor no interior em vez de textura detalhada |
| **Pivô no centro** | porta girando como placa | Empty na borda (Receita 3) |
| **Abrir demais** | vira casa de boneca, perde abandono | regra do ⅓ (§15.5.6): 21 intervenções, nunca 40 |
| **NavMesh só no `Ground`** | props novos mudam a rota do inimigo | re-assar por fase; props fora da rota em layer ignorada |
| **Câmera em espaço apertado** | 6,5 no nicho, 2,5 no subsolo, 0,76 no sótão | `CameraVolume` (offset curto) + forro rebaixado |
| **Vão sul aberto** | parece cena inacabada | assumir como “quintal” tomado por vegetação, ou parede quebrada |

### 15.14 Ganchos (continua valendo §14)

- Água (tropical) apaga brasa (infernal) → abre passagem no fogão.
- Gelo escorregadio + `Glider` = atalho das pontes para a mesa.
- Formigas disputando a Feira da Gaveta.
- Minimapa (`Minimap` existe) com tint por bioma.
- Diário de gerações: 3 riscos de altura na parede da Vila do Vão.

- Gelo escorregadio + `Glider` = atalhos aéreos.
- Formigas como segunda facção disputando a feira.
- Minimapa (`Minimap` já existe) com tint por bioma.
- "Diário" de gerações: 3 riscos de altura na parede contam a cronologia sem texto.
