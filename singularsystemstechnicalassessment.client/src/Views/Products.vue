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
        <div class="filter-group">
          <label>Product Description</label>
          <input
            v-model="filters.description"
            type="text"
            placeholder="Filter by description"
          />
        </div>
        <div class="filter-actions">
          
          <button
            class="btn btn-primary"
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
        <form @submit.prevent="submitAddProduct" class="modal-form" enctype="multipart/form-data">
          <div class="form-group">
            <label>Description *</label>
            <input
              v-model="newProduct.description"
              type="text"
              required
              placeholder="Enter product description"
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

    <!-- Products Table -->
    <div class="table-container">
      <div class="table-wrapper">
        <table class="products-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Description</th>
              <th>Sale Price</th>
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

            <!-- clickable rows: open modal on click or keyboard (Enter/Space) -->
            <tr
              v-for="product in filteredProducts"
              :key="product.id"
              tabindex="0"
              role="button"
              @click="openProduct(product)"
              @keydown.enter.prevent="openProduct(product)"
              @keydown.space.prevent="openProduct(product)"
              class="clickable-row"
            >
              <td>{{ product.id }}</td>
              <td>{{ product.description }}</td>
              <td>${{ (product.salePrice ?? 0).toFixed(2) }}</td>
              <td>{{ product.totalSales }}</td>
              <td>${{ (product.totalRevenue ?? 0).toFixed(2) }}</td>
            </tr>

            <tr v-if="!loading && filteredProducts.length === 0">
              <td colspan="5" class="text-center">No products found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Product details modal (inline so the Edit button can be inside the popup) -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Product Details</h3>
          <button class="modal-close" @click="closeModal">&times;</button>
        </div>

        <div class="modal-form" style="padding:1rem;">
          <div style="display:flex;gap:1.5rem;align-items:flex-start;">
            <div style="flex:1">
              <p><strong>ID:</strong> {{ selectedProduct?.id ?? '-' }}</p>
              <p><strong>Description:</strong> {{ selectedProduct?.description ?? '-' }}</p>
              <p><strong>Sale Price:</strong> ${{ (selectedProduct?.salePrice ?? 0).toFixed(2) }}</p>
              <p><strong>Category:</strong> {{ selectedProduct?.category ?? '-' }}</p>
              <p><strong>Total Sales:</strong> {{ selectedProduct?.totalSales ?? 0 }}</p>
              <p><strong>Total Revenue:</strong> ${{ (selectedProduct?.totalRevenue ?? 0).toFixed(2) }}</p>
            </div>
            <div v-if="selectedProduct?.image" style="width:180px;">
              <img :src="selectedProduct.image" alt="product image" style="max-width:100%;border-radius:6px;" />
            </div>
          </div>
        </div>

        <div class="modal-actions" style="justify-content:flex-end; padding:1rem;">
          <button class="btn btn-primary" @click="openEditFromDetails">Edit</button>
          <button class="btn btn-secondary" @click="closeModal">Close</button>
        </div>
      </div>
    </div>

    <!-- Edit Product Modal -->
    <div v-if="showEditModal" class="modal-overlay" @click.self="closeEditModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Edit Product</h3>
          <button class="modal-close" @click="closeEditModal">&times;</button>
        </div>
        <form @submit.prevent="submitEditProduct" class="modal-form">
          <div class="form-group">
            <label>Description *</label>
            <input v-model="editProduct.description" type="text" required placeholder="Enter product description" />
          </div>
          <div class="form-group">
            <label>Sale Price *</label>
            <input v-model.number="editProduct.salePrice" type="number" step="0.01" required placeholder="Enter sale price" />
          </div>
          <div class="form-group">
            <label>Category</label>
            <input v-model="editProduct.category" type="text" placeholder="Enter category" />
          </div>
          <div class="form-group">
            <label>Image URL (optional)</label>
            <input v-model="editProduct.image" type="text" placeholder="Set image URL or leave blank" />
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn btn-success" :disabled="submitting">Save</button>
            <button type="button" class="btn btn-secondary" @click="closeEditModal">Cancel</button>
          </div>
        </form>
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
import { getAllProducts, createProduct, updateProduct } from '../Services/productService.js';

const allProducts = ref([]);
const pageNumber = ref(1);
const pageSize = ref(10);
const totalPages = ref(1); // NEW
const loading = ref(false);
const showAddModal = ref(false);

const filters = ref({
  id: null,
  name: '',
  description: ''
});

// newProduct holds file and preview
const newProduct = ref({ description: '', salePrice: 0, imageFile: null, imagePreview: '' });
const submitting = ref(false);

// modal state for product details
const selectedProduct = ref(null);
const showModal = ref(false);

// Edit modal state
const showEditModal = ref(false);
const editProduct = ref({ id: 0, description: '', salePrice: 0, category: '', image: '' });

function openProduct(p){
  selectedProduct.value = p;
  showModal.value = true;
}
function closeModal(){
  showModal.value = false;
  selectedProduct.value = null;
}

function openEditFromDetails() {
  if (!selectedProduct.value) return;
  const p = selectedProduct.value;
  editProduct.value = {
    id: p.id,
    description: p.description ?? '',
    salePrice: Number(p.salePrice ?? 0),
    category: p.category ?? '',
    image: p.image ?? ''
  };
  showEditModal.value = true;
}

async function submitEditProduct() {
  if (!editProduct.value.id) return;
  submitting.value = true;
  try {
    await updateProduct(editProduct.value.id, {
      description: editProduct.value.description || '',
      salePrice: editProduct.value.salePrice ?? 0,
      category: editProduct.value.category || '',
      image: editProduct.value.image || null
    });
    // Refresh list and close modals
    await fetchProducts();
    showEditModal.value = false;
    showModal.value = false; // close details popup too
    selectedProduct.value = null;
  } catch (err) {
    console.error('Error updating product:', err);
    alert('Failed to update product: ' + (err?.message || 'unknown'));
  } finally {
    submitting.value = false;
  }
}

function closeEditModal() {
  showEditModal.value = false;
}

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
    const matchDesc = !filters.value.description || (product.description || '').toLowerCase().includes(filters.value.description.toLowerCase());
    return matchId && matchDesc;
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
  filters.value = { id: null, name: '', description: '' };
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
.clickable-row{ cursor:pointer; }
</style>
