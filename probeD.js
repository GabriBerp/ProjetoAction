globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const obj = resp && typeof resp === 'object' && !Array.isArray(resp) ? resp : (Array.isArray(resp) ? { arr: resp } : {});
  const keys = Object.keys(obj).length ? Object.keys(obj) : ['n/a'];
  const out = {};
  keys.slice(0, 40).forEach(k => {
    let v = obj[k];
    if (v && typeof v === 'object' && !Array.isArray(v)) v = JSON.stringify(v).slice(0, 800);
    else v = String(v).slice(0, 800);
    out[k] = v;
  });
  return JSON.stringify({ keys, out, totalKeys: keys.length });
};
fn();
