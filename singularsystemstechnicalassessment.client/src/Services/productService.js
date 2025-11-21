import axios from 'axios';

const API_URL = 'https://localhost:7110/api/products';

//  paginated products
export const getAllProducts = (pageNumber = 1, pageSize = 10) => {
  return axios.get(`${API_URL}/GetAllPagination`, {
    params: { pageNumber, pageSize }
  });
};

// Create product
export const createProduct = (product) => {
  return axios.post(API_URL, product);
};
