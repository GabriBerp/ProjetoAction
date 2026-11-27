globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const list = await unity.commands({ query: 'scene get_hierarchy' });
  const out = list && list.data ? list.data : list;
  return JSON.stringify({ type: typeof list, top: typeof out, keys: Object.keys(out||{}).join(','), sample: JSON.stringify(out).slice(0,2000) });
};
fn();
