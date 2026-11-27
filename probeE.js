globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const out = resp && typeof resp === 'object' ? resp : null;
  const keys = Object.keys(out || {});
  const seen = [];
  for (let i = 0; i < keys.length; i++) {
    const k = keys[i];
    let v = out[k];
    if (typeof v === 'string') v = v.slice(0, 300);
    else if (v && typeof v === 'object' && !Array.isArray(v)) v = JSON.stringify(v).slice(0, 300);
    else v = String(v).slice(0, 300);
    seen.push({ k, v });
  }
  return JSON.stringify({ keys, seen });
};
fn();
