<template>
  <div class="products-page">
    <!-- Header Section -->
    <header class="page-header">
      <div class="header-content">
        <h1>Products</h1>
        <button
          class="btn btn-add"
          @click="showAddModal = true"
        >
          <span class="plus-icon">+</span> Add Product
        </button>
      </div>
    </header>

    <!-- Filter Panel -->
    <div class="filter-section">
      <div class="filter-wrapper">
        <div class="filter-inputs">
          <div class="filter-group">
            <input
              v-model.number="filters.id"
              type="number"
              placeholder="Filter by Product ID"
              class="filter-input no-spinner"
            />
          </div>
          <div class="filter-group">
            <input
              v-model="filters.name"
              type="text"
              placeholder="Filter by Product"
              class="filter-input"
            />
          </div>
        </div>
        <button
          class="btn btn-reset"
          @click="resetFilters"
        >
          ✕ Reset
        </button>
      </div>
    </div>

    <!-- Add Product Modal -->
    <div v-if="showAddModal" class="modal-overlay" @click.self="closeAddModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Add New Product</h3>
          <button class="modal-close" @click="closeAddModal">&times;</button>
        </div>
        <form @submit.prevent="submitAddProduct" class="modal-form" enctype="multipart/form-data">
          <div class="form-group">
            <label>Description *</label>
            <input
              v-model="newProduct.description"
              type="text"
              required
              placeholder="Enter product name"
            />
          </div>
          <div class="form-group">
            <label>Sale Price *</label>
            <input
              v-model.number="newProduct.salePrice"
              type="number"
              step="0.01"
              required
              placeholder="Enter sale price"
            />
          </div>
          <div class="form-group">
            <label>Product Image</label>
            <input type="file" accept="image/*" @change="onImageSelected" />
            <div v-if="newProduct.imagePreview" style="margin-top:8px;">
              <img :src="newProduct.imagePreview" alt="preview" style="max-width:160px;border-radius:6px;" />
            </div>
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn btn-success" :disabled="submitting">
              <span v-if="submitting">Saving…</span><span v-else>Add Product</span>
            </button>
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

    <!-- Products Grid -->
    <div class="products-container">
      <div v-if="loading" class="products-grid">
        <div v-for="i in 12" :key="'skeleton-'+i" class="product-card skeleton-card">
          <div class="card-image-skeleton"></div>
          <div class="card-content-skeleton">
            <div class="skeleton-line" style="width: 80%; margin-bottom: 8px;"></div>
            <div class="skeleton-line" style="width: 60%;"></div>
          </div>
        </div>
      </div>

      <div v-else-if="filteredProducts.length === 0" class="empty-state">
        <p>No products found.</p>
      </div>

      <div v-else class="products-grid">
        <div
          v-for="product in filteredProducts"
          :key="product.id"
          class="product-card"
        >
          <div class="card-image-container">
            <img
              v-if="product.image"
              :src="product.image"
              :alt="product.description"
              class="card-image"
            />
            <div v-else class="card-image-placeholder">
              <span class="placeholder-text">No Image</span>
            </div>
            <div class="card-badge">ID: {{ product.id }}</div>
          </div>
          <div class="card-content">
            <h3 class="card-title">{{ product.description }}</h3>
            <div class="card-price">${{ (product.salePrice ?? 0).toFixed(2) }}</div>
          </div>
        </div>
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
const totalPages = ref(1);
const loading = ref(false);
const showAddModal = ref(false);

const filters = ref({
  id: null,
  name: ''
});

// newProduct holds file and preview
const newProduct = ref({ description: '', salePrice: 0, imageFile: null, imagePreview: '' });
const submitting = ref(false);

const fetchProducts = async () => {
  loading.value = true;
  try {
    const res = await getAllProducts(pageNumber.value, pageSize.value);
    const data = res.data || {};
    allProducts.value = data.items || data.Items || [];
    totalPages.value = data.totalPages ?? data.TotalPages ?? Math.max(1, Math.ceil((data.totalCount ?? allProducts.value.length) / pageSize.value));
    pageNumber.value = data.pageNumber ?? data.PageNumber ?? pageNumber.value;
  } catch (err) {
    console.error('Error fetching products:', err);
  } finally {
    loading.value = false;
  }
};

// Filter products (current page items)
const filteredProducts = computed(() => {
  return allProducts.value.filter(product => {
    const matchId = filters.value.id === null || filters.value.id === '' || product.id === filters.value.id;
    const matchName = !filters.value.name || (product.description || '').toLowerCase().includes(filters.value.name.toLowerCase());
    return matchId && matchName;
  });
});

// Use server total pages
const computedTotalPages = computed(() => totalPages.value);

const submitAddProduct = async () => {
  if (submitting.value) return;
  submitting.value = true;
  try {
    const fd = new FormData();
    fd.append('description', newProduct.value.description || '');
    fd.append('salePrice', String(newProduct.value.salePrice ?? 0));
    if (newProduct.value.imageFile) fd.append('image', newProduct.value.imageFile);

    if (newProduct.value.imageFile) {
      await createProduct(fd);
    } else {
      await createProduct({
        description: newProduct.value.description || '',
        salePrice: newProduct.value.salePrice ?? 0
      });
    }

    // cleanup
    if (newProduct.value.imagePreview) { try { URL.revokeObjectURL(newProduct.value.imagePreview); } catch {} }
    newProduct.value = { description: '', salePrice: 0, imageFile: null, imagePreview: '' };
    showAddModal.value = false;
    pageNumber.value = 1;
    await fetchProducts();
  } catch (err) {
    console.error('Error adding product:', err);
    alert('Failed to add product: ' + (err?.message || 'unknown'));
  } finally {
    submitting.value = false;
  }
};

const onImageSelected = (event) => {
  const file = event.target.files && event.target.files[0];
  if (!file) {
    if (newProduct.value.imagePreview) { try { URL.revokeObjectURL(newProduct.value.imagePreview); } catch {} }
    newProduct.value.imageFile = null;
    newProduct.value.imagePreview = '';
    return;
  }
  newProduct.value.imageFile = file;
  if (newProduct.value.imagePreview) { try { URL.revokeObjectURL(newProduct.value.imagePreview); } catch {} }
  newProduct.value.imagePreview = URL.createObjectURL(file);
};

const closeAddModal = () => {
  if (newProduct.value.imagePreview) { try { URL.revokeObjectURL(newProduct.value.imagePreview); } catch {} }
  newProduct.value = { description: '', salePrice: 0, imageFile: null, imagePreview: '' };
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
    fetchProducts();
  }
};

const prevPage = () => {
  if (pageNumber.value > 1) {
    pageNumber.value--;
    fetchProducts();
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

@media (max-width: 900px) {
  .products-page {
    margin-left: 0 !important;
    padding-top: calc(var(--mobile-topbar-height, 56px) + 1rem);
    padding-left: 1rem;
    padding-right: 1rem;
  }
}
</style>

<style scoped>
/* Products Page Container */
.products-page {
  padding: 2rem;
  max-width: none;
  width: 100%;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  min-height: 100vh;
  background-attachment: fixed;
}

/* Header Section */
.page-header {
  margin-bottom: 2.5rem;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

.page-header h1 {
  font-size: 2.5rem;
  font-weight: 700;
  color: #2c3e50;
  margin: 0;
  letter-spacing: -0.5px;
}

.btn-add {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  font-size: 1rem;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.btn-add:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
}

.plus-icon {
  font-size: 1.2rem;
  font-weight: bold;
}

/* Filter Section */
.filter-section {
  margin-bottom: 2rem;
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.filter-wrapper {
  display: flex;
  gap: 1rem;
  align-items: flex-end;
  flex-wrap: wrap;
}

.filter-inputs {
  display: flex;
  gap: 1rem;
  flex: 1;
  min-width: 300px;
  flex-wrap: wrap;
}

.filter-group {
  flex: 1;
  min-width: 200px;
}

.filter-input {
  width: 100%;
  padding: 0.75rem 1rem;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 0.95rem;
  transition: all 0.3s ease;
  font-family: inherit;
}

.filter-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.btn-reset {
  padding: 0.75rem 1.25rem;
  background: #e0e0e0;
  color: #2c3e50;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s ease;
  white-space: nowrap;
}

.btn-reset:hover {
  background: #d0d0d0;
  transform: translateY(-1px);
}

/* Products Container */
.products-container {
  padding: 0;
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  background: white;
  border-radius: 12px;
  color: #7f8c8d;
  font-size: 1.1rem;
}

/* Products Grid */
.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 2rem;
  margin-bottom: 2rem;
  width: 100%;
}

/* Product Cards */
.product-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;
  height: 100%;
}

.product-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);
}

/* Card Image Container */
.card-image-container {
  position: relative;
  width: 100%;
  overflow: hidden;
  background: #f5f5f5;
  aspect-ratio: 1;
}

.card-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.product-card:hover .card-image {
  transform: scale(1.05);
}

.card-image-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, #f5f5f5 0%, #e0e0e0 100%);
  color: #999;
  font-size: 0.95rem;
  font-weight: 500;
}

.card-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  font-size: 0.85rem;
  font-weight: 600;
}

/* Card Content */
.card-content {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.card-title {
  font-size: 1.05rem;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 0.75rem 0;
  line-height: 1.4;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.card-price {
  font-size: 1.5rem;
  font-weight: 700;
  color: #27ae60;
}

/* Skeleton Loading */
.skeleton-card {
  pointer-events: none;
}

.card-image-skeleton {
  width: 100%;
  aspect-ratio: 1;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 37%, #f0f0f0 63%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}

.card-content-skeleton {
  padding: 1.5rem;
}

.skeleton-line {
  height: 12px;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 37%, #f0f0f0 63%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 4px;
}

/* Pagination */
.pagination-container {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  padding: 2rem 0;
}

.pagination-btn {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);
}

.pagination-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.5);
}

.pagination-btn:disabled {
  background: #bdc3c7;
  cursor: not-allowed;
  box-shadow: none;
}

.pagination-info {
  font-weight: 600;
  color: #2c3e50;
  min-width: 150px;
  text-align: center;
}

/* Responsive Design */
@media (max-width: 1200px) {
  .products-grid {
    grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
    gap: 1.5rem;
  }
}

@media (max-width: 768px) {
  .products-page {
    padding: 1rem;
  }

  .page-header h1 {
    font-size: 1.8rem;
  }

  .header-content {
    flex-direction: column;
    align-items: flex-start;
  }

  .btn-add {
    width: 100%;
    justify-content: center;
  }

  .products-grid {
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 1rem;
  }

  .filter-inputs {
    flex-direction: column;
    min-width: 100%;
  }

  .filter-group {
    width: 100%;
  }

  .btn-reset {
    width: 100%;
  }

  .filter-wrapper {
    flex-direction: column;
  }

  .pagination-info {
    font-size: 0.9rem;
  }
}

@media (max-width: 480px) {
  .products-page {
    padding: 0.75rem;
  }

  .page-header h1 {
    font-size: 1.5rem;
  }

  .products-grid {
    grid-template-columns: 1fr;
  }

  .filter-section {
    padding: 1rem;
  }

  .card-title {
    font-size: 0.95rem;
  }

  .card-price {
    font-size: 1.3rem;
  }
}

/* Animations */
@keyframes slideInCard {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.product-card {
  animation: slideInCard 0.3s ease-out;
}

/* Remove number input spinners */
.no-spinner::-webkit-outer-spin-button,
.no-spinner::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.no-spinner[type=number] {
  -moz-appearance: textfield;
}
</style>
