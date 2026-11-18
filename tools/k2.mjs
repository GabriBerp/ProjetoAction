// Fase 1 — variações de fachada nos demais móveis.
// Cada móvel = grupo filho de Prototipo_Aberturas, posicionado na FACE do móvel e com
// rotação Y para que o eixo local +z aponte para dentro da cozinha. Peças em coords locais.
const RAIZ = '/COZINHA/Prototipo_Aberturas';
const MAT = {
  black: 'Assets/Materials/BlackConcrete.mat',
  wood: 'Assets/Materials/Wood.mat',
  iron: 'Assets/Materials/Iron.mat',
  quartz: 'Assets/Materials/Quartz.mat',
};
const J = (a) => JSON.stringify(a);
const INICIO = 0; // índice inicial (inclusive)
const FIM = 2; // índice final (exclusive)

// ---------- templates ----------
function nicho(pr, { W, H, y0, yTop }) {
  const cy = (y0 + yTop) / 2;
  return [
    { n: `${pr}_Fundo`, p: 'cube', x: 0, y: cy, z: 0.14, s: [W, H, 0.2], m: 'black' },
    { n: `${pr}_Topo`, p: 'cube', x: 0, y: yTop + 0.15, z: 0.85, s: [W, 0.3, 1.7], m: 'black' },
    { n: `${pr}_Base`, p: 'cube', x: 0, y: y0 - 0.15, z: 0.85, s: [W, 0.3, 1.7], m: 'black' },
    { n: `${pr}_LatE`, p: 'cube', x: -(W / 2 + 0.15), y: cy, z: 0.85, s: [0.3, H, 1.7], m: 'black' },
    { n: `${pr}_LatD`, p: 'cube', x: W / 2 + 0.15, y: cy, z: 0.85, s: [0.3, H, 1.7], m: 'black' },
    { n: `${pr}_Bat_Topo`, p: 'cube', x: 0, y: yTop + 0.5, z: 1.86, s: [W + 1.5, 0.7, 0.6], m: 'wood' },
    { n: `${pr}_Bat_Base`, p: 'cube', x: 0, y: y0 - 0.5, z: 1.86, s: [W + 1.5, 0.7, 0.6], m: 'wood' },
    { n: `${pr}_Bat_E`, p: 'cube', x: -(W / 2 + 0.35), y: cy, z: 1.86, s: [0.7, H + 0.7, 0.6], m: 'wood' },
    { n: `${pr}_Bat_D`, p: 'cube', x: W / 2 + 0.35, y: cy, z: 1.86, s: [0.7, H + 0.7, 0.6], m: 'wood' },
  ];
}

// Porta entreaberta: dobradiça no batente (lado esquerdo/direito do VÃO) e folha abrindo para +z local.
function porta(pr, { W, lado = 'esq', y, w, h, t = 0.3, swing = 25, z = 2.31 }) {
  const a = (swing * Math.PI) / 180;
  const sinal = lado === 'esq' ? 1 : -1; // esq: dobradiça em x negativo, folha cresce para +x
  const hingeX = sinal * (W / 2 - 0.15);
  return [
    {
      n: `${pr}_Porta`,
      p: 'cube',
      x: hingeX + sinal * (w / 2) * Math.cos(a),
      y,
      z: z + (w / 2) * Math.sin(a),
      s: [w, h, t],
      m: 'wood',
      r: [0, -sinal * swing, 0],
    },
  ];
}

function gaveta(pr, { y, pull, W = 6.4, h = 2.9 }) {
  return [
    { n: `${pr}_Fundo`, p: 'cube', x: 0, y, z: 0.14, s: [W, h, 0.2], m: 'black' },
    { n: `${pr}_Frente`, p: 'cube', x: 0, y, z: pull, s: [W + 0.22, h, 0.35], m: 'wood' },
    { n: `${pr}_LatE`, p: 'cube', x: -(W / 2 - 0.11), y, z: pull / 2 - 0.05, s: [0.35, h - 0.5, pull + 0.15], m: 'wood' },
    { n: `${pr}_LatD`, p: 'cube', x: W / 2 - 0.11, y, z: pull / 2 - 0.05, s: [0.35, h - 0.5, pull + 0.15], m: 'wood' },
    { n: `${pr}_FundoGaveta`, p: 'cube', x: 0, y: y - h / 2 - 0.05, z: pull / 2, s: [W - 0.4, 0.22, pull], m: 'wood' },
    { n: `${pr}_TrilhoE`, p: 'cube', x: -(W / 2 - 0.28), y: y - h / 2 + 0.16, z: pull / 2 - 0.08, s: [0.2, 0.28, pull + 0.4], m: 'iron' },
    { n: `${pr}_TrilhoD`, p: 'cube', x: W / 2 - 0.28, y: y - h / 2 + 0.16, z: pull / 2 - 0.08, s: [0.2, 0.28, pull + 0.4], m: 'iron' },
    { n: `${pr}_Puxador`, p: 'cube', x: 0, y: y + h * 0.42, z: pull + 0.23, s: [2.4, 0.22, 0.22], m: 'iron' },
  ];
}

function armario(pr, { W, y0, yTop, bays = 2 }) {
  const H = yTop - y0;
  const cy = (y0 + yTop) / 2;
  const out = [
    { n: `${pr}_Fundo`, p: 'cube', x: 0, y: cy, z: 0.14, s: [W, H, 0.2], m: 'black' },
    { n: `${pr}_Topo`, p: 'cube', x: 0, y: yTop + 0.15, z: 1.15, s: [W, 0.3, 2.1], m: 'black' },
    { n: `${pr}_Base`, p: 'cube', x: 0, y: y0 - 0.15, z: 1.15, s: [W, 0.3, 2.1], m: 'black' },
    { n: `${pr}_LatE`, p: 'cube', x: -(W / 2 + 0.15), y: cy, z: 1.15, s: [0.3, H, 2.1], m: 'black' },
    { n: `${pr}_LatD`, p: 'cube', x: W / 2 + 0.15, y: cy, z: 1.15, s: [0.3, H, 2.1], m: 'black' },
    { n: `${pr}_Bat_Topo`, p: 'cube', x: 0, y: yTop + 0.5, z: 1.6, s: [W + 1.4, 0.7, 0.6], m: 'wood' },
    { n: `${pr}_Bat_Base`, p: 'cube', x: 0, y: y0 - 0.5, z: 1.6, s: [W + 1.4, 0.7, 0.6], m: 'wood' },
    { n: `${pr}_Bat_E`, p: 'cube', x: -(W / 2 + 0.35), y: cy, z: 1.6, s: [0.7, H + 0.7, 0.6], m: 'wood' },
    { n: `${pr}_Bat_D`, p: 'cube', x: W / 2 + 0.35, y: cy, z: 1.6, s: [0.7, H + 0.7, 0.6], m: 'wood' },
  ];
  if (bays > 1) {
    out.push({ n: `${pr}_Montante`, p: 'cube', x: 0, y: cy, z: 1.15, s: [0.5, H, 2.1], m: 'wood' });
  }
  return out;
}

// ---------- execução ----------
async function grupo(nome, pos, yaw) {
  const g = await unity.command('gameobject create', { name: nome, parent: { hierarchyPath: RAIZ } });
  await unity.command('gameobject set_transform', {
    target: { hierarchyPath: g.hierarchyPath },
    position: J(pos),
    rotation: J([0, yaw, 0]),
  });
  return g.hierarchyPath;
}

async function peca(parent, part) {
  const go = await unity.command('gameobject create', { name: part.n, primitive: part.p, parent: { hierarchyPath: parent } });
  await unity.command('gameobject set_transform', {
    target: { hierarchyPath: go.hierarchyPath },
    position: J([part.x, part.y, part.z]),
    scale: J(part.s),
    ...(part.r ? { rotation: J(part.r) } : {}),
  });
  await unity.command('component set_field', {
    target: { hierarchyPath: go.hierarchyPath },
    component: 'MeshRenderer',
    field: 'm_Materials',
    value: J([{ path: MAT[part.m] }]),
  });
  return go.hierarchyPath;
}

async function luz(parent, { x, y, z, cor, i = 0.4, r = 3 }) {
  const go = await unity.command('gameobject create', { name: 'Luz', parent: { hierarchyPath: parent } });
  await unity.command('gameobject set_transform', { target: { hierarchyPath: go.hierarchyPath }, position: J([x, y, z]) });
  await unity.command('component add', { target: { hierarchyPath: go.hierarchyPath }, type: 'Light' });
  const set = (field, value) =>
    unity.command('component set_field', {
      target: { hierarchyPath: go.hierarchyPath },
      component: 'Light',
      field,
      value: typeof value === 'object' ? J(value) : value,
    });
  await set('m_Type', 2);
  await set('m_Color', { r: cor[0], g: cor[1], b: cor[2], a: 1 });
  await set('m_Intensity', i);
  await set('m_Range', r);
  await set('m_Shadows', { m_Type: 0 });
  return go.hierarchyPath;
}

// ---------- especificações ----------
const UNIDADES = [
  {
    // 0 — balcão norte x = 5: módulo aberto (sem porta), bioma úmido começando
    nome: 'V1_NichoAberto_Norte_x5',
    pos: [-5.33, 0, -32.56],
    yaw: 0,
    partes: [
      ...nicho('V1', { W: 6.4, H: 6.6, y0: 3.4, yTop: 10.0 }),
      { n: 'V1_Prateleira', p: 'cube', x: 0, y: 6.4, z: 1.11, s: [6.0, 0.3, 1.3], m: 'wood' },
      { n: 'V1_Caixote', p: 'cube', x: -1.9, y: 4.0, z: 1.0, s: [1.8, 1.2, 1.4], m: 'wood', r: [0, 12, 0] },
      { n: 'V1_Lata', p: 'cylinder', x: 1.5, y: 4.35, z: 0.95, s: [0.8, 0.65, 0.8], m: 'iron' },
      { n: 'V1_Novelo', p: 'sphere', x: -2.7, y: 3.95, z: 0.95, s: [0.6, 0.6, 0.6], m: 'wood' },
      { n: 'V1_Rolha', p: 'cylinder', x: 0.6, y: 4.1, z: 1.3, s: [0.55, 0.42, 0.55], m: 'wood', r: [0, 0, 6] },
      { n: 'V1_Tampinha', p: 'cylinder', x: 0.9, y: 6.58, z: 1.1, s: [0.72, 0.05, 0.72], m: 'iron' },
    ],
    luz: { x: -0.2, y: 9.3, z: 1.1, cor: [0.55, 0.85, 0.55], i: 0.35 },
  },
  {
    // 1 — balcão norte x = 25: gaveta puxada + gaveta caída encostada no piso
    nome: 'V2_GavetaDupla_Norte_x25',
    pos: [24.67, 0, -30.56],
    yaw: 0,
    partes: [
      ...gaveta('V2A', { y: 9.6, pull: 3.4, W: 6.0, h: 2.4 }),
      ...gaveta('V2B', { y: 5.4, pull: 4.6, W: 6.0, h: 2.4 }),
      { n: 'V2_GavetaCaida', p: 'cube', x: 2.2, y: 1.35, z: 1.5, s: [6.2, 2.5, 0.35], m: 'wood', r: [0, 14, 76] },
      { n: 'V2_GavetaCaida_Lat', p: 'cube', x: 4.9, y: 1.2, z: 1.6, s: [0.35, 2.1, 1.6], m: 'wood', r: [0, 14, 76] },
      { n: 'V2_Tampinha_A', p: 'cylinder', x: -1.4, y: 6.68, z: 3.5, s: [0.7, 0.06, 0.7], m: 'iron' },
      { n: 'V2_Tampinha_B', p: 'cylinder', x: -1.5, y: 6.8, z: 3.4, s: [0.7, 0.06, 0.7], m: 'iron' },
      { n: 'V2_Palito', p: 'cylinder', x: 0.6, y: 6.65, z: 3.2, s: [0.07, 0.75, 0.07], m: 'wood', r: [90, 20, 0] },
    ],
    luz: null,
  },
  {
    // 2 — balcão leste z = 15: gaveta puxada 40% (feira), tampinhas como moeda
    nome: 'V3_GavetaFeira_Leste_z15',
    pos: [31.9, 0, 15],
    yaw: -90,
    partes: [
      ...gaveta('V3', { y: 6.6, pull: 4.8, W: 6.4, h: 3.0 }),
      { n: 'V3_Tampinha_A', p: 'cylinder', x: -2.4, y: 5.28, z: 2.6, s: [0.72, 0.05, 0.72], m: 'iron' },
      { n: 'V3_Tampinha_B', p: 'cylinder', x: -2.2, y: 5.38, z: 2.5, s: [0.72, 0.05, 0.72], m: 'iron' },
      { n: 'V3_Tampinha_C', p: 'cylinder', x: -2.5, y: 5.48, z: 2.7, s: [0.72, 0.05, 0.72], m: 'quartz' },
      { n: 'V3_Novelo', p: 'sphere', x: 0.4, y: 5.55, z: 3.0, s: [0.62, 0.62, 0.62], m: 'wood' },
      { n: 'V3_Botao', p: 'cylinder', x: 1.9, y: 5.27, z: 3.4, s: [0.42, 0.04, 0.42], m: 'quartz' },
      { n: 'V3_Clipe', p: 'cube', x: 1.4, y: 5.27, z: 2.2, s: [0.7, 0.07, 0.07], m: 'iron', r: [0, 28, 0] },
    ],
    luz: null,
  },
  {
    // 3 — balcão leste z = -25 (ao lado das pias): porta arrancada, ambiente úmido
    nome: 'V4_PortaArrancada_Leste_z-25',
    pos: [31.9, 0, -26],
    yaw: -90,
    partes: [
      ...nicho('V4', { W: 6.4, H: 6.6, y0: 3.4, yTop: 10.0 }),
      { n: 'V4_Prateleira', p: 'cube', x: 0, y: 6.4, z: 1.11, s: [6.0, 0.3, 1.3], m: 'wood' },
      ...porta('V4', { W: 6.4, lado: 'esq', y: 6.7, w: 3.0, h: 6.4, swing: 24 }),
      { n: 'V4_PortaArrancada', p: 'cube', x: 3.4, y: 3.4, z: 1.9, s: [2.9, 6.4, 0.3], m: 'wood', r: [0, 20, 12] },
      { n: 'V4_Pote', p: 'cylinder', x: -1.8, y: 4.8, z: 1.0, s: [1.2, 1.0, 1.2], m: 'quartz' },
      { n: 'V4_Pote_Tampa', p: 'cylinder', x: -1.8, y: 5.84, z: 1.0, s: [1.28, 0.05, 1.28], m: 'iron' },
      { n: 'V4_Lata', p: 'cylinder', x: -0.2, y: 4.35, z: 1.1, s: [0.8, 0.65, 0.8], m: 'iron' },
      { n: 'V4_Palito', p: 'cylinder', x: 1.5, y: 3.76, z: 1.3, s: [0.08, 0.8, 0.08], m: 'wood', r: [90, 0, 10] },
    ],
    luz: { x: 0.4, y: 9.2, z: 1.2, cor: [0.6, 0.9, 0.85], i: 0.35 },
  },
  {
    // 4 — balcão oeste z = -15: nicho com musgo (luz esverdeada, resto escuro)
    nome: 'V5_NichoUmido_Oeste_z-15',
    pos: [-32.56, 0, -15],
    yaw: 90,
    partes: [
      ...nicho('V5', { W: 6.4, H: 6.6, y0: 3.4, yTop: 10.0 }),
      { n: 'V5_Prateleira', p: 'cube', x: 0, y: 6.4, z: 1.11, s: [6.0, 0.3, 1.3], m: 'wood' },
      { n: 'V5_CaixaFosforo', p: 'cube', x: -1.6, y: 4.0, z: 1.0, s: [1.9, 0.62, 1.25], m: 'wood', r: [0, -10, 0] },
      { n: 'V5_CaixaFosforo_Rotulo', p: 'cube', x: -1.6, y: 4.33, z: 1.0, s: [1.5, 0.06, 0.95], m: 'iron', r: [0, -10, 0] },
      { n: 'V5_Lata', p: 'cylinder', x: 1.4, y: 4.35, z: 1.1, s: [0.8, 0.65, 0.8], m: 'iron' },
      { n: 'V5_Novelo', p: 'sphere', x: 0.2, y: 3.95, z: 0.9, s: [0.6, 0.6, 0.6], m: 'wood' },
      { n: 'V5_Palito', p: 'cylinder', x: -0.9, y: 3.76, z: 1.4, s: [0.08, 0.8, 0.08], m: 'wood', r: [90, 0, 8] },
    ],
    luz: { x: -0.3, y: 9.0, z: 1.0, cor: [0.5, 0.8, 0.5], i: 0.3 },
  },
  {
    // 5 — balcão oeste z = 25: combo porta entreaberta (embaixo) + gaveta (em cima)
    nome: 'V6_PortaGaveta_Oeste_z25',
    pos: [-32.56, 0, 25],
    yaw: 90,
    partes: [
      ...nicho('V6', { W: 6.4, H: 3.4, y0: 3.4, yTop: 6.8 }),
      ...porta('V6', { W: 6.4, lado: 'esq', y: 5.1, w: 3.0, h: 3.2, swing: 26 }),
      { n: 'V6_Prateleira', p: 'cube', x: 0, y: 5.0, z: 1.05, s: [6.0, 0.25, 1.2], m: 'wood' },
      ...gaveta('V6G', { y: 9.1, pull: 3.6, W: 6.2, h: 2.5 }),
      { n: 'V6_Lata', p: 'cylinder', x: -1.9, y: 3.85, z: 1.0, s: [0.75, 0.55, 0.75], m: 'iron' },
      { n: 'V6_Rolha', p: 'cylinder', x: 1.2, y: 3.8, z: 1.1, s: [0.55, 0.4, 0.55], m: 'wood', r: [0, 0, 8] },
      { n: 'V6_Tampinha', p: 'cylinder', x: 0.2, y: 5.15, z: 1.6, s: [0.7, 0.05, 0.7], m: 'iron' },
      { n: 'V6_Botao', p: 'cylinder', x: -2.0, y: 9.15, z: 2.0, s: [0.42, 0.04, 0.42], m: 'quartz' },
    ],
    luz: { x: 0, y: 9.9, z: 1.4, cor: [1, 0.82, 0.58], i: 0.4 },
  },
  {
    // 6 — armário canto NE (32 / 27 / -37), face z = -27: depósito com porta entreaberta
    nome: 'A2_Deposito_NE',
    pos: [32, 0, -27],
    yaw: 0,
    partes: [
      ...armario('A2', { W: 9.6, y0: 23.4, yTop: 30.4, bays: 2 }),
      { n: 'A2_Prateleira_E', p: 'cube', x: -2.4, y: 26.6, z: 1.3, s: [3.9, 0.3, 1.6], m: 'wood' },
      { n: 'A2_Prateleira_D', p: 'cube', x: 2.4, y: 28.2, z: 1.3, s: [3.9, 0.3, 1.6], m: 'wood' },
      ...porta('A2', { W: 9.6, lado: 'esq', y: 26.9, w: 4.0, h: 6.8, t: 0.35, swing: 24, z: 1.95 }),
      { n: 'A2_PortaSolta', p: 'cube', x: 2.9, y: 26.6, z: 1.5, s: [3.9, 6.8, 0.35], m: 'wood', r: [0, 8, 72] },
      { n: 'A2_CaixaFosforo', p: 'cube', x: -2.3, y: 27.0, z: 1.2, s: [1.9, 0.62, 1.25], m: 'wood', r: [0, -14, 0] },
      { n: 'A2_Lata', p: 'cylinder', x: -2.6, y: 24.0, z: 1.1, s: [0.8, 0.65, 0.8], m: 'iron' },
      { n: 'A2_Carretel', p: 'cylinder', x: 2.6, y: 28.5, z: 1.2, s: [0.85, 0.13, 0.85], m: 'wood' },
    ],
    luz: { x: -2.4, y: 29.7, z: 1.4, cor: [1, 0.82, 0.58], i: 0.4 },
  },
  {
    // 7 — armário oeste (-37,25 / 27 / 24), face x = -32,25: vão aberto + 1 porta larga
    nome: 'A3_Aberto_Oeste_z24',
    pos: [-32.25, 0, 24],
    yaw: 90,
    partes: [
      ...armario('A3', { W: 9.6, y0: 23.4, yTop: 30.4, bays: 2 }),
      { n: 'A3_Prateleira_E', p: 'cube', x: -2.4, y: 26.4, z: 1.3, s: [3.9, 0.3, 1.6], m: 'wood' },
      { n: 'A3_Prateleira_D', p: 'cube', x: 2.4, y: 27.6, z: 1.3, s: [3.9, 0.3, 1.6], m: 'wood' },
      ...porta('A3', { W: 9.6, lado: 'esq', y: 26.9, w: 3.9, h: 6.8, t: 0.35, swing: 22, z: 1.95 }),
      { n: 'A3_Caixote', p: 'cube', x: 2.3, y: 24.0, z: 1.2, s: [1.8, 1.2, 1.4], m: 'wood', r: [0, 10, 0] },
      { n: 'A3_Novelo', p: 'sphere', x: 1.2, y: 23.7, z: 1.0, s: [0.6, 0.6, 0.6], m: 'wood' },
      { n: 'A3_Lata', p: 'cylinder', x: 3.6, y: 24.05, z: 1.0, s: [0.8, 0.65, 0.8], m: 'iron' },
      { n: 'A3_Palito', p: 'cylinder', x: 0.4, y: 23.9, z: 1.6, s: [0.07, 0.8, 0.07], m: 'wood', r: [0, 0, 72] },
    ],
    luz: { x: 2.4, y: 26.8, z: 1.3, cor: [0.55, 0.85, 0.55], i: 0.3 },
  },
  {
    // 8 — armário leste (37 / 27 / 3), face x = 32: duas portas entreabertas (70 de comprimento)
    nome: 'A4_DuploPorta_Leste',
    pos: [32, 0, 3],
    yaw: -90,
    partes: [
      ...armario('A4', { W: 9.6, y0: 23.4, yTop: 30.4, bays: 2 }),
      { n: 'A4_Prateleira_E', p: 'cube', x: -2.4, y: 26.8, z: 1.3, s: [3.9, 0.3, 1.6], m: 'wood' },
      { n: 'A4_Prateleira_D', p: 'cube', x: 2.4, y: 27.4, z: 1.3, s: [3.9, 0.3, 1.6], m: 'wood' },
      ...porta('A4A', { W: 9.6, lado: 'esq', y: 26.9, w: 3.9, h: 6.8, t: 0.35, swing: 20, z: 1.95 }),
      ...porta('A4B', { W: 9.6, lado: 'dir', y: 26.9, w: 3.9, h: 6.8, t: 0.35, swing: 30, z: 1.95 }),
      { n: 'A4_Pote', p: 'cylinder', x: -2.4, y: 28.1, z: 1.2, s: [1.2, 1.0, 1.2], m: 'quartz' },
      { n: 'A4_Pote_Tampa', p: 'cylinder', x: -2.4, y: 29.14, z: 1.2, s: [1.28, 0.05, 1.28], m: 'iron' },
      { n: 'A4_Lata', p: 'cylinder', x: 2.2, y: 24.05, z: 1.0, s: [0.8, 0.65, 0.8], m: 'iron' },
      { n: 'A4_Tampinha', p: 'cylinder', x: 3.5, y: 23.46, z: 1.3, s: [0.7, 0.05, 0.7], m: 'iron' },
    ],
    luz: null,
  },
  {
    // 9 — armário oeste (-37,25 / 27 / -17), face x = -32,25: prateleiras + porta entreaberta
    nome: 'A5_Prateleiras_Oeste_z-17',
    pos: [-32.25, 0, -17],
    yaw: 90,
    partes: [
      ...armario('A5', { W: 9.6, y0: 23.4, yTop: 30.4, bays: 2 }),
      { n: 'A5_Prateleira_E', p: 'cube', x: -2.4, y: 25.4, z: 1.3, s: [3.9, 0.3, 1.6], m: 'wood' },
      { n: 'A5_Prateleira_D', p: 'cube', x: 2.4, y: 25.8, z: 1.3, s: [3.9, 0.3, 1.6], m: 'wood' },
      { n: 'A5_Prateleira_D2', p: 'cube', x: 2.4, y: 28.4, z: 1.3, s: [3.9, 0.3, 1.6], m: 'wood' },
      ...porta('A5', { W: 9.6, lado: 'esq', y: 26.9, w: 3.9, h: 6.8, t: 0.35, swing: 27, z: 1.95 }),
      { n: 'A5_CaixaFosforo', p: 'cube', x: -2.2, y: 25.8, z: 1.2, s: [1.9, 0.62, 1.25], m: 'wood', r: [0, -8, 0] },
      { n: 'A5_Carretel', p: 'cylinder', x: 2.6, y: 28.7, z: 1.2, s: [0.85, 0.13, 0.85], m: 'wood' },
      { n: 'A5_Lata', p: 'cylinder', x: 1.6, y: 24.05, z: 1.0, s: [0.8, 0.65, 0.8], m: 'iron' },
      { n: 'A5_Novelo', p: 'sphere', x: 3.4, y: 23.7, z: 1.0, s: [0.6, 0.6, 0.6], m: 'wood' },
    ],
    luz: { x: -2.4, y: 29.5, z: 1.4, cor: [1, 0.82, 0.58], i: 0.4 },
  },
];

// ---------- build ----------
const feitos = [];
for (let i = INICIO; i < Math.min(FIM, UNIDADES.length); i++) {
  const u = UNIDADES[i];
  const g = await grupo(u.nome, u.pos, u.yaw);
  for (const part of u.partes) await peca(g, part);
  if (u.luz) await luz(g, u.luz);
  feitos.push({ nome: u.nome, partes: u.partes.length, luz: Boolean(u.luz) });
}

return { construidos: feitos };
