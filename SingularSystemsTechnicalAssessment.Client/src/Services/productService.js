import axios from 'axios';

// Try to detect a working backend base URL.
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
    // fallback to first candidate will likely fail but let caller handle errors
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

// Create product: accepts FormData 
export async function createProduct(payload) {
  const base = await detectBaseUrl(); // e.g. http://localhost:5075/api/products

  if (payload instanceof FormData) {
    const res = await fetch(base, { method: 'POST', body: payload });

    if (!res.ok) {
      let body;
      try { body = await res.json(); }
      catch { body = await res.text().catch(()=>null); }
      throw new Error((body && body.message) || body || `Request failed ${res.status}`);
    }

    // Created with empty body return null
    const contentLength = res.headers.get('content-length');
    const contentType = (res.headers.get('content-type') || '').toLowerCase();
    if ((contentLength === '0') || (!contentType.includes('application/json') && res.status === 201)) {
      return null;
    }

    try { return await res.json(); } catch { return null; }
  } else {
    // plain object -> send JSON
    const res = await fetch(`${base}/json`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    if (!res.ok) {
      let body;
      try { body = await res.json(); }
      catch { body = await res.text().catch(()=>null); }
      throw new Error((body && body.message) || body || `Request failed ${res.status}`);
    }

    const contentLength = res.headers.get('content-length');
    const contentType = (res.headers.get('content-type') || '').toLowerCase();
    if ((contentLength === '0') || !contentType.includes('application/json')) {
      return null;
    }

    try { return await res.json(); } catch { return null; }
  }
};

export async function updateProduct(id, payload) {
  const base = await detectBaseUrl(); // e.g. http://localhost:5075/api/products
  const res = await fetch(`${base}/${encodeURIComponent(id)}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });

  if (!res.ok) {
    let body;
    try { body = await res.json(); }
    catch { body = await res.text().catch(()=>null); }
    throw new Error((body && body.message) || body || `Request failed ${res.status}`);
  }

  // No body expected on 204
  return null;
}
