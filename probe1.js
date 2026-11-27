globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
globalThis.setImmediate = function(){};
const fn = async () => {
  const item = await unity.command('scene get_hierarchy', {});
  return typeof item === 'string' ? item.slice(0, 2000) : JSON.stringify(item).slice(0, 2000);
};
fn();
