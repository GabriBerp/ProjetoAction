globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const out = resp && typeof resp === 'object' ? resp : {};
  const keys = Object.keys(out);
  const obj = {};
  keys.slice(0, 60).forEach(k => {
    let v = out[k];
    if (typeof v === 'string') v = v.slice(0, 200);
    else if (v && typeof v === 'object') v = JSON.stringify(v).slice(0, 200);
    else v = String(v).slice(0, 200);
    obj[k] = v;
  });
  return JSON.stringify(obj);
};
fn();
