globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  return 'LEN:' + JSON.stringify(resp).length + ' KEYS:' + Object.keys(resp||{}).join(',');
};
fn();
