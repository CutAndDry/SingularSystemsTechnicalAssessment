import axios from "axios";

const API_URL = "https://localhost:7110/api/sales";

export const getSales = (pageNumber = 1, pageSize = 10) => {
  return axios.get(`${API_URL}/GetAllPagination`, {
    params: { pageNumber, pageSize }
  });
};

export const addSale = (sale) => {
  return axios.post(API_URL, sale);
};
