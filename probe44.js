globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const obj = resp || {};
  const keys = Object.keys(obj);
  const out = JSON.stringify(obj).slice(0, 5000);
  return JSON.stringify({ keys, out });
};
fn();
