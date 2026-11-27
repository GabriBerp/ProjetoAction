globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const out = resp && typeof resp === 'object' ? resp : null;
  const keys = Object.keys(out || {});
  const entries = [];
  for (let i = 0; i < keys.length; i++) {
    const k = keys[i];
    const v = out[k];
    if (typeof v === 'string') entries.push({ k, t: 'string', s: v.slice(0, 200) });
    else if (v && typeof v === 'object') entries.push({ k, t: typeof v, s: JSON.stringify(v).slice(0, 200) });
    else entries.push({ k, t: typeof v, s: String(v).slice(0, 200) });
  }
  return JSON.stringify({ total: keys.length, entries });
};
fn();
