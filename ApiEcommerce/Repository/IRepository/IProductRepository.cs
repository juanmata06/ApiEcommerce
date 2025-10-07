using System;
using ApiEcommerce.Models;

namespace ApiEcommerce.Repository.IRepository;

public interface IProductRepository
{
    ICollection<Product> GetProducts();
    ICollection<Product> GetProductsByCategoryId(int categoryId);
    ICollection<Product> GetProductsByName(string name);
    Product? GetProductById(int id);
    bool ProductExistsById(int id);
    bool ProductExistsByName(string name);
    bool CreateProduct(Product product);
    bool UpdateProduct(Product product);
    bool DeleteProduct(Product product);
    bool BuyProduct(string name, int quantity);
    bool Save(); 
}
