import axios from "axios";

const API_URL = "https://localhost:7110/api/sales";

export function getSales(pageNumber, pageSize, productId, startDate, endDate) {
  return axios.get("https://localhost:7110/api/Sales/GetAllPagination", {
    params: {
      pageNumber,
      pageSize,
      productId,
      startDate,
      endDate
    }
  });
}
export const addSale = (sale) => {
  return axios.post(API_URL, sale);
};
