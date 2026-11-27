globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const item = await unity.command('scene get_hierarchy', {});
  const keys = Object.keys(item);
  return JSON.stringify({ keys: keys.slice(0, 30), sample: keys.slice(0, 3).map(k => ({ k, v: (typeof item[k] === 'string' ? item[k].slice(0,300) : String(item[k]).slice(0,100)) })) });
};
fn();
