using MLCodingChallenge.Models;

namespace MLCodingChallenge.Data;

public interface IProductRepository
{
    List<Product> GetAll();
    Product? GetById(int id);
    List<Product> GetByIds(IEnumerable<int> ids);
}
