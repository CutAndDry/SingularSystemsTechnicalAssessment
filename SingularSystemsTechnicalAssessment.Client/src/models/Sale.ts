export interface SaleListDto {
  id: number;
  productName: string;
  saleQty: number;
  salePrice: number;
  saleDate: string;
}
export interface SaleDetailDto {
  id: number;
  productId: number;
  productName: string;
  
  saleQty?: number;
  quantity?: number;

  salePrice?: number;
  unitPrice?: number;

  saleDate?: string;
  date?: string;
}
export interface SaleCreateDto {
  productId: number;
  saleQty: number;
  salePrice?: number | null;
}
export interface SaleUpdateDto {
  productId: number;
  saleQty: number;
  salePrice: number;
  saleDate: string;
}
export interface SalesPagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
