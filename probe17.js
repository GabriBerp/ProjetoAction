globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {});
  const obj = out || {};
  const flat = {};
  (function dive(o, p){ for (const k of Object.keys(o||{})) { const full = p ? p+'.'+k : k; const v = o[k]; if (typeof v === 'object' && v !== null && !Array.isArray(v)) { dive(v, full); } else { flat[full] = String(v).slice(0,120); } } })(obj);
  return JSON.stringify({ n: Object.keys(flat).length, first120: Object.entries(flat).slice(0,120).map(([k,v])=>k+':'+v) });
};
fn();
