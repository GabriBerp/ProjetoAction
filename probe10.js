globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {});
  const keys = Object.keys(out || {});
  const obj = {};
  keys.forEach(k => { obj[k] = (typeof out[k] === 'string' ? out[k].slice(0,120) : String(out[k]).slice(0,120)); });
  return JSON.stringify(obj);
};
fn();
