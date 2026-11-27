globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const item = await unity.command('scene get_hierarchy', {});
  return (item && item.data) ? JSON.stringify(item.data).slice(0,200000) : JSON.stringify(item);
};
fn();
