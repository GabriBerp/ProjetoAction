globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {});
  return JSON.stringify({ keys: Object.keys(out||{}).join(','), out });
};
fn();
