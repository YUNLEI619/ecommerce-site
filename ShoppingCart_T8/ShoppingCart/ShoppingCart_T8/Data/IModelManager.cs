namespace ShoppingCart_T8.Data
{
    public interface IModelManager<T> where T : class, IEntityManager, new()
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetAsync(int id);
        Task AddAsync(T model);
    }
}
