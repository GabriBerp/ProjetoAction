globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {});
  return JSON.stringify({
    keys: Object.keys(out || {}).join(','),
    totalKeys: Object.keys(out || {}).length,
    rawKeys: out ? Object.keys(out).map(k => k + '=' + String(out[k]).slice(0, 150)).join('\n') : 'null'
  });
};
fn();
