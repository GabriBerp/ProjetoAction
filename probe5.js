globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const item = await unity.command('scene get_hierarchy', {});
  const list = Array.isArray(item) ? item : item?.data ?? item?.result ?? item?.hierarchy ?? null;
  return JSON.stringify({ type: typeof item, isArray: Array.isArray(item), listLength: list ? list.length : 'nullish', sample: list && Array.isArray(list) ? list.slice(0, 10).map(x => typeof x === 'string' ? x.slice(0,200) : String(x).slice(0,200) ) : list });
};
fn();
