const V=(x,y,z)=>({x,y,z});
const rows=[];
const errors=[];
for (const [id,x,y,z] of rows) {
  try { await unity.command('component set_field',{target:id,component:'Transform',field:'m_LocalPosition',value:V(x,y,z)}); }
  catch(e){ errors.push(id+' '+String(e.message).slice(0,120)); }
}
return { fixed: rows.length, errors };
