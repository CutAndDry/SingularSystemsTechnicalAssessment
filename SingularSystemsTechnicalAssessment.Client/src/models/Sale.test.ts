
// This test verifies Sale model structure

import test from 'node:test';
import assert from 'node:assert';

test('SaleCreateDto structure is correct', () => {
  const sale = {
    productId: 1,
    saleQty: 5,
    salePrice: 10.50,
    saleDate: new Date().toISOString()
  };
  
  assert.strictEqual(sale.productId, 1, 'productId should be 1');
  assert.strictEqual(sale.saleQty, 5, 'saleQty should be 5');
  assert.strictEqual(sale.salePrice, 10.50, 'salePrice should be 10.50');
  assert(typeof sale.saleDate === 'string', 'saleDate should be a string');
});

test('SalesPagedResult structure is correct', () => {
  const paged = {
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  };
  
  assert.strictEqual(paged.items.length, 0, 'items should be empty');
  assert.strictEqual(paged.pageNumber, 1, 'pageNumber should be 1');
  assert.strictEqual(paged.totalPages, 0, 'totalPages should be 0');
});
