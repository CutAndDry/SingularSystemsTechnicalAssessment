import { addSale } from './salesService.js';

// This test verifies that the salesService module exports expected functions

import test from 'node:test';
import assert from 'node:assert';

test('salesService module exports expected functions', async () => {
  const module = await import('./salesService.js');
  assert(module.getSales !== undefined, 'getSales should be exported');
  assert(module.getAllSales !== undefined, 'getAllSales should be exported');
  assert(module.addSale !== undefined, 'addSale should be exported');
  assert(module.updateSale !== undefined, 'updateSale should be exported');
});
