globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const list = await unity.commands({ query: 'scene get_hierarchy' });
  const out = list && list.data ? list.data : list;
  return JSON.stringify(out);
};
fn();
