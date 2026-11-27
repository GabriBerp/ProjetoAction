globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const out = resp && typeof resp === 'object' ? resp : null;
  return JSON.stringify({ keys: out ? Object.keys(out).join(',') : 'none', json: JSON.stringify(out).slice(0, 4000) });
};
fn();
