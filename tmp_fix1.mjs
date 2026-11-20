const groups = {
  Arquitetura: '56075093047711',
  Eletrodomesticos: '3298534914478',
  Moveis: '3298534914484',
};
const plan = [
  ['1099511656294', 'Arquitetura'], ['1099511656861', 'Arquitetura'],
  ['1099511656106', 'Arquitetura'], ['1099511656193', 'Arquitetura'], ['1099511656425', 'Arquitetura'],
  ['1099511656096', 'Eletrodomesticos'], ['1099511655879', 'Eletrodomesticos'],
  ['1099511656359', 'Eletrodomesticos'], ['1099511655964', 'Eletrodomesticos'], ['1099511656546', 'Eletrodomesticos'],
  ['1099511656180', 'Moveis'], ['1099511656822', 'Moveis'],
  ['1099511656846', 'Moveis'], ['1099511656142', 'Moveis'], ['3298534910850', 'Moveis'], ['1099511656302', 'Moveis'],
];
const errors = [];
for (const [id, g] of plan) {
  try {
    await unity.command('gameobject set_parent', { target: id, parent: groups[g], 'world-position-stays': true });
  } catch (e) { errors.push(g + ':' + id + ' ' + String(e.message).slice(0, 160)); }
}
return { moved: plan.length, errors };
