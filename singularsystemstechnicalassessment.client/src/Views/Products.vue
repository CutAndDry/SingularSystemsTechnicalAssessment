<template>
  <div class="products-page">
    <header class="page-header">
      <h1>Products</h1>
    </header>

    <!-- Filter Panel -->
    <div class="filter-panel">
      <div class="filter-row">
        <div class="filter-group">
          <label>Product ID</label>
          <input
            v-model.number="filters.id"
            type="number"
            placeholder="Filter by ID"
          />
        </div>
        <div class="filter-group">
          <label>Product Name</label>
          <input
            v-model="filters.name"
            type="text"
            placeholder="Filter by name"
          />
        </div>
        <div class="filter-actions">
          <button
            class="btn btn-secondary"
            @click="applyFilters"
          >
            Apply Filters
          </button>
          <button
            class="btn btn-outline"
            @click="resetFilters"
          >
            Reset
          </button>
          <button
            class="btn btn-primary"
            @click="showAddModal = true"
          >
            + Add Product
          </button>
        </div>
      </div>
    </div>

    <!-- Add Product Modal -->
    <div v-if="showAddModal" class="modal-overlay" @click.self="closeAddModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Add New Product</h3>
          <button class="modal-close" @click="closeAddModal">&times;</button>
        </div>
        <form @submit.prevent="submitAddProduct" class="modal-form">
          <div class="form-group">
            <label>Product Name *</label>
            <input
              v-model="newProduct.name"
              type="text"
              required
              placeholder="Enter product name"
            />
          </div>
          <div class="form-group">
            <label>Price *</label>
            <input
              v-model.number="newProduct.price"
              type="number"
              step="0.01"
              required
              placeholder="Enter price"
            />
          </div>
          <div class="form-group">
            <label>Description</label>
            <input
              v-model="newProduct.description"
              type="text"
              placeholder="Enter description"
            />
          </div>
          <div class="modal-actions">
            <button type="submit" class="btn btn-success">Add Product</button>
            <button
              type="button"
              class="btn btn-secondary"
              @click="closeAddModal"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Products Table -->
    <div class="table-container">
      <div class="table-wrapper">
        <table class="products-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Price</th>
              <th>Total Sales</th>
              <th>Total Revenue</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-if="loading"
              v-for="i in 8"
              :key="'skeleton-'+i"
              class="skeleton-row"
            >
              <td colspan="5">&nbsp;</td>
            </tr>

            <tr v-for="product in paginatedProducts" :key="product.id">
              <td>{{ product.id }}</td>
              <td>{{ product.name }}</td>
              <td>${{ product.price.toFixed(2) }}</td>
              <td>{{ product.totalSales }}</td>
              <td>${{ product.totalRevenue.toFixed(2) }}</td>
            </tr>

            <tr v-if="!loading && paginatedProducts.length === 0">
              <td colspan="5" class="text-center">No products found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Pagination -->
    <div class="pagination-container">
      <button
        class="pagination-btn"
        :disabled="pageNumber <= 1"
        @click="prevPage"
      >
        ‹ Previous
      </button>
      <span class="pagination-info">Page {{ pageNumber }} of {{ computedTotalPages }}</span>
      <button
        class="pagination-btn"
        :disabled="pageNumber >= computedTotalPages"
        @click="nextPage"
      >
        Next ›
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { getAllProducts, createProduct } from '../Services/productService.js';

const allProducts = ref([]);
const pageNumber = ref(1);
const pageSize = ref(10);
const loading = ref(false);
const showAddModal = ref(false);

const filters = ref({
  id: null,
  name: ''
});

const newProduct = ref({ name: '', description: '', price: 0 });

const fetchProducts = async () => {
  loading.value = true;
  try {
    const res = await getAllProducts(pageNumber.value, pageSize.value);
    allProducts.value = res.data.items || [];
  } catch (err) {
    console.error('Error fetching products:', err);
  } finally {
    loading.value = false;
  }
};

// Filter products based on criteria
const filteredProducts = computed(() => {
  return allProducts.value.filter(product => {
    const matchId = filters.value.id === null || filters.value.id === '' || product.id === filters.value.id;
    const matchName = !filters.value.name || product.name.toLowerCase().includes(filters.value.name.toLowerCase());
    return matchId && matchName;
  });
});

// Paginate filtered products
const paginatedProducts = computed(() => {
  const start = (pageNumber.value - 1) * pageSize.value;
  const end = start + pageSize.value;
  return filteredProducts.value.slice(start, end);
});

// Update totalPages based on filtered results
const computedTotalPages = computed(() => {
  return Math.max(1, Math.ceil(filteredProducts.value.length / pageSize.value));
});

const submitAddProduct = async () => {
  try {
    await createProduct(newProduct.value);
    newProduct.value = { name: '', description: '', price: 0 };
    showAddModal.value = false;
    pageNumber.value = 1;
    fetchProducts();
  } catch (err) {
    console.error('Error adding product:', err);
  }
};

const closeAddModal = () => {
  showAddModal.value = false;
};

const applyFilters = () => {
  pageNumber.value = 1;
};

const resetFilters = () => {
  filters.value = { id: null, name: '' };
  pageNumber.value = 1;
};

const nextPage = () => {
  if (pageNumber.value < computedTotalPages.value) {
    pageNumber.value++;
  }
};

const prevPage = () => {
  if (pageNumber.value > 1) {
    pageNumber.value--;
  }
};

onMounted(fetchProducts);
</script>

<style scoped>
.products-page {
  margin-left: var(--sidebar-width, 220px);
  padding: 1.5rem;
  min-height: 100vh;
  background: var(--color-background, #f5f5f5);
  transition: margin-left 0.2s ease;
}

.page-header {
  margin-bottom: 1.5rem;
}

.page-header h1 {
  font-size: 2rem;
  margin: 0;
  color: var(--color-heading, #2c3e50);
}

/* Filter Panel */
.filter-panel {
  background: #e8eef2;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
}

.filter-row {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.filter-group {
  flex: 1;
}

.filter-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #2c3e50;
}

.filter-group input {
  width: 100%;
  padding: 0.6rem;
  border: 1px solid #bdc3c7;
  border-radius: 6px;
  font-size: 14px;
}

.filter-group input:focus {
  outline: none;
  border-color: #3498db;
  box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
}

.filter-actions {
  display: flex;
  gap: 1rem;
}

/* Button Styles */
.btn {
  padding: 0.6rem 1.2rem;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-primary {
  background: #3498db;
  color: white;
}

.btn-primary:hover {
  background: #2980b9;
}

.btn-secondary {
  background: #95a5a6;
  color: white;
}

.btn-secondary:hover {
  background: #7f8c8d;
}

.btn-success {
  background: #27ae60;
  color: white;
}

.btn-success:hover {
  background: #229954;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  max-width: 500px;
  width: 95%;
  animation: slideIn 0.2s ease;
}

@keyframes slideIn {
  from { transform: translateY(-20px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e6e6e6;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.3rem;
  color: #2c3e50;
}

.modal-close {
  background: transparent;
  border: none;
  font-size: 28px;
  cursor: pointer;
  color: #7f8c8d;
}

.modal-close:hover {
  color: #2c3e50;
}

.modal-form {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #2c3e50;
}

.form-group input {
  width: 100%;
  padding: 0.6rem;
  border: 1px solid #bdc3c7;
  border-radius: 6px;
  font-size: 14px;
}

.form-group input:focus {
  outline: none;
  border-color: #3498db;
  box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid #e6e6e6;
}

/* Table */
.table-container {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  margin-bottom: 1.5rem;
}

.table-wrapper {
  overflow-x: auto;
}

.products-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.products-table thead {
  background: #f8f9fa;
  border-bottom: 2px solid #e6e6e6;
}

.products-table th {
  padding: 1rem;
  text-align: left;
  font-weight: 600;
  color: #2c3e50;
}

.products-table td {
  padding: 0.8rem 1rem;
  border-bottom: 1px solid #e6e6e6;
}

.products-table tbody tr:hover {
  background: #f8f9fa;
}

.skeleton-row td {
  background: linear-gradient(90deg, rgba(0,0,0,0.02) 25%, rgba(0,0,0,0.04) 37%, rgba(0,0,0,0.02) 63%);
  height: 40px;
  animation: shimmer 1.5s infinite;
}

@keyframes shimmer {
  0% { opacity: 1; }
  50% { opacity: 0.7; }
  100% { opacity: 1; }
}

.text-center {
  text-align: center;
  color: #7f8c8d;
}

/* Pagination */
.pagination-container {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
}

.pagination-btn {
  padding: 0.6rem 1rem;
  background: #3498db;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: background 0.2s ease;
}

.pagination-btn:hover:not(:disabled) {
  background: #2980b9;
}

.pagination-btn:disabled {
  background: #bdc3c7;
  cursor: not-allowed;
}

.pagination-info {
  font-weight: 600;
  color: #2c3e50;
}

/* Responsive */
@media (max-width: 900px) {
  .products-page {
    margin-left: 64px;
    padding: 1rem;
  }

  .filter-content {
    flex-direction: column;
    align-items: flex-start;
  }

  .products-table {
    font-size: 12px;
  }

  .products-table th,
  .products-table td {
    padding: 0.6rem 0.4rem;
  }

  .page-header h1 {
    font-size: 1.5rem;
  }
}
</style>

<style scoped>
@media (max-width: 900px) {
  .products-page {
    margin-left: 0 !important;
    padding-top: calc(var(--mobile-topbar-height, 56px) + 1rem);
    padding-left: 1rem;
    padding-right: 1rem;
  }
}
</style>
