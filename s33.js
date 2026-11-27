globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const o = resp || {};
  const k = Object.keys(o).slice(0, 8);
  const out = {};
  k.forEach(key => {
    const v = o[key];
    if (typeof v === 'string') out[key] = { t: 'string', len: v.length, head: v.slice(0, 150) };
    else if (v && typeof v === 'object' && !Array.isArray(v)) {
      const s = JSON.stringify(v);
      out[key] = { t: typeof v, len: s.length, head: s.slice(0, 150) };
    } else {
      out[key] = { t: typeof v, val: String(v).slice(0, 150) };
    }
  });
  return JSON.stringify({ totalKeys: k.length, out });
};
fn();
