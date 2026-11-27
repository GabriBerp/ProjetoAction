globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  console.log('resp type:', typeof resp);
  console.log('keys:', Object.keys(resp||{}).join(','));
  console.log('head:', JSON.stringify(resp).slice(0, 1500));
};
fn();
