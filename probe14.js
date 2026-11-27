globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  return JSON.stringify({ kind: typeof resp, keys: Object.keys(resp||{}).join(','), json: JSON.stringify(resp).slice(0,1200) });
};
fn();
