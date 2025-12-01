import { getAllProducts } from './productService.js';
// Test for productService module
// This test verifies that the productService module exports expected functions

import test from 'node:test';
import assert from 'node:assert';

test('productService module exports expected functions', async () => {
  const module = await import('./productService.js');
  assert(module.getAllProducts !== undefined, 'getAllProducts should be exported');
  assert(module.createProduct !== undefined, 'createProduct should be exported');
  assert(module.updateProduct !== undefined, 'updateProduct should be exported');
});
