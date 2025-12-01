export interface ProductListDto {
  id: number;
  description?: string | null;
  salePrice: number;
  totalSales: number;
  totalRevenue: number;
}

import type { SaleDetailDto } from "./Sale";

export interface ProductDetailDto {
  id: number;
  description?: string | null;
  salePrice: number;
  totalSales: number;
  totalRevenue: number;
  sales: SaleDetailDto[];
}

export interface ProductCreateDto {
  description?: string | null;
  salePrice: number;
  category?: string | null;
  image?: string | null;
}

export interface ProductUpdateDto {
  description?: string | null;
  salePrice: number;
  category?: string | null;
  image?: string | null;
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
