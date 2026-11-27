globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const o = resp || {};
  const k = Object.keys(o);
  if (!k.length) return 'NO KEYS';
  const first = k[0];
  const v = o[first];
  if (typeof v === 'string') return JSON.stringify({ firstKey: first, val: v, len: v.length });
  if (v && typeof v === 'object') {
    const s = JSON.stringify(v);
    return JSON.stringify({ firstKey: first, len: s.length, sample: s.slice(0, 200) });
  }
  return JSON.stringify({ firstKey: first, type: typeof v });
};
fn();
