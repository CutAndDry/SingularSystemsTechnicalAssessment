import axios from "axios";

// Reuse the same detection strategy as productService: probe likely backend URLs and cache a working base.
const CANDIDATES = [
  (import.meta.env && import.meta.env.VITE_API_URL) || null,
  'http://localhost:5075',
  'http://localhost:5074',
  (typeof window !== 'undefined' && window.location && window.location.origin) || null
].filter(Boolean);

let _detectedBase = null;
let _detectPromise = null;

async function detectBase() {
  if (_detectedBase) return _detectedBase;
  if (_detectPromise) return _detectPromise;

  _detectPromise = (async () => {
    for (const candidate of CANDIDATES) {
      try {
        const probe = `${candidate.replace(/\/$/, '')}/api/seedstatus`;
        const r = await axios.get(probe, { timeout: 2000 });
        if (r && r.status === 200) {
          _detectedBase = candidate.replace(/\/$/, '') + '/api/sales';
          return _detectedBase;
        }
      } catch (e) {
        // ignore and try next
      }
    }
    _detectedBase = CANDIDATES[0].replace(/\/$/, '') + '/api/sales';
    return _detectedBase;
  })();

  return _detectPromise;
}

export async function getSales(pageNumber = 1, pageSize = 10, productId = null, startDate = null, endDate = null) {
  const base = await detectBase();
  return axios.get(`${base}/GetAllPagination`, {
    params: { pageNumber, pageSize, productId, startDate, endDate }
  });
}

export async function getAllSales() {
  const base = await detectBase();
  return axios.get(base);
}

export const addSale = async (sale) => {
  const base = await detectBase();
  return axios.post(base, sale);
};
