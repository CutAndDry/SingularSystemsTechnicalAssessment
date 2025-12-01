<template>
  <div class="sales-page">
    <header class="page-header">
      <h1>Sales</h1>
    </header>

    <!-- Filter Panel -->
    <div class="filter-panel">
      <div class="filter-row">
        <div class="filter-group">
          <label>Product</label>
          <input v-model="filters.productDescription" type="text" placeholder="Filter by product" />
        </div>
        <div class="filter-group">
          <label>Start Date</label>
          <input v-model="filters.startDate" type="date" />
        </div>
        <div class="filter-group">
          <label>End Date</label>
          <input v-model="filters.endDate" type="date" />
        </div>
        <div class="filter-actions">
          <button class="btn btn-primary" @click="applyFilters">Apply Filters</button>
          <button class="btn btn-outline" @click="resetFilters">Reset</button>
          <button class="btn btn-primary" @click="showAddModal = true">+ Add Sale</button>
        </div>
      </div>
    </div>

    <!-- Add Sale Modal -->
    <div v-if="showAddModal" class="modal-overlay" @click.self="closeAddModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Add New Sale</h3>
          <button class="modal-close" @click="closeAddModal">&times;</button>
        </div>
        <form @submit.prevent="submitAddSale" class="modal-form">
          <div class="form-group">
            <label>Product ID *</label>
            <input v-model.number="newSale.productId" type="number" required placeholder="Enter Product ID" />
          </div>
          <div class="form-group">
            <label>Quantity *</label>
            <input v-model.number="newSale.saleQty" type="number" min="1" required placeholder="Enter quantity" />
          </div>
          <div class="form-group">
            <label>Unit Price (optional)</label>
            <input v-model.number="newSale.salePrice" type="number" step="0.01" placeholder="Enter unit price" />
          </div>
          <div class="modal-actions">
            <button type="submit" class="btn btn-success">Add Sale</button>
            <button type="button" class="btn btn-secondary" @click="closeAddModal">Cancel</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Sales Table -->
    <div class="table-container">
      <div class="table-wrapper">
        <table class="sales-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Product</th>
              <th>Quantity</th>
              <th>Unit Price</th>
              <th>Total</th>
              <th>Date</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" v-for="i in 8" :key="'skeleton-'+i" class="skeleton-row">
              <td colspan="6">&nbsp;</td>
            </tr>

            <!-- clickable sale rows -->
            <tr
              v-for="sale in sales"
              :key="sale.id"
              tabindex="0"
              role="button"
              class="clickable-row"
              @click="openSale(sale)"
              @keydown.enter.prevent="openSale(sale)"
              @keydown.space.prevent="openSale(sale)"
            >
              <td>{{ sale.id }}</td>
              <td>{{ sale.product?.description ?? sale.productName ?? '-' }}</td>
              <td>{{ sale.saleQty ?? sale.quantity ?? sale.qty ?? '-' }}</td>
              <td>{{ formatPrice(sale.salePrice ?? sale.unitPrice ?? sale.price) }}</td>
              <td>{{ formatPrice((sale.saleQty ?? sale.quantity ?? sale.qty ?? 0) * (sale.salePrice ?? sale.unitPrice ?? sale.price ?? 0)) }}</td>
              <td>{{ formatDate(sale.saleDate ?? sale.date) }}</td>
            </tr>

            <tr v-if="!loading && sales.length === 0">
              <td colspan="6" class="text-center">No sales found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- sale modal (inline so Edit button sits inside the popup) -->
    <div v-if="showSaleModal" class="modal-overlay" @click.self="closeSaleModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Sale Details</h3>
          <button class="modal-close" @click="closeSaleModal">&times;</button>
        </div>

        <div class="modal-form" style="padding:1rem;">
          <div style="display:flex;gap:1.5rem;align-items:flex-start;">
            <div style="flex:1">
              <p><strong>ID:</strong> {{ selectedSale?.id ?? '-' }}</p>
              <p><strong>Product ID:</strong> {{ selectedSale?.productId ?? '-' }}</p>
              <p><strong>Product:</strong> {{ selectedSale?.product?.description ?? selectedSale?.productName ?? '-' }}</p>
              <p><strong>Quantity:</strong> {{ selectedSale?.saleQty ?? selectedSale?.quantity ?? '-' }}</p>
              <p><strong>Unit Price:</strong> {{ formatPrice(selectedSale?.salePrice ?? selectedSale?.unitPrice ?? null) }}</p>
              <p><strong>Date:</strong> {{ formatDate(selectedSale?.saleDate ?? selectedSale?.date) }}</p>
            </div>
          </div>
        </div>

        <div class="modal-actions" style="justify-content:flex-end; padding:1rem;">
          <button class="btn btn-primary" @click="openEditSaleFromDetails">Edit</button>
          <button class="btn btn-secondary" @click="closeSaleModal">Close</button>
        </div>
      </div>
    </div>

    <!-- Edit Sale Modal -->
    <div v-if="showEditSaleModal" class="modal-overlay" @click.self="closeEditModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Edit Sale</h3>
          <button class="modal-close" @click="closeEditModal">&times;</button>
        </div>
        <form @submit.prevent="submitEditSale" class="modal-form">
          <div class="form-group">
            <label>Product *</label>
            <select v-model.number="editSale.productId" required>
              <option :value="null" disabled>Select product</option>
              <option v-for="p in productsList" :key="p.id" :value="p.id">
                {{ p.description || ('Product ' + p.id) }}
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Quantity *</label>
            <input v-model.number="editSale.saleQty" type="number" min="1" required />
          </div>
          <div class="form-group">
            <label>Unit Price *</label>
            <input v-model.number="editSale.salePrice" type="number" step="0.01" required />
          </div>
          <div class="form-group">
            <label>Sale Date *</label>
            <input v-model="editSale.saleDateLocal" type="datetime-local" required />
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
      <button class="pagination-btn" :disabled="!pageInfo.hasPreviousPage" @click="prevPage">‹ Previous</button>
      <span class="pagination-info">Page {{ pageInfo.pageNumber }} of {{ pageInfo.totalPages }}</span>
      <button class="pagination-btn" :disabled="!pageInfo.hasNextPage" @click="nextPage">Next ›</button>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import { getSales, getAllSales, addSale, updateSale } from "../Services/salesService";
import { getAllProducts } from "../Services/productService"; // added

export default {
  name: "Sales",
  data() {
    return {
      sales: [],
      pageInfo: {
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false
      },
      pageNumber: 1,
      pageSize: 10,
      showAddModal: false,
      loading: false,
      filters: {
        productDescription: '',
        startDate: new Date(new Date().getFullYear(), 0, 1).toISOString().split('T')[0],
        endDate: new Date(new Date().getFullYear(), 11, 31).toISOString().split('T')[0]
      },
      newSale: {
        productId: null,
        saleQty: 1,
        salePrice: null
      },
      // modal state
      selectedSale: null,
      showSaleModal: false,
      // edit sale modal
      showEditSaleModal: false,
      editSale: { id: 0, productId: null, saleQty: 1, salePrice: null, saleDateLocal: '' },
      submitting: false,
      productsList: [], // NEW: product dropdown data
    };
  },

  methods: {
    async fetchSales() {
      this.loading = true;
      try {
        // Convert product description to product ID
        let productIdFilter = null;
        if (this.filters.productDescription && this.productsList.length > 0) {
          const matchedProduct = this.productsList.find(p => 
            (p.description || '').toLowerCase().includes(this.filters.productDescription.toLowerCase())
          );
          productIdFilter = matchedProduct ? matchedProduct.id : null;
        }

        const res = await getSales(
          this.pageNumber,
          this.pageSize,
          productIdFilter,
          this.filters.startDate || null,
          this.filters.endDate || null
        );

        const data = res.data || {};
        const items = data.items || data.Items || [];
        const pageNumber = data.pageNumber ?? data.PageNumber ?? this.pageNumber;
        const pageSize = data.pageSize ?? data.PageSize ?? this.pageSize;
        const totalPages = data.totalPages ?? data.TotalPages ?? Math.max(1, Math.ceil((data.totalCount ?? items.length) / (pageSize || 10)));

        if (pageNumber === 1 && items.length === 0) {
          try {
            const fullRes = await getAllSales();
            const fullItems = (fullRes.data || []);
            this.sales = fullItems;
            this.pageInfo = {
              pageNumber: 1,
              pageSize: fullItems.length || pageSize,
              totalPages: 1,
              hasNextPage: false,
              hasPreviousPage: false
            };
            return;
          } catch (fallbackErr) {
            console.warn("Fallback full sales fetch failed", fallbackErr);
          }
        }

        this.sales = items;
        this.pageInfo = {
          pageNumber: pageNumber || 1,
          pageSize: pageSize || 10,
          totalPages: totalPages || 1,
          hasNextPage: (pageNumber ?? 1) < (totalPages || 1),
          hasPreviousPage: (pageNumber ?? 1) > 1
        };
      } catch (err) {
        console.error("Error fetching sales:", err);
      } finally {
        this.loading = false;
      }
    },

    applyFilters() {
      this.pageNumber = 1;
      this.fetchSales();
    },

    resetFilters() {
      this.filters = { 
        productDescription: '', 
        startDate: new Date(new Date().getFullYear(), 0, 1).toISOString().split('T')[0],
        endDate: new Date(new Date().getFullYear(), 11, 31).toISOString().split('T')[0]
      };
      this.pageNumber = 1;
      this.fetchSales();
    },

    async submitAddSale() {
      try {
        await addSale(this.newSale);
        this.newSale = { productId: null, saleQty: 1, salePrice: null };
        this.showAddModal = false;
        this.fetchSales();
      } catch (err) {
        console.error("Error adding sale:", err);
      }
    },

    closeAddModal() {
      this.showAddModal = false;
    },

    nextPage() {
      if (this.pageInfo.hasNextPage) {
        this.pageNumber++;
        this.fetchSales();
      }
    },

    prevPage() {
      if (this.pageInfo.hasPreviousPage) {
        this.pageNumber--;
        this.fetchSales();
      }
    },

    openSale(sale) {
      this.selectedSale = sale;
      this.showSaleModal = true;
    },

    closeSaleModal() {
      this.showSaleModal = false;
      this.selectedSale = null;
    },

    openEditSaleFromDetails() {
      if (!this.selectedSale) return;
      const s = this.selectedSale;
      const dt = s.saleDate ? new Date(s.saleDate) : (s.date ? new Date(s.date) : new Date());
      const local = new Date(dt.getTime() - dt.getTimezoneOffset() * 60000).toISOString().slice(0,16);
      this.editSale = {
        id: s.id,
        productId: s.productId ?? (s.product && s.product.id) ?? null, // prefill id
        saleQty: s.saleQty ?? s.quantity ?? 1,
        salePrice: Number(s.salePrice ?? s.unitPrice ?? 0),
        saleDateLocal: local
      };
      // ensure products list available
      if (!this.productsList || this.productsList.length === 0) {
        this.fetchProductsList();
      }
      this.showEditSaleModal = true;
    },

    closeEditModal() {
      this.showEditSaleModal = false;
    },

    async submitEditSale() {
      if (!this.editSale.id) return;
      if (this.editSale.productId == null) {
        alert("Please select a product.");
        return;
      }
      this.submitting = true;
      try {
        const iso = new Date(this.editSale.saleDateLocal).toISOString();
        await updateSale(this.editSale.id, {
          productId: this.editSale.productId,
          saleQty: this.editSale.saleQty,
          salePrice: this.editSale.salePrice,
          saleDate: iso
        });
        await this.fetchSales();
        this.showEditSaleModal = false;
        this.showSaleModal = false;
        this.selectedSale = null;
      } catch (err) {
        console.error('Error updating sale:', err);
        alert('Failed to update sale: ' + (err?.message || 'unknown'));
      } finally {
        this.submitting = false;
      }
    },

    async fetchProductsList() { // NEW
      try {
        const res = await getAllProducts(1, 1000); // large page to get all
        const data = res.data || {};
        this.productsList = data.items || data.Items || [];
      } catch (err) {
        console.warn("Failed to load products for dropdown", err);
        this.productsList = [];
      }
    },

    formatPrice(v) {
      if (v == null || isNaN(Number(v))) return '-';
      return Number(v).toLocaleString(undefined, { style:'currency', currency:'USD' });
    },

    formatDate(d) {
      if (!d) return '-';
      const dt = new Date(d);
      return isNaN(dt.getTime()) ? String(d) : dt.toLocaleString();
    }
  },

  mounted() {
    this.fetchSales();
    this.fetchProductsList(); // NEW: load products for selects
  },

  computed: {
    // Remove local slicing; rely on server pagination
  }
};
</script>

<style scoped>
.sales-page {
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
  flex-wrap: wrap;
  gap: 1rem;
  align-items: flex-end;
}

.filter-group {
  flex: 1;
  min-width: 160px;
}

.filter-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
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
  gap: 0.5rem;
  flex-wrap: wrap;
}

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

.btn-outline {
  background: transparent;
  color: #2c3e50;
  border: 1px solid #bdc3c7;
}

.btn-outline:hover {
  background: #f8f9fa;
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

.sales-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.sales-table thead {
  background: #f8f9fa;
  border-bottom: 2px solid #e6e6e6;
}

.sales-table th {
  padding: 1rem;
  text-align: left;
  font-weight: 600;
  color: #2c3e50;
}

.sales-table td {
  padding: 0.8rem 1rem;
  border-bottom: 1px solid #e6e6e6;
}

.sales-table tbody tr:hover {
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
  .sales-page {
    margin-left: 64px;
    padding: 1rem;
  }

  .filter-row {
    flex-direction: column;
  }

  .filter-group {
    min-width: unset;
  }

  .filter-actions {
    width: 100%;
    justify-content: flex-start;
  }

  .sales-table {
    font-size: 12px;
  }

  .sales-table th,
  .sales-table td {
    padding: 0.6rem 0.4rem;
  }

  .page-header h1 {
    font-size: 1.5rem;
  }
}

.clickable-row {
  cursor: pointer;
}
</style>

<style scoped>
@media (max-width: 900px) {
  .sales-page {
    margin-left: 0 !important;
    padding-top: calc(var(--mobile-topbar-height, 56px) + 1rem);
    padding-left: 1rem;
    padding-right: 1rem;
  }
}
</style>
