const h = await unity.command('scene get_hierarchy', {});
const items = [];
const walk = (n) => {
  if (/^Item/.test(n.name)) items.push({ id: n.instanceId, name: n.name, comps: n.components });
  (n.children || []).forEach(walk);
};
h.roots.forEach(walk);

const out = [];
for (const it of items) {
  const tr = await unity.command('component get_properties', { target: String(it.id), type: 'Transform' });
  const pos = tr.properties.m_LocalPosition;
  const scl = tr.properties.m_LocalScale;
  const type = it.comps.includes('BoxCollider') ? 'BoxCollider' : 'SphereCollider';
  const c = await unity.command('component get_properties', { target: String(it.id), type });
  const p = c.properties;
  let bottomLocal, topLocal;
  if (type === 'BoxCollider') {
    bottomLocal = p.m_Center[1] - p.m_Size[1] / 2;
    topLocal = p.m_Center[1] + p.m_Size[1] / 2;
  } else {
    bottomLocal = p.m_Center[1] - p.m_Radius;
    topLocal = p.m_Center[1] + p.m_Radius;
  }
  const sy = scl[1];
  out.push({
    id: it.id, name: it.name, type,
    pos, scale: scl,
    bottomLocal, topLocal,
    worldBottom: pos[1] + bottomLocal * sy,
    restY: 11.5 - bottomLocal * sy,
    size: type === 'BoxCollider' ? p.m_Size : [p.m_Radius * 2, p.m_Radius * 2, p.m_Radius * 2],
    center: p.m_Center,
  });
}
return out;
