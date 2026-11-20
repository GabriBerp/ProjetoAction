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
const gos = new Map(), trByGo = new Map(), colByGo = new Map();
for (const d of docs) {
  if (d.classId === 1) { let n = '?'; for (const l of d.lines) if (l.startsWith('  m_Name:')) n = l.slice(9).trim(); gos.set(d.fileId, n); }
  else if (d.classId === 4) {
    let go = null, father = '0', pos = [0, 0, 0], scale = [1, 1, 1];
    for (const l of d.lines) {
      const m = /m_GameObject:\s*\{fileID:\s*(\d+)\}/.exec(l); if (m) go = m[1];
      const f = /m_Father:\s*\{fileID:\s*(\d+)\}/.exec(l); if (f) father = f[1];
      if (l.trimStart().startsWith('m_LocalPosition:')) pos = vec(l) || pos;
      if (l.trimStart().startsWith('m_LocalScale:')) scale = vec(l) || scale;
    }
    if (go) trByGo.set(go, { pos, scale, father });
  } else if (d.classId === 65 || d.classId === 135) {
    let go = null, center = [0, 0, 0], size = null, radius = null;
    for (const l of d.lines) {
      const m = /m_GameObject:\s*\{fileID:\s*(\d+)\}/.exec(l); if (m) go = m[1];
      if (l.trimStart().startsWith('m_Center:')) center = vec(l) || center;
      if (l.trimStart().startsWith('m_Size:')) size = vec(l) || size;
      if (l.trimStart().startsWith('m_Radius:')) radius = parseFloat(l.split(':')[1]);
    }
    if (go) colByGo.set(go, d.classId === 65 ? { type: 'Box', center, size } : { type: 'Sphere', center, radius });
  }
}
const rows = [];
for (const [go, name] of gos) {
  if (!/^Item/.test(name)) continue;
  const t = trByGo.get(go), c = colByGo.get(go);
  if (!t || !c) { rows.push({ name, note: 'sem transform/collider' }); continue; }
  const sy = t.scale[1];
  const half = c.type === 'Box' ? c.size[1] / 2 : c.radius;
  const bottomLocal = c.center[1] - half;
  const worldBottom = t.pos[1] + bottomLocal * sy;
  rows.push({ go, name, type: c.type, posY: t.pos[1], sy, bottomLocal, worldBottom, restY: 11.5 - bottomLocal * sy });
}
rows.sort((a, b) => a.name.localeCompare(b.name));
for (const r of rows) {
  if (r.note) { console.log(r.name, r.note); continue; }
  console.log(`${r.go}\t${r.name}\t${r.type}\ty=${r.posY.toFixed(3)}\tscaleY=${r.sy}\tbottom=${r.worldBottom.toFixed(3)}\tnovoY=${r.restY.toFixed(3)}`);
}
