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
  --accent: #6495ff;
  --muted: #6b7280;
  --bg: #fff;
}

/* root / backdrop */
.modal-root{ 
  position:fixed; 
  inset:0; 
  display:flex; 
  align-items:center; 
  justify-content:center; 
  z-index:1400;
  animation: fadeIn 0.3s ease-out;
}

.backdrop{ 
  position:absolute; 
  inset:0; 
  background:linear-gradient(rgba(11, 18, 32, 0.6), rgba(11, 18, 32, 0.6)); 
  backdrop-filter: blur(8px);
  transition: backdrop-filter 0.3s ease;
}

.modal-root:hover .backdrop {
  backdrop-filter: blur(10px);
}

/* panel */
.panel{
  position:relative;
  z-index:1401;
  width:min(var(--modal-width), calc(100% - 32px));
  max-height:90vh;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.95));
  border-radius:16px;
  border: 1px solid rgba(100, 150, 255, 0.1);
  box-shadow: 0 25px 80px rgba(100, 150, 255, 0.25), 0 0 40px rgba(100, 150, 255, 0.15);
  overflow:auto;
  display:flex;
  flex-direction:column;
  animation: popIn 0.4s cubic-bezier(0.36, 0, 0.66, 1);
  padding: 0;
}

/* header */
.panel-header{
  display:flex;
  align-items:center;
  justify-content:space-between;
  padding:24px 32px;
  border-bottom: 2px solid rgba(100, 150, 255, 0.1);
  background: linear-gradient(90deg, rgba(100, 150, 255, 0.05), transparent);
  animation: slideInDown 0.4s ease-out;
}

.panel-header h2{
  margin:0;
  font-size:1.35rem;
  font-weight:700;
  color: #0f172a;
  letter-spacing: -0.3px;
}

.close{
  background: rgba(100, 150, 255, 0.1);
  border: 1px solid rgba(100, 150, 255, 0.2);
  font-size:26px;
  line-height:1;
  color: #6495ff;
  cursor:pointer;
  padding:6px 10px;
  border-radius: 8px;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
}

.close:hover {
  background: rgba(255, 60, 60, 0.2);
  color: #ff3c3c;
  transform: scale(1.1) rotate(90deg);
  border-color: rgba(255, 60, 60, 0.3);
}

/* body layout */
.panel-body{
  display:flex;
  gap:32px;
  padding:28px 32px;
  align-items:flex-start;
  animation: slideInUp 0.4s ease-out 0.1s both;
}

/* left image column */
.image-col{ 
  flex:0 0 360px; 
  display:flex; 
  flex-direction:column; 
  gap:16px; 
  align-items:stretch;
  animation: slideInLeft 0.4s ease-out 0.15s both;
}

.image-card{
  width:100%;
  border-radius:14px;
  overflow:hidden;
  background: linear-gradient(135deg, #f8fafc, #f1f5f9);
  box-shadow: 0 15px 40px rgba(100, 150, 255, 0.2), 0 0 20px rgba(100, 150, 255, 0.1);
  padding: 8px;
  border: 1px solid rgba(100, 150, 255, 0.15);
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

.image-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 20px 50px rgba(100, 150, 255, 0.3), 0 0 30px rgba(100, 150, 255, 0.15);
  border-color: rgba(100, 150, 255, 0.3);
}

.image-card img{
  display:block;
  width:100%;
  height:100%;
  max-height:380px;
  object-fit:cover;
  border-radius:10px;
  transition: all 0.4s ease;
}

.image-card:hover img {
  transform: scale(1.05);
  filter: brightness(1.08);
}

/* small meta under image */
.image-meta{ 
  display:flex; 
  gap:8px; 
  align-items:center; 
  margin-top:6px;
  animation: fadeInUp 0.4s ease-out 0.25s both;
}

.badge{
  background: linear-gradient(135deg, rgba(100, 150, 255, 0.15), rgba(100, 120, 200, 0.08));
  color: #6495ff;
  padding: 8px 14px;
  border-radius: 20px;
  font-weight: 700;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border: 1px solid rgba(100, 150, 255, 0.2);
  transition: all 0.3s ease;
}

.badge:hover {
  background: linear-gradient(135deg, rgba(100, 150, 255, 0.25), rgba(100, 120, 200, 0.15));
  border-color: rgba(100, 150, 255, 0.4);
  transform: scale(1.05);
}

/* right details column */
.details-col{ 
  flex:1 1 auto; 
  min-width:0; 
  display:flex; 
  flex-direction:column; 
  gap:16px; 
  padding:4px 0;
  animation: slideInRight 0.4s ease-out 0.15s both;
}

.details-grid{ 
  display:grid; 
  grid-template-columns:repeat(2, 1fr); 
  gap: 18px;
  margin:0; 
  padding:0; 
  list-style:none;
}

.details-grid .field{ 
  display:flex; 
  flex-direction:column;
  padding: 12px;
  border-radius: 8px;
  background: linear-gradient(135deg, rgba(100, 150, 255, 0.05), transparent);
  border: 1px solid rgba(100, 150, 255, 0.1);
  transition: all 0.3s ease;
  animation: fadeInUp 0.4s ease-out forwards;
}

.details-grid .field:nth-child(1) { animation-delay: 0.2s; }
.details-grid .field:nth-child(2) { animation-delay: 0.25s; }
.details-grid .field:nth-child(3) { animation-delay: 0.3s; }
.details-grid .field:nth-child(4) { animation-delay: 0.35s; }
.details-grid .field.full { animation-delay: 0.4s; }

.details-grid .field:hover {
  background: linear-gradient(135deg, rgba(100, 150, 255, 0.12), rgba(100, 120, 200, 0.05));
  border-color: rgba(100, 150, 255, 0.25);
  transform: translateX(4px);
}

.details-grid .field dt{ 
  font-size: 12px; 
  color: #6495ff;
  margin-bottom:8px; 
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.details-grid .field dd{ 
  margin:0; 
  font-size: 16px; 
  color: #0f172a; 
  padding-bottom:6px;
  font-weight: 600;
}

.details-grid .field.full{ 
  grid-column:1 / -1; 
}

/* description text */
.desc{ 
  color: #334155; 
  line-height:1.6; 
  white-space:pre-wrap; 
  max-height:28vh; 
  overflow:auto; 
  padding: 12px;
  border-radius: 8px; 
  background: linear-gradient(135deg, #fbfdff, #f8fafc);
  border: 1px solid rgba(100, 150, 255, 0.1);
}

/* actions */
.actions{ 
  display:flex; 
  gap: 12px; 
  margin-top: 12px;
  animation: fadeInUp 0.4s ease-out 0.45s both;
}

.btn{ 
  padding: 12px 18px; 
  border-radius: 10px; 
  border: none; 
  cursor: pointer; 
  font-weight: 700;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  font-size: 0.85rem;
  position: relative;
  overflow: hidden;
}

.btn::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  width: 0;
  height: 0;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.3);
  transform: translate(-50%, -50%);
  transition: width 0.6s, height 0.6s;
  z-index: 0;
}

.btn:hover::before {
  width: 300px;
  height: 300px;
}

.btn.primary{ 
  background: linear-gradient(135deg, #6495ff, #5080d0);
  color: white;
  box-shadow: 0 6px 20px rgba(100, 150, 255, 0.3);
}

.btn.primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 30px rgba(100, 150, 255, 0.5);
}

.btn.secondary{ 
  background: rgba(100, 150, 255, 0.1);
  color: #6495ff;
  border: 1px solid rgba(100, 150, 255, 0.2);
}

.btn.secondary:hover {
  background: rgba(100, 150, 255, 0.2);
  border-color: rgba(100, 150, 255, 0.4);
  transform: translateY(-2px);
}

/* focus/keyboard styles */
.panel:focus{ outline:none; }
.clickable{ cursor:pointer; }

/* animations */
@keyframes popIn { 
  from{ 
    opacity:0; 
    transform:translateY(-12px) scale(0.95) 
  } 
  to{ 
    opacity:1; 
    transform:none 
  } 
}

@keyframes slideInDown {
  from {
    opacity: 0;
    transform: translateY(-15px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slideInUp {
  from {
    opacity: 0;
    transform: translateY(15px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slideInLeft {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

/* responsive */
@media (max-width:820px){
  .panel-body{ 
    flex-direction:column; 
    gap:20px; 
    padding: 20px;
  }
  
  .image-col{ 
    flex:0 0 auto; 
    width:100%; 
  }
  
  .details-grid{ 
    grid-template-columns:1fr; 
  }
  
  .image-card img{ 
    max-height:320px; 
  }
  
  .panel-header {
    padding: 16px 20px;
  }
  
  .panel-header h2 {
    font-size: 1.1rem;
  }
}
</style>
