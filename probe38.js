globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const out = resp && typeof resp === 'object' && 'data' in resp ? resp.data : resp;
  const keys = Object.keys(out || {});
  const items = [];
  for (let i = 0; i < keys.length; i++) {
    const k = keys[i];
    let v = out[k];
    if (typeof v === 'string') {
      items.push({ k, t: typeof v, s: v.slice(0, 150) });
    } else if (v && typeof v === 'object') {
      items.push({ k, t: typeof v, s: JSON.stringify(v).slice(0, 150) });
    } else {
      items.push({ k, t: typeof v, s: String(v).slice(0, 150) });
    }
  }
  return JSON.stringify({ total: keys.length, items });
};
fn();
