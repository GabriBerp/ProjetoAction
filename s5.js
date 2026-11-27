globalThis.setTimeout = function(){};
globalThis.clearTimeout = function(){};
const fn = async () => {
  const out = await unity.command('scene get_hierarchy', {});
  return JSON.stringify({
    ok: out !== undefined,
    type: typeof out,
    keys: out ? Object.keys(out) : [],
    head: out ? JSON.stringify(out).slice(0, 800) : 'null'
  });
};
fn();
