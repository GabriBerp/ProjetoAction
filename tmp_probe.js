globalThis.setTimeout = function() {};
globalThis.clearTimeout = function() {};
await new Promise(r => { globalThis.setTimeout(r, 500); });
return unity.commands({ query: "scene get_hierarchy" });
