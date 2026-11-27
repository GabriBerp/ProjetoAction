globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const item = await unity.command('scene get_hierarchy', {});
  const obj = {};
  if (item && typeof item === 'object') {
    for (const k of Object.keys(item)) {
      let v = item[k];
      if (typeof v === 'string') v = v.slice(0, 3000);
      else if (v && typeof v === 'object' && !Array.isArray(v)) v = JSON.stringify(v).slice(0, 3000);
      else v = String(v);
      obj[k] = v;
    }
  } else {
    obj.raw = String(item);
  }
  return JSON.stringify(obj);
};
fn();
