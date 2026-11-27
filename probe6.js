globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const item = await unity.command('scene get_hierarchy', {});
  const list = Array.isArray(item) ? item : (item && typeof item === 'object' ? (item.data ?? item.result ?? item.hierarchy) : null);
  return JSON.stringify({ isObject: !Array.isArray(item) && typeof item === 'object', list });
};
fn();
