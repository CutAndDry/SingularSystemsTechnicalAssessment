export interface ProductListDto {
  id: number;
  name: string;
  price: number;
  totalSales: number;
  totalRevenue: number;
}

import type { SaleDetailDto } from "./Sale";

export interface ProductDetailDto {
  id: number;
  name: string;
  description?: string | null;
  price: number;
  totalSales: number;
  totalRevenue: number;
  sales: SaleDetailDto[];
}
export interface ProductCreateDto {
  name: string;
  description?: string | null;
  price: number;
}
export interface ProductUpdateDto {
  name: string;
  description?: string | null;
  price: number;
}
export interface ProductPagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;

  totalCount: number;
  totalPages: number;

  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
