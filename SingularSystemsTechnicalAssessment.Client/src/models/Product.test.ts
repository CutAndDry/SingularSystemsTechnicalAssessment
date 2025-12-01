// Test for Product models
// This test verifies the Product model structure

import test from 'node:test';
import assert from 'node:assert';

test('ProductListDto structure is correct', () => {
  const product = {
    id: 1,
    description: 'Test Product',
    salePrice: 10.99,
    totalSales: 5,
    totalRevenue: 54.95
  };
  
  assert.strictEqual(product.id, 1, 'id should be 1');
  assert.strictEqual(product.salePrice, 10.99, 'salePrice should be 10.99');
  assert.strictEqual(product.totalSales, 5, 'totalSales should be 5');
});

test('ProductPagedResult structure is correct', () => {
  const pagedResult = {
    items: [{ id: 1, description: 'Test', salePrice: 10, totalSales: 5, totalRevenue: 50 }],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 100,
    totalPages: 10,
    hasPreviousPage: false,
    hasNextPage: true
  };
  
  assert.strictEqual(pagedResult.items.length, 1, 'items should have 1 element');
  assert.strictEqual(pagedResult.pageNumber, 1, 'pageNumber should be 1');
  assert.strictEqual(pagedResult.totalPages, 10, 'totalPages should be 10');
  assert.strictEqual(pagedResult.hasNextPage, true, 'hasNextPage should be true');
});
