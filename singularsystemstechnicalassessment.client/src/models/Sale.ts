export interface SaleListDto {
  id: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  saleDate: string;
}
export interface SaleDetailDto {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  saleDate: string;
}
export interface SaleCreateDto {
  productId: number;
  quantity: number;
  unitPrice?: number | null;
}
export interface SaleUpdateDto {
  productId: number;
  quantity: number;
  unitPrice: number;
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
