globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {});
  const o = out && typeof out === 'object' ? out : { raw: String(out) };
  const keys = Object.keys(o);
  const obj2 = {};
  keys.slice(0, 120).forEach(k => { obj2[k] = JSON.stringify(o[k]).slice(0, 400); });
  return JSON.stringify({ totalKeys: keys.length, out: obj2 });
};
fn();
