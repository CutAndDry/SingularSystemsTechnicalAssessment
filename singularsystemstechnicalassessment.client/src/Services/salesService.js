import axios from "axios";

const API_BASE = "http://localhost:5075/api/sales"; // changed to http:// to match backend protocol

export function getSales(pageNumber = 1, pageSize = 10, productId = null, startDate = null, endDate = null) {
  return axios.get(`${API_BASE}/GetAllPagination`, {
    params: {
      pageNumber,
      pageSize,
      productId,
      startDate,
      endDate
    }
  });
}

// fallback to non-paginated list if needed
export function getAllSales() {
  return axios.get(API_BASE);
}

export const addSale = (sale) => {
  return axios.post(API_BASE, sale);
};
