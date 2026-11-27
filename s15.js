globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  return 'KEYS:' + Object.keys(resp||{}).join(',') + '\nHEAD:' + JSON.stringify(resp).slice(0, 1500);
};
fn();
