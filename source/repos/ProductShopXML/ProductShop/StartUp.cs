using ProductShop.Data;
using ProductShop.Dtos;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using ProductShop.XMLHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            ProductShopContext context = new ProductShopContext();

            //context.Database.EnsureDeleted();
            //Console.WriteLine("Successfully deleted the database!");

            //context.Database.EnsureCreated();
            //Console.WriteLine("Successfully created the database!");

            //var usersXml = File.ReadAllText("../../../Datasets/users.xml");
            //var productsXml = File.ReadAllText("../../../Datasets/products.xml");
            //var categoriesXml = File.ReadAllText("../../../Datasets/categories.xml");
            var categoriesProducts = File.ReadAllText("../../../Datasets/categories-products.xml");

            //ImportUsers(context, usersXml);
            //ImportProducts(context, productsXml);
            //ImportCategories(context, categoriesXml);

            var productsInRange = GetUsersWithProducts(context);

            File.WriteAllText("../../../Results/users-and-products.xml", productsInRange);
        }

        //Problem 01
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Users";

            var usersResult = XmlConverter.Deserializer<ImportUserDTO>(inputXml, rootElement);

            var users = usersResult
                .Select(u => new User
                {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Age = u.Age
                })
                .ToArray();

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        //Problem 02
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Products";

            var productDtos = XmlConverter.Deserializer<ImportProductDTO>(inputXml, rootElement);

            var products = productDtos.Select(p => new Product
            {
                Name = p.Name,
                Price = p.Price,
                BuyerId = p.BuyerId,
                SellerId = p.SellerId
            })
                .ToArray();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        //Problem 03
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Categories";

            var categoriesDTO = XmlConverter.Deserializer<ImportCategoryDTO>(inputXml, rootElement);

            var categories = categoriesDTO.Where(c => c.Name != null)
                .Select(c => new Category
                {
                    Name = c.Name
                })
                .ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }
        
        //Problem 04
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            const string rootElement = "CategoryProducts";

            var categoryProductDtos = XmlConverter.Deserializer<ImportCategoryProductDTO>(inputXml, rootElement);

            var categoriesProducts = categoryProductDtos
                .Where(cp => context.Categories.Any(c=>c.Id == cp.CategoryId) &&
                             context.Products.Any(p => p.Id == cp.ProductId))
                .Select(cp => new CategoryProduct
                {
                    CategoryId = cp.CategoryId,
                    ProductId = cp.ProductId
                })
                .ToArray();

            context.CategoryProducts.AddRange(categoriesProducts);
            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Length}";
        }

        //Problem 05
        public static string GetProductsInRange(ProductShopContext context)
        {
            const string rootElement = "Products";

            var products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(x => new ExportProductInfoDTO
                {
                    Name = x.Name,
                    Price = x.Price,
                    Buyer = x.Buyer.FirstName + " " + x.Buyer.LastName
                })
                .OrderBy(p => p.Price)
                .Take(10)
                .ToList();

            var result = XmlConverter.Serialize(products, rootElement);

            return result;
        }

        //Problem 06
        public static string GetSoldProducts(ProductShopContext context)
        {
            const string rootElement = "Users";

            var usersWithProducts = context.Users
                .Where(u=>u.ProductsSold.Any())
                .Select(x => new ExportUserSoldProductDTO
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    SoldProducts = x.ProductsSold.Select(p => new UserProductDTO
                    {
                        Name = p.Name,
                        Price = p.Price
                    })
                    .ToArray()
                })
                .OrderBy(l=>l.LastName)
                .ThenBy(f=>f.FirstName)
                .Take(5)
                .ToArray();

            var result = XmlConverter.Serialize(usersWithProducts, rootElement);

            return result;
        }

        //Problem 07
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            const string rootElement = "Categories";

            var categories = context.Categories
                .Select(c => new ExportCategoryDTO
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(p => p.Product.Price),
                    TotalRevenue = c.CategoryProducts.Sum(p => p.Product.Price)
                })
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.TotalRevenue)
                .ToArray();

            var result = XmlConverter.Serialize(categories, rootElement);

            return result;
        }

        //Problem 08
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersAndProducts = context.Users
                .Where(p => p.ProductsSold.Any())
                .Select(u => new ExportUserDTO
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProduct = new ExportProductCountDTO
                    {
                        Count = u.ProductsSold.Count,
                        Products = u.ProductsSold.Select(p => new ExportProductDTO
                        {
                            Name = p.Name,
                            Price = p.Price
                        })
                        .OrderByDescending(p=>p.Price)
                        .ToArray()
                    }
                })
                .OrderByDescending(x => x.SoldProduct.Count)
                .Take(10)
                .ToArray();

            var resultDto = new ExportUserCountDTO
            {
                Count = context.Users.Count(p => p.ProductsSold.Any()),
                Users = usersAndProducts
            };

            var result = XmlConverter.Serialize(resultDto, "Users");

            return result;
        }
    }
}