import fs from 'node:fs';

const lines = fs.readFileSync('Assets/Scenes/SampleScene.unity', 'utf8').split(/\r?\n/);
const docs = [];
let cur = null;
for (const line of lines) {
  const m = /^--- !u!(\d+) &(\d+)/.exec(line);
  if (m) { if (cur) docs.push(cur); cur = { classId: +m[1], fileId: m[2], lines: [] }; }
  else if (cur) cur.lines.push(line);
}
if (cur) docs.push(cur);
const vec = (l) => { const m = /x:\s*(-?[\d.eE+-]+),\s*y:\s*(-?[\d.eE+-]+),\s*z:\s*(-?[\d.eE+-]+)/.exec(l || ''); return m ? [+m[1], +m[2], +m[3]] : null; };
const gos = new Map(), trs = new Map(), pb = new Map();
for (const d of docs) {
  if (d.classId === 1) { let n = '?'; for (const l of d.lines) if (l.startsWith('  m_Name:')) n = l.slice(9).trim(); gos.set(d.fileId, n); }
  else if (d.classId === 4) {
    let go = null, father = '0', pos = [0, 0, 0], scale = [1, 1, 1], rot = [0, 0, 0, 1], children = [];
    let inKids = false;
    for (const l of d.lines) {
      let m;
      if ((m = /m_GameObject:\s*\{fileID:\s*(\d+)\}/.exec(l))) go = m[1];
      if ((m = /m_Father:\s*\{fileID:\s*(\d+)\}/.exec(l))) father = m[1];
      if (l.trimStart().startsWith('m_LocalPosition:')) pos = vec(l) || pos;
      if (l.trimStart().startsWith('m_LocalScale:')) scale = vec(l) || scale;
      if (l.trimStart().startsWith('m_LocalRotation:')) { const q = /x:\s*(-?[\d.eE+-]+),\s*y:\s*(-?[\d.eE+-]+),\s*z:\s*(-?[\d.eE+-]+),\s*w:\s*(-?[\d.eE+-]+)/.exec(l); if (q) rot = [+q[1], +q[2], +q[3], +q[4]]; }
      if (l.trimStart().startsWith('m_Children:')) { inKids = true; continue; }
      if (inKids) { const k = /-\s*\{fileID:\s*(\d+)\}/.exec(l); if (k) children.push(k[1]); else if (children.length && !l.trimStart().startsWith('-')) inKids = false; }
    }
    if (go) trs.set(go, { fileId: d.fileId, father, pos, scale, rot, children });
  } else if (d.classId === 114 && d.lines.some(l => l.includes('ProBuilder.ProBuilderMesh'))) {
    let go = null; const verts = []; let inPos = false;
    for (const l of d.lines) {
      let m;
      if ((m = /m_GameObject:\s*\{fileID:\s*(\d+)\}/.exec(l))) go = m[1];
      if (l.trimStart().startsWith('m_Positions:')) { inPos = true; continue; }
      if (inPos) { if (l.trimStart().startsWith('- {x:')) { const v = vec(l); if (v) verts.push(v); } else if (verts.length && !l.trimStart().startsWith('-')) inPos = false; }
    }
    if (go) pb.set(go, verts);
  }
}
const tf2go = new Map(); for (const [go, t] of trs) tf2go.set(t.fileId, go);
const qmul = (a, b) => [
  a[3] * b[0] + a[0] * b[3] + a[1] * b[2] - a[2] * b[1],
  a[3] * b[1] - a[0] * b[2] + a[1] * b[3] + a[2] * b[0],
  a[3] * b[2] + a[0] * b[1] - a[1] * b[0] + a[2] * b[3],
  a[3] * b[3] - a[0] * b[0] - a[1] * b[1] - a[2] * b[2],
];
const qrot = (q, v) => {
  const [x, y, z, w] = q;
  const ix = w * v[0] + y * v[2] - z * v[1], iy = w * v[1] + z * v[0] - x * v[2], iz = w * v[2] + x * v[1] - y * v[0], iw = -x * v[0] - y * v[1] - z * v[2];
  return [ix * w + iw * -x + iy * -z - iz * -y, iy * w + iw * -y + iz * -x - ix * -z, iz * w + iw * -z + ix * -y - iy * -x];
};
const worldOf = (go) => {
  let t = trs.get(go), pos = [0, 0, 0], rot = [0, 0, 0, 1], scale = [1, 1, 1];
  const chain = [];
  let guard = 0;
  while (t && guard++ < 64) { chain.push(t); if (!t.father || t.father === '0') break; t = trs.get(tf2go.get(t.father)); }
  for (let i = chain.length - 1; i >= 0; i--) {
    const c = chain[i];
    const sp = [pos[0] * c.scale[0], pos[1] * c.scale[1], pos[2] * c.scale[2]];
    const rp = qrot(c.rot, sp);
    pos = [c.pos[0] + rp[0], c.pos[1] + rp[1], c.pos[2] + rp[2]];
    rot = qmul(c.rot, rot);
    scale = [scale[0] * c.scale[0], scale[1] * c.scale[1], scale[2] * c.scale[2]];
  }
  return { pos, rot, scale };
};
const findByName = (n) => [...gos.entries()].filter(([id, name]) => name === n).map(([id]) => id);

const target = findByName('Vassoura')[0];
const overrides = { "1033884546": -7.9 };
const nodes = [target, ...[...trs.get(target).children].map(c => tf2go.get(c)).filter(Boolean)];
let min = [1e9, 1e9, 1e9], max = [-1e9, -1e9, -1e9];
for (const go of nodes) {
  const w = worldOf(go);
  const verts = pb.get(go) || [];
  for (const v of verts) {
    const s = [v[0] * w.scale[0], v[1] * w.scale[1], v[2] * w.scale[2]];
    const r = qrot(w.rot, s);
    const p = [w.pos[0] + r[0], w.pos[1] + r[1], w.pos[2] + r[2]];
    for (let k = 0; k < 3; k++) { if (p[k] < min[k]) min[k] = p[k]; if (p[k] > max[k]) max[k] = p[k]; }
  }
  console.log(go, gos.get(go), 'worldPos', w.pos.map(n => n.toFixed(3)).join(','));
}
console.log('GRUPO min', min.map(n => n.toFixed(3)).join(','), 'max', max.map(n => n.toFixed(3)).join(','));
console.log('altura do grupo', (max[1] - min[1]).toFixed(3), '| deslocamento para pousar em y=0:', (-min[1]).toFixed(3));
const vT = trs.get(target);
console.log('Vassoura localPos atual y =', vT.pos[1], '=> novo y =', (vT.pos[1] - min[1]).toFixed(3));
