globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp1 = await unity.command('scene get_hierarchy', {});
  return JSON.stringify({ raw: resp1, keys: Object.keys(resp1||{}).join(',') });
};
fn();
