globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const out = resp && resp.data ? resp.data : resp;
  const keys = Object.keys(out || {});
  const obj = {};
  for (let i = 0; i < keys.length; i++) {
    const k = keys[i];
    let v = out[k];
    if (v && typeof v === 'object' && !Array.isArray(v)) v = JSON.stringify(v).slice(0, 1000);
    else if (v && typeof v === 'string') v = v.slice(0, 1000);
    else v = String(v);
    obj[k] = v;
  }
  const entries = Object.entries(obj).slice(0, 60);
  return JSON.stringify({ totalKeys: keys.length, entries });
};
fn();
