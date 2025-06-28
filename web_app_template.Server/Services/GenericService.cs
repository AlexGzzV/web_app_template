using AutoMapper;
using web_app_template.Data.Repositories;
using web_app_template.Domain.Enums;
using web_app_template.Domain.Interfaces;
using web_app_template.Domain.Models;

namespace web_app_template.Server.Services
{
    public class GenericService : IGenericInterface
    {
        private readonly GenericRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GenericService> _logger;

        public ResponseStatusCodes _responseMessage { get; set; }

        public GenericService(GenericRepository repository, IMapper mapper, ILogger<GenericService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an entity by its identifier, maps it to the specified destination type, and returns the mapped
        /// result.
        /// </summary>
        /// <remarks>If the entity with the specified <paramref name="id"/> is not found, or if the
        /// mapping fails, the method returns <see langword="null"/>. Additionally, the method logs any exceptions that
        /// occur during the retrieval or mapping process.</remarks>
        /// <typeparam name="TSource">The type of the source entity to retrieve from the repository.</typeparam>
        /// <typeparam name="TDestination">The type to which the retrieved entity will be mapped.</typeparam>
        /// <param name="id">The unique identifier of the entity to retrieve. Must be greater than zero.</param>
        /// <returns>The mapped entity of type <typeparamref name="TDestination"/> if found and successfully mapped; otherwise,
        /// <see langword="null"/>.</returns>
        public async Task<TDestination> GetByIdAsync<TSource, TDestination>(int id, List<string> includings = null)
            where TSource : class
            where TDestination : class
        {
            if (id <= 0)
            {
                _responseMessage = ResponseStatusCodes.BadRequest;
                return null;
            }

            try
            {
                var entity = await _repository.GetByIdAsync<TSource>(id, includings);

                if (entity == null)
                {
                    _responseMessage = ResponseStatusCodes.NotFound;
                    return null;
                }

                var mappedEntity = _mapper.Map<TDestination>(entity);

                if (mappedEntity == null)
                {
                    _responseMessage = ResponseStatusCodes.BadRequest;
                    return null;
                }

                return mappedEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity of type {EntityType} with ID {Id}", typeof(TSource).Name, id);
                _responseMessage = ResponseStatusCodes.InternalServerError;
                return null;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all entities of the specified source type, maps them to the specified destination
        /// type, and returns the result as a list.
        /// </summary>
        /// <remarks>This method uses a repository to retrieve all entities of type <typeparamref
        /// name="TSource"/> and maps them to type <typeparamref name="TDestination"/> using the configured mapper. If
        /// no entities are found, an empty list is returned. If an error occurs during the operation, the error is
        /// logged, and an empty list is returned.</remarks>
        /// <typeparam name="TSource">The type of the source entities to retrieve. Must be a reference type.</typeparam>
        /// <typeparam name="TDestination">The type to which the source entities will be mapped. Must be a reference type.</typeparam>
        /// <returns>A list of mapped entities of type <typeparamref name="TDestination"/>.  Returns an empty list if no entities
        /// are found, if mapping fails, or if an error occurs during the operation.</returns>
        public async Task<List<TDestination>> GetAllAsync<TSource, TDestination>(List<string> includings = null)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entities = await _repository.GetAllAsync<TSource>(includings);

                if (!entities.Any())
                {
                    _responseMessage = ResponseStatusCodes.NotFound;
                    return new List<TDestination>();
                }

                var mappedEntities = _mapper.Map<List<TDestination>>(entities);

                if (mappedEntities == null)
                {
                    _responseMessage = ResponseStatusCodes.BadRequest;
                    return new List<TDestination>();
                }

                return mappedEntities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(TSource).Name);
                _responseMessage = ResponseStatusCodes.InternalServerError;
                return new List<TDestination>();
            }
        }

        /// <summary>
        /// Asynchronously retrieves an entity based on the specified property filters and maps it to the specified
        /// destination type.
        /// </summary>
        /// <remarks>This method queries the data source for an entity that matches the specified property
        /// filters. If a matching entity is found, it is mapped to the specified destination type. If no entity is
        /// found, the method returns <see langword="null"/> and sets an internal response message to indicate a "Not
        /// Found" status. If the mapping operation fails, the method returns <see langword="null"/> and sets an
        /// internal response message to indicate a "Bad Request" status. In the event of an exception, the method
        /// returns <see langword="null"/> and sets an internal response message to indicate an "Internal Server Error"
        /// status.</remarks>
        /// <typeparam name="TSource">The type of the source entity to query.</typeparam>
        /// <typeparam name="TDestination">The type to which the retrieved entity will be mapped.</typeparam>
        /// <param name="propertyFilters">A list of <see cref="PropertyFilter"/> objects that define the filtering criteria for the query. Cannot be
        /// null or empty.</param>
        /// <param name="includings">An optional list of related entities to include in the query. Can be null if no related entities need to be
        /// included.</param>
        /// <returns>An instance of <typeparamref name="TDestination"/> representing the mapped entity if found; otherwise, <see
        /// langword="null"/>.</returns>
        public async Task<TDestination> FindByPropertiesAsync<TSource, TDestination>(List<PropertyFilter> propertyFilters, List<string> includings = null)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entity = await _repository.FindByPropertiesAsync<TSource>(propertyFilters, includings);

                if (entity == null)
                {
                    _responseMessage = ResponseStatusCodes.NotFound;
                    return null;
                }

                var mappedEntity = _mapper.Map<TDestination>(entity);

                if (mappedEntity == null)
                {
                    _responseMessage = ResponseStatusCodes.BadRequest;
                    return null;
                }

                return mappedEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding entity of type {EntityType} with properties {PropertyFilters}", typeof(TSource).Name, propertyFilters);
                _responseMessage = ResponseStatusCodes.InternalServerError;
                return null;
            }
        }

        /// <summary>
        /// Filters a collection of entities based on the specified property filters, maps the results to the specified
        /// destination type, and returns the filtered and mapped collection.
        /// </summary>
        /// <remarks>This method uses a repository to perform the filtering operation and a mapper to
        /// transform the filtered entities into the destination type. If no entities match the specified filters, an
        /// empty list is returned. If an error occurs during mapping or filtering, an empty list is also
        /// returned.</remarks>
        /// <typeparam name="TSource">The type of the source entities to filter.</typeparam>
        /// <typeparam name="TDestination">The type to which the filtered entities will be mapped.</typeparam>
        /// <param name="propertyFilters">A list of <see cref="PropertyFilter"/> objects that define the filtering criteria.</param>
        /// <param name="order">The sorting order for the filtered results. Defaults to 1.</param>
        /// <param name="includings">An optional list of related entities to include in the query results.</param>
        /// <returns>A list of <typeparamref name="TDestination"/> objects representing the filtered and mapped entities. Returns
        /// an empty list if no entities match the filters or if an error occurs during processing.</returns>
        public async Task<List<TDestination>> FilterByPropertiesAsync<TSource, TDestination>(List<PropertyFilter> propertyFilters, int order = 1, List<string> includings = null)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entities = await _repository.FilterByPropertiesAsync<TSource>(propertyFilters, order, includings);

                if (!entities.Any())
                {
                    _responseMessage = ResponseStatusCodes.NotFound;
                    return new List<TDestination>();
                }

                var mappedEntities = _mapper.Map<List<TDestination>>(entities);

                if (mappedEntities == null)
                {
                    _responseMessage = ResponseStatusCodes.BadRequest;
                    return new List<TDestination>();
                }

                return mappedEntities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering entities of type {EntityType} with properties {PropertyFilters}", typeof(TSource).Name, propertyFilters);
                _responseMessage = ResponseStatusCodes.InternalServerError;
                return new List<TDestination>();
            }
        }

        /// <summary>
        /// Retrieves a paginated and filtered list of entities, maps them to the specified destination type,  and
        /// returns the result as a <see cref="PaginatedResult{TDestination}"/>.
        /// </summary>
        /// <remarks>This method applies the specified filters and pagination to the source entities, maps
        /// the results to the  destination type, and returns the paginated result. If an error occurs during
        /// processing, an empty result  is returned.</remarks>
        /// <typeparam name="TSource">The type of the source entities to filter and paginate.</typeparam>
        /// <typeparam name="TDestination">The type to which the source entities will be mapped.</typeparam>
        /// <param name="propertyFilters">A list of <see cref="PropertyFilter"/> objects used to filter the source entities.</param>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items per page. Must be greater than 0.</param>
        /// <param name="order">The sorting order for the results. Use 1 for ascending order and -1 for descending order.</param>
        /// <param name="includings">An optional list of related entities to include in the query. Can be null if no related entities are
        /// required.</param>
        /// <returns>A <see cref="PaginatedResult{TDestination}"/> containing the filtered and paginated list of mapped entities.
        /// If no entities match the filters, the <see cref="PaginatedResult{TDestination}.Items"/> collection will be
        /// empty,  and <see cref="PaginatedResult{TDestination}.TotalCount"/> will be 0.</returns>
        public async Task<PaginatedResult<TDestination>> FilterPaginatedAsync<TSource, TDestination>(List<PropertyFilter> propertyFilters, int pageNumber, int pageSize, int order = 1, List<string> includings = null)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entities = await _repository.FilterPaginatedAsync<TSource>(propertyFilters, pageNumber, pageSize, order, includings);

                if (!entities.Items.Any())
                {
                    _responseMessage = ResponseStatusCodes.NotFound;
                    return new PaginatedResult<TDestination>
                    {
                        Items = new List<TDestination>(),
                        TotalCount = 0
                    };
                }

                var mappedEntities = _mapper.Map<List<TDestination>>(entities.Items);

                if (mappedEntities == null)
                {
                    _responseMessage = ResponseStatusCodes.BadRequest;
                    return new PaginatedResult<TDestination>
                    {
                        Items = new List<TDestination>(),
                        TotalCount = 0
                    };
                }

                return new PaginatedResult<TDestination>
                {
                    Items = mappedEntities,
                    TotalCount = entities.TotalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering and paginating entities of type {EntityType} with properties {PropertyFilters}, page number {PageNumber}, page size {PageSize}, order {Order}",
                    typeof(TSource).Name, propertyFilters, pageNumber, pageSize, order);
                _responseMessage = ResponseStatusCodes.InternalServerError;
                return new PaginatedResult<TDestination>
                {
                    Items = new List<TDestination>(),
                    TotalCount = 0
                };
            }
        }
    }
}
