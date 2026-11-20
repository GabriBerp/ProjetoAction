const V = (x, y, z) => ({ x, y, z });
const items = [
  ['1099511656753', 1.410, 4.526, 11.625], ['1099511656401', 0.660, 4.276, 11.750],
  ['1099511656394', 0.910, 3.276, 12.000], ['1099511655951', 2.292, 4.276, 11.750],
  ['1099511656019', 3.292, 3.526, 11.750], ['1099511656310', 2.792, 4.026, 11.750],
  ['1099511656795', 2.542, 3.276, 12.000], ['1099511656514', 2.542, 5.026, 11.625],
  ['1099511656885', 1.660, 3.526, 11.750], ['1099511656387', 0.910, 5.026, 11.625],
  ['1099511656246', 1.160, 4.026, 11.750],
];
const errors = [];
for (const [id, x, z, y] of items) {
  try { await unity.command('component set_field', { target: id, component: 'Transform', field: 'm_LocalPosition', value: V(x, y, z) }); }
  catch (e) { errors.push('item ' + id + ' ' + String(e.message).slice(0, 120)); }
}
// cadeira: yaw 180 (mesma pose das irmas) e base no mesmo nivel
try {
  await unity.command('component set_field', { target: '3298534910850', component: 'Transform', field: 'm_LocalPosition', value: V(-21.634, -1.517, 15.0) });
  await unity.command('component set_field', { target: '3298534910850', component: 'Transform', field: 'm_LocalRotation', value: { x: 0, y: 1, z: 0, w: 0 } });
  await unity.command('component set_field', { target: '3298534910850', component: 'Transform', field: 'm_LocalScale', value: V(1, 1, 1) });
} catch (e) { errors.push('cadeira ' + String(e.message).slice(0, 200)); }
// cabeca da vassoura no fim do cabo
try { await unity.command('component set_field', { target: '1099511656331', component: 'Transform', field: 'm_LocalPosition', value: V(0, -7.9, 0.11) }); }
catch (e) { errors.push('cabeca ' + String(e.message).slice(0, 200)); }
return { done: items.length + 2, errors };
