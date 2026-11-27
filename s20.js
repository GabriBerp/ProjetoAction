globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const o = resp || {};
  const k = Object.keys(o);
  if (!k.length) return 'NO KEYS';
  const first = k[0];
  const v = o[first];
  if (typeof v === 'string') return 'KEY:' + first + ' VAL:' + v;
  if (v && typeof v === 'object') return 'KEY:' + first + ' OBJ:' + JSON.stringify(v);
  return 'KEY:' + first + ' TYPE:' + typeof v;
};
fn();
