import fs from 'node:fs';

// ---------- YAML ----------
const lines = fs.readFileSync('Assets/Scenes/SampleScene.unity', 'utf8').split(/\r?\n/);
const docs = [];
let cur = null;
for (const line of lines) {
  const m = /^--- !u!(\d+) &(\d+)/.exec(line);
  if (m) { if (cur) docs.push(cur); cur = { classId: +m[1], fileId: m[2], lines: [] }; }
  else if (cur) cur.lines.push(line);
}
if (cur) docs.push(cur);

const vec = (l) => {
  const m = /x:\s*(-?[\d.eE+-]+),\s*y:\s*(-?[\d.eE+-]+),\s*z:\s*(-?[\d.eE+-]+)/.exec(l || '');
  return m ? [+m[1], +m[2], +m[3]] : null;
};
const gos = new Map(), trs = new Map(), pb = new Map();
for (const d of docs) {
  if (d.classId === 1) {
    let name = '?', comps = [];
    let inComp = false;
    for (const l of d.lines) {
      if (l.startsWith('  m_Name:')) name = l.slice(9).trim();
      if (l.startsWith('  m_Component:')) { inComp = true; continue; }
      if (inComp) {
        const m = /component:\s*\{fileID:\s*(\d+)\}/.exec(l);
        if (m) comps.push(m[1]);
        if (l.startsWith('  m_Layer:')) inComp = false;
      }
    }
    gos.set(d.fileId, { name, comps });
  } else if (d.classId === 4) {
    let go = null, father = '0', pos = [0, 0, 0], scale = [1, 1, 1], rot = [0, 0, 0, 1], children = [];
    let inKids = false;
    for (const l of d.lines) {
      let m;
      if ((m = /m_GameObject:\s*\{fileID:\s*(\d+)\}/.exec(l))) go = m[1];
      if ((m = /m_Father:\s*\{fileID:\s*(\d+)\}/.exec(l))) father = m[1];
      if (l.trimStart().startsWith('m_LocalPosition:')) pos = vec(l) || pos;
      if (l.trimStart().startsWith('m_LocalScale:')) scale = vec(l) || scale;
      if (l.trimStart().startsWith('m_LocalRotation:')) {
        const q = /x:\s*(-?[\d.eE+-]+),\s*y:\s*(-?[\d.eE+-]+),\s*z:\s*(-?[\d.eE+-]+),\s*w:\s*(-?[\d.eE+-]+)/.exec(l);
        if (q) rot = [+q[1], +q[2], +q[3], +q[4]];
      }
      if (l.trimStart().startsWith('m_Children:')) { inKids = true; continue; }
      if (inKids) {
        const k = /-\s*\{fileID:\s*(\d+)\}/.exec(l);
        if (k) children.push(k[1]);
        else if (children.length && !l.trimStart().startsWith('-')) inKids = false;
      }
    }
    if (go) trs.set(go, { fileId: d.fileId, father, pos, scale, rot, children });
  } else if (d.classId === 114 && d.lines.some(l => l.includes('ProBuilder.ProBuilderMesh'))) {
    let go = null; const verts = []; let inPos = false;
    for (const l of d.lines) {
      let m;
      if ((m = /m_GameObject:\s*\{fileID:\s*(\d+)\}/.exec(l))) go = m[1];
      if (l.trimStart().startsWith('m_Positions:')) { inPos = true; continue; }
      if (inPos) {
        if (l.trimStart().startsWith('- {x:')) { const v = vec(l); if (v) verts.push(v); }
        else if (verts.length && !l.trimStart().startsWith('-')) inPos = false;
      }
    }
    if (go) pb.set(go, verts);
  }
}
const qrot = (q, v) => {
  const [x, y, z, w] = q;
  const ix = w * v[0] + y * v[2] - z * v[1], iy = w * v[1] + z * v[0] - x * v[2], iz = w * v[2] + x * v[1] - y * v[0], iw = -x * v[0] - y * v[1] - z * v[2];
  return [ix * w + iw * -x + iy * -z - iz * -y, iy * w + iw * -y + iz * -x - ix * -z, iz * w + iw * -z + ix * -y - iy * -x];
};
// transform fileId -> gameObject fileId
const tf2go = new Map();
for (const [go, t] of trs) tf2go.set(t.fileId, go);
// build yaml tree: root transform = father 0
const yRoots = [...trs.values()].filter(t => t.father === '0').map(t => tf2go.get(t.fileId));
const yChildren = (go) => (trs.get(go)?.children || []).map(c => tf2go.get(c)).filter(Boolean);
const decodeName = (s) => {
  if (!s.startsWith('"')) return s;
  return s.slice(1, -1).replace(/\\x([0-9A-Fa-f]{2})/g, (_, h) => String.fromCharCode(parseInt(h, 16)))
    .replace(/\\u([0-9A-Fa-f]{4})/g, (_, h) => String.fromCharCode(parseInt(h, 16)));
};
const yName = (go) => decodeName(gos.get(go)?.name || '?');
const yInfo = (go) => {
  const t = trs.get(go), verts = pb.get(go) || [];
  let min = [Infinity, Infinity, Infinity], max = [-Infinity, -Infinity, -Infinity];
  for (const v of verts) {
    const w = qrot(t.rot, [v[0] * t.scale[0], v[1] * t.scale[1], v[2] * t.scale[2]]);
    for (let k = 0; k < 3; k++) { if (w[k] < min[k]) min[k] = w[k]; if (w[k] > max[k]) max[k] = w[k]; }
  }
  return { go, name: yName(go), pos: t.pos, rot: t.rot, scale: t.scale, nverts: verts.length, min: verts.length ? min : null, max: verts.length ? max : null };
};

// ---------- editor dump ----------
const dump = JSON.parse(fs.readFileSync('tmp_unity.json', 'utf8')).result;
const byParent = new Map();
for (const o of dump.flat) {
  if (!byParent.has(o.parent)) byParent.set(o.parent, []);
  byParent.get(o.parent).push(o);
}
const cozId = dump.cozinhaId;

// ---------- lockstep match ----------
const matched = [];
let mismatches = [];
const walkMatch = (yGo, eNode, path) => {
  matched.push({ id: eNode.id, name: eNode.name, comps: eNode.comps, parent: eNode.parent, path, y: yInfo(yGo) });
  const yk = yChildren(yGo);
  const ek = byParent.get(eNode.id) || [];
  if (yk.length !== ek.length) {
    mismatches.push(`${path}/${eNode.name}: yaml ${yk.length} vs editor ${ek.length}`);
    return;
  }
  for (let i = 0; i < yk.length; i++) {
    if (yName(yk[i]) !== ek[i].name) {
      mismatches.push(`${path}/${eNode.name}[${i}]: yaml '${yName(yk[i])}' vs editor '${ek[i].name}'`);
      return;
    }
    walkMatch(yk[i], ek[i], path + '/' + eNode.name);
  }
};
const yCoz = yRoots.find(r => yName(r) === 'COZINHA');
walkMatch(yCoz, { id: cozId, name: 'COZINHA', parent: 0, comps: ['Transform'] }, '');

const f = (n) => (n === null || n === undefined ? '-' : (+n).toFixed(3));
const q2e = (q) => {
  const [x, y, z, w] = q;
  const ex = Math.atan2(2 * (w * x + y * z), 1 - 2 * (x * x + y * y));
  let t2 = 2 * (w * y - z * x); t2 = Math.max(-1, Math.min(1, t2));
  const ey = Math.asin(t2);
  const ez = Math.atan2(2 * (w * z + x * y), 1 - 2 * (y * y + z * z));
  return [ex, ey, ez].map(v => +(v * 180 / Math.PI).toFixed(2));
};
const depth = (p) => (p.match(/\//g) || []).length;

fs.writeFileSync('tmp_tree.json', JSON.stringify({ matched, mismatches, roots: dump.roots }, null, 1));
console.log('mismatches:', mismatches.length ? mismatches : 'none');
console.log('matched objects:', matched.length);
console.log('\n=== TREE (depth | id | name | localPos | scale | euler | verts | localSize) ===');
for (const m of matched) {
  const y = m.y;
  console.log([
    depth(m.path), m.id, m.name,
    `(${y.pos.map(f).join(',')})`,
    `(${y.scale.map(f).join(',')})`,
    `(${q2e(y.rot).join(',')})`,
    y.nverts,
    y.min ? `(${f(y.max[0] - y.min[0])},${f(y.max[1] - y.min[1])},${f(y.max[2] - y.min[2])})` : '-',
  ].join(' | '));
}

// ---------- items (roots) ----------
console.log('\n=== ROOT ITEMS ===');
for (const r of dump.roots.filter(r => /^Item/.test(r.name))) {
  const y = yRoots.map(yInfo).find(y => y.name === r.name);
  console.log([r.id, r.name, y ? `pos(${y.pos.map(f).join(',')})` : 'no-yaml', y ? `scale(${y.scale.map(f).join(',')})` : '', y ? `minYlocal ${y.min ? f(y.min[1]) : '-'}` : ''].join(' | '));
}


