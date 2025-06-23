using web_app_template.Domain.Models;

namespace web_app_template.Domain.Interfaces
{
    public interface IGenericInterface
    {
        public Task<TDestination> GetByIdAsync<TSource, TDestination>(int id, List<string> includings = null) where TSource : class where TDestination : class;
        public Task<List<TDestination>> GetAllAsync<TSource, TDestination>(List<string> includings = null) where TSource : class where TDestination : class;
        public Task<PaginatedResult<TDestination>> GetPaginatedAsync<TSource, TDestination>(int pageNumber, int pageSize, List<string> includings = null) where TSource : class where TDestination : class;
        public Task<TDestination> FindByPropertyAsync<TSource, TDestination>(string propertyName, object value, List<string> includings = null) where TSource : class where TDestination : class;
        public Task<List<TDestination>> FilterByPropertyAsync<TSource, TDestination>(string propertyName, object value, List<string> includings = null) where TSource : class where TDestination : class;
        public Task<PaginatedResult<TDestination>> FilterPaginatedAsync<TSource, TDestination>(string propertyName, object value, int pageNumber, int pageSize, List<string> includings = null) where TSource : class where TDestination : class;
    }
}
