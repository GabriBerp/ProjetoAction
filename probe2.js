globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const item = await unity.command('scene get_hierarchy', {});
  return JSON.stringify(item);
};
fn();
