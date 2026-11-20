const h = await unity.command('scene get_hierarchy', {});
const coz = h.roots.find(r => r.name === 'COZINHA');
const flat = [];
const walk = (n, parent) => {
  flat.push({ id: n.instanceId, name: n.name, parent, comps: n.components });
  (n.children || []).forEach(c => walk(c, n.instanceId));
};
walk(coz, 0);
const roots = h.roots.map(r => ({ id: r.instanceId, name: r.name }));
return { cozinhaId: coz.instanceId, flat, roots };
