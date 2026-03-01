using MLCodingChallenge.Models;

namespace MLCodingChallenge.Services;

public interface IProductService
{
    List<Product> GetAll();
    CompareResponse Compare(List<int> ids, List<string>? fields);
}
