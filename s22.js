globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const o = resp || {};
  const k = Object.keys(o);
  return JSON.stringify({ totalKeys: k.length, first5: k.slice(0, 5).join(',') });
};
fn();
