using web_app_template.Domain.Enums;
using web_app_template.Domain.Models;

namespace web_app_template.Domain.Interfaces
{
    public interface IGenericInterface
    {
        public ResponseStatusCodes _responseMessage { get; set; }
        public Task<TDestination> GetByIdAsync<TSource, TDestination>(int id, List<string> includings = null) where TSource : class where TDestination : class;
        public Task<List<TDestination>> GetAllAsync<TSource, TDestination>(List<string> includings = null) where TSource : class where TDestination : class;
        public Task<TDestination> FindByPropertiesAsync<TSource, TDestination>(List<PropertyFilter> propertyFilters, List<string> includings = null) where TSource : class where TDestination : class;
        public Task<List<TDestination>> FilterByPropertiesAsync<TSource, TDestination>(List<PropertyFilter> propertyFilters, int order = 1, List<string> includings = null) where TSource : class where TDestination : class;
        public Task<PaginatedResult<TDestination>> FilterPaginatedAsync<TSource, TDestination>(List<PropertyFilter> propertyFilters, int pageNumber, int pageSize, int order = 1, List<string> includings = null) where TSource : class where TDestination : class;
    }
}
