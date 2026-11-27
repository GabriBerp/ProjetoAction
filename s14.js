globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  return JSON.stringify({ type: typeof resp, keys: Object.keys(resp||{}).join(','), head: JSON.stringify(resp).slice(0, 2000) });
};
fn();
