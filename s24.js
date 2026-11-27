globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const o = resp || {};
  const k = Object.keys(o);
  if (!k.length) return 'NO KEYS';
  const first = k[0];
  const v = o[first];
  if (v && typeof v === 'object' && !Array.isArray(v)) {
    const s = JSON.stringify(v);
    return JSON.stringify({ firstKey: first, type: typeof v, len: s.length, sample: s.slice(0, 600), tail: s.slice(-200) });
  }
  return JSON.stringify({ firstKey: first, type: typeof v, val: String(v).slice(0, 300) });
};
fn();
