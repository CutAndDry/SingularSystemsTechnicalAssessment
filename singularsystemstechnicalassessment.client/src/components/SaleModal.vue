<template>
  <div class="modal-root" role="dialog" aria-modal="true" @keydown.esc.window="close">
    <div class="backdrop" @click="close"></div>
    <div class="panel" role="document" aria-labelledby="title">
      <header class="panel-header">
        <h2 id="title">Sale Details</h2>
        <button class="close" @click="close" aria-label="Close">&times;</button>
      </header>

      <div class="panel-body">
        <div class="image-col" v-if="sale?.product?.image || sale?.image">
          <div class="image-card">
            <img :src="sale?.product?.image ?? sale?.image ?? placeholder" :alt="sale?.product?.name ?? 'Product image'"/>
          </div>
        </div>

        <div class="details-col">
          <dl class="details-grid">
            <div class="field">
              <dt>Sale ID</dt>
              <dd>{{ sale?.id ?? '-' }}</dd>
            </div>
            <div class="field">
              <dt>Product</dt>
              <dd>{{ sale?.product?.name ?? sale?.productName ?? '-' }}</dd>
            </div>
            <div class="field">
              <dt>Quantity</dt>
              <dd>{{ sale?.quantity ?? sale?.qty ?? '-' }}</dd>
            </div>
            <div class="field">
              <dt>Unit Price</dt>
              <dd>{{ formatPrice(sale?.unitPrice ?? sale?.price) }}</dd>
            </div>
            <div class="field">
              <dt>Total</dt>
              <dd>{{ formatPrice(sale?.total ?? computeTotal()) }}</dd>
            </div>
            <div class="field full">
              <dt>Date</dt>
              <dd>{{ formatDate(sale?.date) }}</dd>
            </div>
            <div class="field full" v-if="sale?.notes">
              <dt>Notes</dt>
              <dd class="desc">{{ sale.notes }}</dd>
            </div>
          </dl>

          <div class="actions">
            <button class="btn primary" @click="close">Close</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  sale: { type: Object, required: false }
});
const emit = defineEmits(['close']);
const placeholder = '/images/placeholder.png';

function close(){ emit('close'); }

function formatPrice(v){
  if (v == null || isNaN(Number(v))) return '-';
  return Number(v).toLocaleString(undefined, { style:'currency', currency:'USD' });
}
function formatDate(d){
  if (!d) return '-';
  const dt = new Date(d);
  return isNaN(dt.getTime()) ? String(d) : dt.toLocaleString();
}
function computeTotal(){
  const q = Number(props.sale?.quantity ?? props.sale?.qty ?? 0);
  const p = Number(props.sale?.unitPrice ?? props.sale?.price ?? 0);
  return q * p;
}
</script>

<style scoped>
.modal-root{ position:fixed; inset:0; display:flex; align-items:center; justify-content:center; z-index:1400; }
.backdrop{ position:absolute; inset:0; background:rgba(0,0,0,0.45); }
.panel{ position:relative; z-index:1401; width:min(900px, calc(100% - 32px)); max-height:90vh; background:#fff; border-radius:10px; box-shadow:0 20px 60px rgba(0,0,0,0.25); overflow:auto; display:flex; flex-direction:column; }
.panel-header{ display:flex; align-items:center; justify-content:space-between; padding:16px 20px; border-bottom:1px solid #eef2f7; }
.panel-header h2{ margin:0; font-size:1.1rem; }
.close{ background:transparent; border:none; font-size:26px; cursor:pointer; color:#6b7280; }
.panel-body{ display:flex; gap:20px; padding:20px; align-items:flex-start; }
.image-col{ flex:0 0 300px; }
.image-card{ border-radius:8px; overflow:hidden; background:#f8fafc; box-shadow:0 8px 24px rgba(2,6,23,0.06); padding:6px; }
.image-card img{ width:100%; height:100%; max-height:360px; object-fit:cover; display:block; border-radius:6px; }
.details-col{ flex:1; display:flex; flex-direction:column; gap:12px; }
.details-grid{ display:grid; grid-template-columns:repeat(2, 1fr); gap:12px; margin:0; padding:0; }
.field dt{ font-size:12px; color:#6b7280; margin-bottom:6px; font-weight:700; }
.field dd{ margin:0; font-size:15px; color:#0f172a; }
.field.full{ grid-column:1 / -1; }
.desc{ background:#fbfdff; padding:8px; border-radius:6px; max-height:20vh; overflow:auto; }
.actions{ display:flex; justify-content:flex-end; gap:8px; margin-top:8px; }
.btn{ padding:8px 12px; border-radius:8px; border:none; cursor:pointer; font-weight:700; }
.btn.primary{ background:#0b6fb2; color:white; }
@media (max-width:820px){ .panel-body{ flex-direction:column; } .image-col{ width:100%; } .details-grid{ grid-template-columns:1fr; } .image-card img{ max-height:260px; } }
</style>
