globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const o = resp || {};
  const k = Object.keys(o);
  if (!k.length) return 'NO KEYS';
  const first = k[0];
  const v = o[first];
  if (typeof v === 'string') return 'KEY:' + first + ' VALUE:' + v.slice(0, 1200);
  if (v && typeof v === 'object') return 'KEY:' + first + ' OBJ:' + JSON.stringify(v).slice(0, 1200);
  return 'KEY:' + first + ' TYPE:' + typeof v;
};
fn();
