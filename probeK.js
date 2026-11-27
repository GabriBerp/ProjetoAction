globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const item = await unity.command('scene get_hierarchy', {});
  const s = JSON.stringify(item);
  const keys = Object.keys(item).join(',');
  return JSON.stringify({ keys, length: s.length });
};
fn();
