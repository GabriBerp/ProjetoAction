globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const list = await unity.commands({ query: 'scene get_hierarchy' });
  const out = list && list.data ? list.data : list;
  const keys = Object.keys(out || {});
  const obj = {};
  keys.forEach((k, i) => {
    let v = out[k];
    if (typeof v === 'string') v = v.slice(0, 600);
    else if (v && typeof v === 'object' && !Array.isArray(v)) v = JSON.stringify(v).slice(0, 600);
    else v = String(v);
    obj[k] = v;
  });
  return JSON.stringify(obj);
};
fn();
