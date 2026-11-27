globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {});
  const keys = Object.keys(out||{});
  const obj = {};
  keys.forEach(k => {
    let v = out[k];
    if (v && typeof v === 'object' && !Array.isArray(v)) {
      v = JSON.stringify(v).slice(0,1500);
    } else if (typeof v === 'string') {
      v = v.slice(0,1500);
    } else {
      v = String(v);
    }
    obj[k] = v;
  });
  return JSON.stringify(obj);
};
fn();
