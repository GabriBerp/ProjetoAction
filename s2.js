globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp1 = await unity.command('scene get_hierarchy', {});
  return resp1;
};
fn();
