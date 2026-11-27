globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {}, { resultOnly: false });
  return { type: typeof out, keys: out ? Object.keys(out).join(',') : 'null', json: JSON.stringify(out).slice(0, 1200) };
};
fn();
