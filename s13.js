globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const o = resp || {};
  const entries = [];
  for (const [k, v] of Object.entries(o)) {
    if (entries.length >= 40) break;
    const s = (typeof v === 'string') ? v.slice(0, 200) : (v && typeof v === 'object' ? JSON.stringify(v).slice(0, 200) : String(v).slice(0, 200));
    entries.push({ k, s });
  }
  return JSON.stringify({ keys: Object.keys(o).join(','), totalKeys: Object.keys(o).length, entries });
};
fn();
