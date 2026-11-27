globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const out = resp && typeof resp === 'object' ? resp : null;
  const keys = Object.keys(out || {});
  let s = '';
  for (let i = 0; i < keys.length; i++) {
    const k = keys[i];
    s += k + '=' + JSON.stringify(out[k]).slice(0, 200) + '\n';
  }
  return s;
};
fn();
