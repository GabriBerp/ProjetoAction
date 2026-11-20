import fs from 'node:fs';
const t = JSON.parse(fs.readFileSync('tmp_tree.json', 'utf8'));
const cozY = 7.517, fridgeY = 5.483;
const gelo = t.matched.filter(m => m.name === 'Gelo');
const rows = [];
for (const g of gelo) {
  const bottom = cozY + fridgeY + g.y.pos[1] + g.y.min[1];
  if (bottom < 0) {
    const needed = 0.15 - cozY - fridgeY - g.y.min[1];
    rows.push([g.id, g.y.pos[0], needed.toFixed(4), g.y.pos[2], bottom.toFixed(3), g.y.pos[1].toFixed(3)]);
  }
}
console.log('gelo abaixo do piso:', rows.length);
for (const r of rows) console.log(r.join('\t'));
const js = `const V=(x,y,z)=>({x,y,z});
const rows=${JSON.stringify(rows.map(r => [r[0], +r[1], +r[2], +r[3]]))};
const errors=[];
for (const [id,x,y,z] of rows) {
  try { await unity.command('component set_field',{target:id,component:'Transform',field:'m_LocalPosition',value:V(x,y,z)}); }
  catch(e){ errors.push(id+' '+String(e.message).slice(0,120)); }
}
return { fixed: rows.length, errors };
`;
fs.writeFileSync('tmp_fix5.mjs', js);
