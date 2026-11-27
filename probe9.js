globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {});
  const keys = Object.keys(out || {});
  return JSON.stringify({ keys, out });
};
fn();
