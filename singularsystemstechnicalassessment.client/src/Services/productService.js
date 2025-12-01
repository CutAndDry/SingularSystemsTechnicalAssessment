import axios from 'axios';

// Try to detect a working backend base URL. Order is:
// 1) Vite env var VITE_API_URL (if set)
// 2) http://localhost:5075
// 3) http://localhost:5074
// 4) window.location.origin (same origin)
const CANDIDATES = [
  (import.meta.env && import.meta.env.VITE_API_URL) || null,
  'http://localhost:5075',
  'http://localhost:5074',
  (typeof window !== 'undefined' && window.location && window.location.origin) || null
].filter(Boolean);

let _detectedBase = null;
let _detectPromise = null;

async function detectBaseUrl() {
  if (_detectedBase) return _detectedBase;
  if (_detectPromise) return _detectPromise;

  _detectPromise = (async () => {
    for (const candidate of CANDIDATES) {
      try {
        // probe seedstatus to minimize CORS issues
        const probeUrl = `${candidate.replace(/\/$/, '')}/api/seedstatus`;
        const resp = await axios.get(probeUrl, { timeout: 2000 });
        if (resp && resp.status === 200) {
          _detectedBase = candidate.replace(/\/$/, '') + '/api/products';
          return _detectedBase;
        }
      } catch (e) {
        // ignore and try next
      }
    }
    // fallback to first candidate (will likely fail but let caller handle errors)
    _detectedBase = CANDIDATES[0].replace(/\/$/, '') + '/api/products';
    return _detectedBase;
  })();

  return _detectPromise;
}

// paginated products
export const getAllProducts = async (pageNumber = 1, pageSize = 10) => {
  const base = await detectBaseUrl();
  return axios.get(`${base}/GetAllPagination`, {
    params: { pageNumber, pageSize }
  });
};

// Create product
export const createProduct = async (product) => {
  const base = await detectBaseUrl();
  return axios.post(base, product);
};
