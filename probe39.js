globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const list = await unity.commands({ query: 'scene get_hierarchy' });
  console.log('type:', typeof list, 'keys:', Object.keys(list||{}).join(','));
  console.log('json:', JSON.stringify(list).slice(0, 1500));
};
fn();
