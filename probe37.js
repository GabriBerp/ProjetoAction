globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const obj = resp || {};
  const keys = Object.keys(obj);
  const entries = [];
  for (let i = 0; i < keys.length; i++) {
    const k = keys[i];
    const v = obj[k];
    if (v && typeof v === 'object' && !Array.isArray(v)) {
      entries.push({ key: k, type: typeof v, str: JSON.stringify(v).slice(0, 200) });
    } else {
      entries.push({ key: k, type: typeof v, str: String(v).slice(0, 200) });
    }
  }
  return JSON.stringify({ total: keys.length, entries });
};
fn();
