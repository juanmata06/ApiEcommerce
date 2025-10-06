using System;

namespace ApiEcommerce.Repository.IRepository;

public interface ICategoryRepository
{
    ICollection<Category> GetCategories();
    Category GetCategoryById(int id);
    bool CategoryExistsById(int id);
    bool CategoryExistsByName(string name);
    bool CreateCategory(Category category);
    bool UpdateCategory(Category category);
    bool DeleteCategory(Category category);
    bool Save();
}
