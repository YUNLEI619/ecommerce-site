using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShoppingCart_T8.Data;

namespace ShoppingCart_T8.Data //An attempt to implement ADLC2 learnings of a seperate Database but unable to fully implement and test due to time.
{
    public class ModelManager<T> : IModelManager<T> where T : class, IEntityManager, new()
    {
        private readonly DataContext _data;

        public ModelManager(DataContext data)
        {
            _data = data;
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _data.Set<T>().ToListAsync();

        public async Task<T> GetAsync(int id) => await _data.Set<T>().FirstOrDefaultAsync(e => e.Id == id);

        public async Task AddAsync(T model) => await _data.Set<T>().AddAsync(model);

        public void DeleteRange(IEnumerable<T> models) => _data.Set<T>().RemoveRange(models);
    }
}
