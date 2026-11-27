globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const out = resp && resp.data ? resp.data : resp;
  const keys = Object.keys(out || {});
  return JSON.stringify({ totalKeys: keys.length, keys: keys.slice(0, 120).join(','), sampleKeys: keys.slice(0, 5).join(',') });
};
fn();
