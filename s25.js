globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const o = resp || {};
  const k = Object.keys(o);
  const first = k[0];
  const v = o[first];
  const s = JSON.stringify(v);
  return JSON.stringify({ firstKey: first, type: typeof v, len: s.length, sample: s.slice(0, 600), tail: s.slice(-200) });
};
fn();
