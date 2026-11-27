globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const obj = resp || {};
  const keys = Object.keys(obj);
  const out = {};
  for (let i = 0; i < keys.length; i++) {
    const k = keys[i];
    const v = obj[k];
    if (v && typeof v === 'object' && !Array.isArray(v)) {
      out[k] = JSON.stringify(v).slice(0, 2500);
    } else if (typeof v === 'string') {
      out[k] = v.slice(0, 2500);
    } else {
      out[k] = String(v).slice(0, 2500);
    }
  }
  return JSON.stringify({ keys, n: Object.keys(out).length, out });
};
fn();
