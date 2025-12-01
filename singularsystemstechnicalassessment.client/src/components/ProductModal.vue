<template>
  <div class="modal-root" role="dialog" aria-modal="true" @keydown.esc="close">
    <div class="backdrop" @click="close" />
    <div class="panel" role="document" aria-labelledby="title" :aria-hidden="false">
      <header class="panel-header">
        <h2 id="title">{{ product?.description || product?.name || 'Product' }}</h2>
        <button class="close" @click="close" aria-label="Close">&times;</button>
      </header>

      <div class="panel-body">
        <div class="image-col">
          <div class="image-card">
            <img :src="product?.image || placeholder" :alt="product?.description || product?.name || 'Product image'" />
          </div>
          <div class="image-meta" v-if="product?.category">
            <span class="badge">{{ product.category }}</span>
          </div>
        </div>

        <div class="details-col">
          <dl class="details-grid">
            <div class="field">
              <dt>ID</dt>
              <dd>{{ product?.id ?? '-' }}</dd>
            </div>
            <div class="field">
              <dt>Name</dt>
              <dd>{{ product?.name ?? '-' }}</dd>
            </div>
            <div class="field">
              <dt>Sale Price</dt>
              <dd>{{ formatPrice(product?.salePrice ?? product?.price) }}</dd>
            </div>
            <div class="field">
              <dt>Category</dt>
              <dd>{{ product?.category ?? '-' }}</dd>
            </div>
            <div class="field full">
              <dt>Description</dt>
              <dd class="desc">{{ product?.description ?? '-' }}</dd>
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
import { defineProps, defineEmits } from 'vue';
const props = defineProps({ product: { type: Object, required: false } });
const emit = defineEmits(['close']);
const placeholder = '/images/placeholder.png';
function close(){ emit('close'); }
function formatPrice(v){ if (v == null || isNaN(Number(v))) return ''; return Number(v).toLocaleString(undefined, { style:'currency', currency:'USD' }); }
</script>

<style scoped>
:root{
  --modal-width: 920px;
  --panel-padding: 18px;
  --accent: #0b6fb2;
  --muted: #6b7280;
  --bg: #fff;
}

/* root / backdrop */
.modal-root{ position:fixed; inset:0; display:flex; align-items:center; justify-content:center; z-index:1400; }
.backdrop{ position:absolute; inset:0; background:linear-gradient(rgba(2,6,23,0.45), rgba(2,6,23,0.45)); backdrop-filter: blur(2px); }

/* panel */
.panel{
  position:relative;
  z-index:1401;
  width:min(var(--modal-width), calc(100% - 32px));
  max-height:90vh;
  background:#ffffff; /* ensure pure white background */
  border-radius:12px;
  box-shadow: 0 20px 60px rgba(2,6,23,0.45);
  overflow:auto;
  display:flex;
  flex-direction:column;
  transform: translateY(-6px);
  animation: popIn .14s ease-out;

  /* increased inner spacing for nicer layout */
  padding: 0; /* header/body handle internal padding */
}

/* header */
.panel-header{
  display:flex;
  align-items:center;
  justify-content:space-between;
  padding:18px 28px; /* increased */
  border-bottom:1px solid #eef2f7;
}
.panel-header h2{
  margin:0;
  font-size:1.125rem;
  font-weight:700;
  color:#0f172a;
}
.close{
  background:transparent;
  border:none;
  font-size:26px;
  line-height:1;
  color:var(--muted);
  cursor:pointer;
  padding:6px;
}

/* body layout */
.panel-body{
  display:flex;
  gap:28px;           /* increased gap between columns */
  padding:22px 28px;  /* increased padding */
  align-items:flex-start;
}

/* left image column */
.image-col{ flex:0 0 340px; display:flex; flex-direction:column; gap:14px; align-items:stretch; }
.image-card{
  width:100%;
  border-radius:12px;
  overflow:hidden;
  background:linear-gradient(180deg, #f8fafc, #fff);
  box-shadow: 0 10px 30px rgba(2,6,23,0.08);
  padding:6px; /* subtle inner padding to frame image */
}
.image-card img{
  display:block;
  width:100%;
  height:100%;
  max-height:380px; /* slightly larger */
  object-fit:cover;
  border-radius:8px;
}

/* small meta under image */
.image-meta{ display:flex; gap:8px; align-items:center; margin-top:6px; }
.badge{
  background:rgba(11,111,178,0.08);
  color:var(--accent);
  padding:8px 12px; /* increased */
  border-radius:999px;
  font-weight:700;
  font-size:13px;
}

/* right details column */
.details-col{ flex:1 1 auto; min-width:0; display:flex; flex-direction:column; gap:16px; padding:4px 0; }
.details-grid{ display:grid; grid-template-columns:repeat(2, 1fr); gap:14px; margin:0; padding:0; list-style:none; }
.details-grid .field{ display:flex; flex-direction:column; }
.details-grid .field dt{ font-size:13px; color:var(--muted); margin-bottom:8px; font-weight:700; }
.details-grid .field dd{ margin:0; font-size:16px; color:#0f172a; padding-bottom:6px; }
.details-grid .field.full{ grid-column:1 / -1; }

/* description text */
.desc{ color:#334155; line-height:1.6; white-space:pre-wrap; max-height:28vh; overflow:auto; padding:8px; border-radius:6px; background:#fbfdff; }

/* actions */
.actions{ display:flex; gap:12px; margin-top:8px; }
.btn{ padding:10px 14px; border-radius:10px; border:none; cursor:pointer; font-weight:700; }
.btn.primary{ background:var(--accent); color:white; }
.btn.secondary{ background:#f2f4f7; color:#0f172a; }

/* focus/keyboard styles */
.panel:focus{ outline:none; }
.clickable{ cursor:pointer; }

/* animation */
@keyframes popIn { from{ opacity:0; transform:translateY(-8px) scale(.995) } to{ opacity:1; transform:none } }

/* responsive */
@media (max-width:820px){
  .panel-body{ flex-direction:column; gap:18px; padding:18px; }
  .image-col{ flex:0 0 auto; width:100%; }
  .details-grid{ grid-template-columns:1fr; }
  .image-card img{ max-height:320px; }
}
</style>
