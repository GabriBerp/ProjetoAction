globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  console.log('RAW keys:', Object.keys(resp).join(','));
  console.log('full:', JSON.stringify(resp).slice(0, 3000));
};
fn();
