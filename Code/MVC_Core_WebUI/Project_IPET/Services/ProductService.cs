using Dapper;
using Project_IPET.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Project_IPET.Services
{
    public class ProductService : IProductService
    {
        private IDbConnection _dbConnection;
        public ProductService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public ProductListResponseModel GetProductList(ProductListRequestModel request)
        {
            ProductListResponseModel result = new ProductListResponseModel()
            {
                ProductList = new List<ProductModel>(),
                Pagination = request.Pagination,
            };
            try
            {
                #region
                //======================================================
                //SQL GROUP BY AND ORDER BY 指令
                //!!!!!!!@@@(但無法與FETCH同時使用)@@@!!!!!!!!!!!!!!
                /*SELECT 評分=avg(cm.Rating), p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.CostPrice,p.UnitPrice, p.UnitsInStock, p.Description, p.HotProduct, p.ProductAvailable, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName 
                                            FROM Products p 
                                            JOIN SubCategories sc ON p.SubCategoryID =sc.SubCategoryID 
                                            JOIN Categories c ON sc.CategoryID = c.CategoryID 
                                            LEFT JOIN  ProductImagePath pp ON p.ProductID =pp.ProductID
                                            JOIN Brand b ON p.BrandID = b.BrandID
                                            JOIN Comment cm ON p.ProductID = cm.ProductID
                                            WHERE pp.IsMainImage = 1
                                            GROUP BY p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.CostPrice,p.UnitPrice, p.UnitsInStock, p.Description, p.HotProduct, p.ProductAvailable, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName 
                                            ORDER BY avg(cm.Rating) ASC*/
                //======================================================
                //string sql = @"SELECT COUNT(1)
                //                                FROM Products p 
                //                                JOIN SubCategories sc ON p.SubCategoryID =sc.SubCategoryID 
                //                                JOIN Categories c ON sc.CategoryID = c.CategoryID 
                //                                LEFT JOIN  ProductImagePath pp ON p.ProductID =pp.ProductID
                //                                JOIN Brand b ON p.BrandID = b.BrandID
                //                                JOIN Comment cm ON p.ProductID = cm.ProductID
                //                                WHERE pp.IsMainImage = 1 ";

                //if (request.CategoryId != -1)
                //{
                //    sql += " AND c.CategoryID = @CategoryID";
                //}
                //var param = new
                //{
                //    CategoryID = request.CategoryId,
                //    PageSize = request.Pagination.PageSize,
                //    Page = request.Pagination.Page,
                //};
                //result.Pagination.TotalRecord = (int)_dbConnection.ExecuteScalar(string.Format(sql), param); //拿第一個cell

                //string sql2 = @"SELECT p.*,sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName,cm.Rating
                //                                FROM Products p 
                //                                JOIN SubCategories sc ON p.SubCategoryID =sc.SubCategoryID 
                //                                JOIN Categories c ON sc.CategoryID = c.CategoryID 
                //                                LEFT JOIN  ProductImagePath pp ON p.ProductID =pp.ProductID
                //                                JOIN Brand b ON p.BrandID = b.BrandID
                //                                JOIN Comment cm ON p.ProductID = cm.ProductID
                //                                WHERE pp.IsMainImage = 1 ";
                //if (request.CategoryId != -1)
                //{
                //    sql += " AND c.CategoryID = @CategoryID";
                //}
                //var param = new
                //{
                //    CategoryID = request.CategoryId,
                //    PageSize = request.Pagination.PageSize,
                //    Page = request.Pagination.Page,
                //};
                //result.Pagination.TotalRecord = (int)_dbConnection.ExecuteScalar(string.Format(sql, column), param); //拿第一個cell

                //column = "p.*,sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName,cm.Rating";
                //sql += " ORDER BY p.ProductID OFFSET @PageSize*(@Page-1) ROWS FETCH NEXT @PageSize ROWS ONLY;";

                //result.ProductList = _dbConnection.Query<ProductModel>(string.Format(sql, column), param).ToList();

                #endregion
                #region 沒有groupby前的指令(但rating會出錯，因為一個商品可能有多個評價導致顯示出錯)
                string sql = @"SELECT avg(ISNULL(cm.Rating,0)) Rating, p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.CostPrice,p.UnitPrice, p.UnitsInStock, p.Description, p.HotProduct, p.ProductAvailable, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName, COUNT(1) OVER () AS TotalRecord
FROM Products p 
JOIN SubCategories sc ON p.SubCategoryID =sc.SubCategoryID 
JOIN Categories c ON sc.CategoryID = c.CategoryID 
JOIN Brand b ON p.BrandID = b.BrandID
LEFT JOIN  ProductImagePath pp ON p.ProductID =pp.ProductID
LEFT JOIN Comment cm ON p.ProductID = cm.ProductID
WHERE pp.IsMainImage = 1 {0}
GROUP BY p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.CostPrice,p.UnitPrice, p.UnitsInStock, p.Description, p.HotProduct, p.ProductAvailable, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName 
{1} OFFSET @PageSize*(@Page-1) ROWS FETCH NEXT @PageSize ROWS ONLY";


                string where = "";
                if (request.CategoryId != -1)
                {
                    where += " AND c.CategoryID = @CategoryID";
                }
                if (request.SubCategoryId != -1)
                {
                    where += " AND sc.SubCategoryID = @SubCategoryID";
                }
                if (!string.IsNullOrWhiteSpace(request.ProductName))
                {
                    where += " AND p.ProductName LIKE '%'+@ProductName+'%'";
                }
                if (request.BrandIds != null && request.BrandIds.Count > 0)
                {
                    where += " AND b.BrandID IN @BrandIDs ";
                }
                var param = new
                {
                    CategoryID = request.CategoryId,
                    SubCategoryID = request.SubCategoryId,
                    ProductName = request.ProductName,
                    BrandIDs = request.BrandIds,
                    PageSize = request.Pagination.PageSize,
                    Page = request.Pagination.Page,
                };


                string orderBy = "";
                switch (request.SortBy)
                {
                    case Enum.SortBy.Default:
                        orderBy += " ORDER BY p.ProductID ASC  ";
                        break;
                    case Enum.SortBy.HighPrice:
                        orderBy += " ORDER BY p.UnitPrice DESC ";
                        break;
                    case Enum.SortBy.LowPrice:
                        orderBy += " ORDER BY p.UnitPrice ASC   ";
                        break;
                    //=====================================
                    //TODO:
                    //1. 尚未撈出商品排名
                    //2. 並且排序(GROUP BY)+算商品評價平均(AVERAGE)顯示在畫面上
                    //3. 還要判斷該商品若未被評價(ProductID=1未被評價)，必須照樣顯示在ProductList (if判斷Rating==null...?)
                    case Enum.SortBy.HighRated:
                        orderBy += " ORDER BY Rating DESC ";
                        break;
                    case Enum.SortBy.LowRated:
                        orderBy += " ORDER BY Rating ASC ";
                        break;
                    //====================================
                    default:
                        orderBy += " ORDER BY p.ProductID ASC  ";
                        break;
                }

                var tempResult = _dbConnection.Query(string.Format(sql, where, orderBy), param);


                result.Pagination.TotalRecord = tempResult.FirstOrDefault().TotalRecord;
                result.ProductList = tempResult.Select(t => new ProductModel
                {
                    ProductName = t.ProductName,
                    ProductID = t.ProductID,
                    BrandID = t.BrandID,
                    Description = t.Description,
                    SubCategoryID = t.SubCategoryID,
                    ProductImage = t.ProductImage,
                    UnitsInStock = t.UnitsInStock,
                    BrandName = t.BrandName,
                    CostPrice = t.CostPrice,
                    HotProduct = t.HotProduct,
                    ProductAvailable = t.ProductAvailable,
                    Rating = t.Rating,
                    UnitPrice = t.UnitPrice,

                }).ToList();
                #endregion
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }
        public List<CategoriesModel> GetCategories()
        {
            List<CategoriesModel> result = new List<CategoriesModel>();

            try
            {
                string sql = @"SELECT c.CategoryID,c.CategoryName,sc.SubCategoryID,sc.SubCategoryName,COUNT(p.ProductID) ProductCount
                                            FROM Categories c 
                                            JOIN SubCategories sc 
                                            ON c.CategoryID = sc.CategoryID 
                                            LEFT JOIN Products p
                                            ON sc.SubCategoryID = p.SubCategoryID
                                            GROUP BY c.CategoryID,c.CategoryName,sc.SubCategoryID,sc.SubCategoryName
                                            ORDER BY c.CategoryID";
                var tempResult = _dbConnection.Query(sql).ToList();

                result = tempResult.GroupBy(t => new { t.CategoryID, t.CategoryName }).Select(t => new CategoriesModel
                {
                    CategoryId = t.Key.CategoryID,
                    CategoryName = t.Key.CategoryName,
                    ProductCount = t.Sum(s => s.ProductCount),
                    SubCategories = t.Select(s => new SubCategoriesModel
                    {
                        CategoryId = t.Key.CategoryID,
                        SubCategoryId = s.SubCategoryID,
                        SubCategoryName = s.SubCategoryName,
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }
        public ProductModel GetProduct(int id)
        {
            ProductModel result = new ProductModel();
            try
            {
                string sql = @"SELECT avg(ISNULL(cm.Rating,0)) Rating, p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.UnitPrice, p.UnitsInStock, p.Description, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName ,c.CategoryID,p.HotProduct,p.ProductAvailable 
	                    FROM Products p 
	                    JOIN SubCategories sc ON p.SubCategoryID =sc.SubCategoryID 
	                    JOIN Categories c ON sc.CategoryID = c.CategoryID 
	                    JOIN Brand b ON p.BrandID = b.BrandID
	                    LEFT JOIN  ProductImagePath pp ON p.ProductID =pp.ProductID
	                    LEFT JOIN Comment cm ON p.ProductID = cm.ProductID
	                    WHERE  p.ProductID = @ProductID
	                    GROUP BY p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.UnitPrice, p.UnitsInStock, p.Description, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName,c.CategoryID,p.HotProduct,p.ProductAvailable  ";

                //匿名類型
                var param = new
                {
                    ProductID = id,
                };
                var list = _dbConnection.Query<ProductModel>(sql, param);

                result = list.FirstOrDefault();
                result.ProductImages = list.Select(p => p.ProductImage).ToList();

            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }
        /// <summary>
        /// 新增產品(INSERT)到DB
        /// </summary>
        /// <param name="product">填好資料後，要傳入一個product給DB</param>
        public void CreateProduct(ProductModel product)
        {
            try
            {
                string sql = @"INSERT INTO  Products 
                                            (ProductName, SubCategoryID, BrandID, CostPrice, UnitPrice, UnitsInStock, Description, HotProduct, ProductAvailable)
                                            OUTPUT INSERTED.ProductID
                                            VALUES 
                                            (@ProductName, @SubCategoryID, @BrandID, @CostPrice, @UnitPrice, @UnitsInStock, @Description, @HotProduct, @ProductAvailable)";
                string imageSql = @"INSERT INTO [dbo].[ProductImagePath] ([ProductID],[ProductImage],[IsMainImage])
                                                        VALUES (@ProductID,@ProductImage,@IsMainImage)";
                //匿名類型
                var param = new
                {
                    ProductName = product.ProductName,
                    SubCategoryID = product.SubCategoryID,
                    BrandID = product.BrandID,
                    CostPrice = product.CostPrice,
                    UnitPrice = product.UnitPrice,
                    UnitsInStock = product.UnitsInStock,
                    Description = product.Description,
                    HotProduct = product.HotProduct,
                    ProductAvailable = product.ProductAvailable
                };
                //
                product.ProductID = _dbConnection.QuerySingle<int>(sql, param);

                for (int i = 0; i < product.ProductImages.Count; i++)
                {
                    var paramImg = new
                    {
                        ProductID = product.ProductID,
                        ProductImage = product.ProductImages[i],
                        IsMainImage = i == 0 // i == 0 ? true : false
                    };
                    _dbConnection.Execute(imageSql, paramImg);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public List<BrandModel> GetBrands()
        {
            List<BrandModel> result = new List<BrandModel>();
            try
            {
                string sql = @"SELECT * FROM Brand";
                result = _dbConnection.Query<BrandModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public void EditProduct(ProductModel product)
        {
            ProductModel result = new ProductModel();
            try
            {
                string sql = @"UPDATE Products 
                                            SET ProductName=@ProductName, SubCategoryID=@SubCategoryID, BrandID=@BrandID, CostPrice=@CostPrice, UnitPrice=@UnitPrice, 
                                                    UnitsInStock=@UnitsInStock, Description=@Description, HotProduct=@HotProduct, ProductAvailable=@ProductAvailable 
                                            WHERE ProductID=@ProductID";
                //1. 先刪除商品所有的照片
                string deleteImageSql = @"DELETE FROM ProductImagePath
                                                                    WHERE ProductID=@ProductID";
                //2. 再進行新增照片的動作，以達到修改商品照片的目的!
                string imageSql = @"INSERT INTO [dbo].[ProductImagePath] ([ProductID],[ProductImage],[IsMainImage])
                                                        VALUES (@ProductID,@ProductImage,@IsMainImage)";

                //匿名類型
                var param = new
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    SubCategoryID = product.SubCategoryID,
                    BrandID = product.BrandID,
                    CostPrice = product.CostPrice,
                    UnitPrice = product.UnitPrice,
                    UnitsInStock = product.UnitsInStock,
                    Description = product.Description,
                    HotProduct = product.HotProduct,
                    ProductAvailable = product.ProductAvailable
                };

                _dbConnection.Execute(sql, param); //1. 修改商品內容
                _dbConnection.Execute(deleteImageSql, param); //1.1 刪除商品所有的照片

                for (int i = 0; i < product.ProductImages.Count; i++)
                {
                    var paramImg = new
                    {
                        ProductID = product.ProductID,
                        ProductImage = product.ProductImages[i],
                        IsMainImage = i == 0 // i == 0 ? true : false
                    };
                    _dbConnection.Execute(imageSql, paramImg); //2. 新增商品圖片進去DB
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void DeleteProduct(int id)
        {
            ProductModel result = new ProductModel();
            try
            {
                string sql = @"DELETE FROM Products 
                                            WHERE ProductID=@ProductID;
                                            DELETE FROM ProductImagePath
                                            WHERE ProductID=@ProductID";
                //匿名類型
                var param = new
                {
                    ProductID = id
                };

                _dbConnection.Execute(sql, param);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ProductListResponseModel GetPrjProductList(ProductListRequestModel request)
        {
            ProductListResponseModel result = new ProductListResponseModel()
            {
                ProductList = new List<ProductModel>(),
                Pagination = request.Pagination,
            };
            try
            {
                string countSql = @"SELECT COUNT(1)
FROM
(
	SELECT avg(ISNULL(cm.Rating,0)) Rating, p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.CostPrice,p.UnitPrice, p.UnitsInStock, p.Description, p.HotProduct, p.ProductAvailable, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName 
	FROM Products p 
	JOIN SubCategories sc ON p.SubCategoryID =sc.SubCategoryID 
	JOIN Categories c ON sc.CategoryID = c.CategoryID 
	JOIN Brand b ON p.BrandID = b.BrandID
    JOIN PrjConnect pc  ON p.ProductID = pc.ProductID
	LEFT JOIN  ProductImagePath pp ON p.ProductID =pp.ProductID
	LEFT JOIN Comment cm ON p.ProductID = cm.ProductID
	WHERE pp.IsMainImage = 1 AND p.UnitsInStock > 0
	GROUP BY p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.CostPrice,p.UnitPrice, p.UnitsInStock, p.Description, p.HotProduct, p.ProductAvailable, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName 
) T";


                string sql = @"SELECT avg(ISNULL(cm.Rating,0)) Rating, p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.CostPrice,p.UnitPrice, p.UnitsInStock, p.Description, p.HotProduct, p.ProductAvailable, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName ,pc.PrjID
FROM Products p 
JOIN SubCategories sc ON p.SubCategoryID =sc.SubCategoryID 
JOIN Categories c ON sc.CategoryID = c.CategoryID 
JOIN Brand b ON p.BrandID = b.BrandID
JOIN PrjConnect pc  ON p.ProductID = pc.ProductID
LEFT JOIN  ProductImagePath pp ON p.ProductID =pp.ProductID
LEFT JOIN Comment cm ON p.ProductID = cm.ProductID
WHERE pp.IsMainImage = 1  AND p.UnitsInStock > 0
GROUP BY p.ProductID, p.ProductName, p.SubCategoryID, p.BrandID, p.CostPrice,p.UnitPrice, p.UnitsInStock, p.Description, p.HotProduct, p.ProductAvailable, sc.SubCategoryName,c.CategoryName,pp.ProductImage,b.BrandName,pc.PrjID 
{0} OFFSET @PageSize*(@Page-1) ROWS FETCH NEXT @PageSize ROWS ONLY";



                var param = new
                {
                    PageSize = request.Pagination.PageSize,
                    Page = request.Pagination.Page,
                };

                result.Pagination.TotalRecord = (int)_dbConnection.ExecuteScalar(countSql, param); //拿第一個cell

                string orderBy = "";
                switch (request.SortBy)
                {
                    case Enum.SortBy.Default:
                        orderBy += " ORDER BY p.ProductID ASC  ";
                        break;
                    case Enum.SortBy.HighPrice:
                        orderBy += " ORDER BY p.UnitPrice DESC ";
                        break;
                    case Enum.SortBy.LowPrice:
                        orderBy += " ORDER BY p.UnitPrice ASC   ";
                        break;
                    case Enum.SortBy.HighRated:
                        orderBy += " ORDER BY Rating DESC ";
                        break;
                    case Enum.SortBy.LowRated:
                        orderBy += " ORDER BY Rating ASC ";
                        break;
                    default:
                        orderBy += " ORDER BY p.ProductID ASC  ";
                        break;
                }

                result.ProductList = _dbConnection.Query<ProductModel>(string.Format(sql, orderBy), param).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }
    }
}
