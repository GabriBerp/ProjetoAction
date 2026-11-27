globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const resp = await unity.command('scene get_hierarchy', {});
  const out = resp && typeof resp === 'object' ? resp : { data: resp, result: resp };
  return JSON.stringify({ keys: Object.keys(out).join(','), data: JSON.stringify(out.data).slice(0,400), result: JSON.stringify(out.result).slice(0,400) });
};
fn();
