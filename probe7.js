globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const item = await unity.command('scene get_hierarchy', {});
  const s = JSON.stringify(item);
  return s.length > 500 ? s.slice(0, 500) + '...' + s.slice(-200) : s;
};
fn();
