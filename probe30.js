globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const list = await unity.commands({ query: 'scene get_hierarchy' });
  return JSON.stringify({ type: typeof list, keys: Object.keys(list||{}).join(','), sample: JSON.stringify(list).slice(0,800) });
};
fn();
