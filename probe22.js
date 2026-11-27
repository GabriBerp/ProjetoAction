globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {});
  const obj = {};
  const keys = Object.keys(out || {});
  for (let i=0;i<keys.length;i++){
    const k = keys[i];
    let v = out[k];
    if (typeof v === 'string') v = v.slice(0,1500);
    else if (v && typeof v === 'object' && !Array.isArray(v)) v = JSON.stringify(v).slice(0,1500);
    else v = String(v);
    obj[k] = v;
  }
  return JSON.stringify(obj);
};
fn();
