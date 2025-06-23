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
        /// Retrieves a paginated list of entities of the specified source type, maps them to the specified destination
        /// type,  and returns the result along with the total count of entities.
        /// </summary>
        /// <remarks>This method retrieves entities from the underlying repository, maps them to the
        /// specified destination type using the configured mapper,  and handles cases where no entities are found or
        /// mapping fails. In case of an error, an empty result is returned, and the error is logged.</remarks>
        /// <typeparam name="TSource">The type of the source entities to retrieve. Must be a reference type.</typeparam>
        /// <typeparam name="TDestination">The type of the destination entities to map to. Must be a reference type.</typeparam>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items per page. Must be greater than or equal to 1.</param>
        /// <returns>A <see cref="PaginatedResult{TDestination}"/> containing the mapped entities for the specified page and the
        /// total count of entities. If no entities are found, the <c>Items</c> collection will be empty, and
        /// <c>TotalCount</c> will be 0.</returns>
        public async Task<PaginatedResult<TDestination>> GetPaginatedAsync<TSource, TDestination>(int pageNumber, int pageSize, List<string> includings = null)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entities = await _repository.GetPaginatedAsync<TSource>(pageNumber, pageSize, includings);

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
                _logger.LogError(ex, "Error retrieving paginated entities of type {EntityType}", typeof(TSource).Name);
                _responseMessage = ResponseStatusCodes.InternalServerError;
                return new PaginatedResult<TDestination>
                {
                    Items = new List<TDestination>(),
                    TotalCount = 0
                };
            }
        }

        /// <summary>
        /// Asynchronously retrieves an entity of type <typeparamref name="TDestination"/> by searching for a property
        /// value in the source entity type <typeparamref name="TSource"/>.
        /// </summary>
        /// <remarks>This method searches for an entity of type <typeparamref name="TSource"/> where the
        /// specified property matches the provided value. If a match is found, the entity is mapped to an instance of
        /// <typeparamref name="TDestination"/>. If no match is found, or if the mapping fails, the method returns <see
        /// langword="null"/>.</remarks>
        /// <typeparam name="TSource">The type of the source entity to search.</typeparam>
        /// <typeparam name="TDestination">The type of the destination entity to map the result to.</typeparam>
        /// <param name="propertyName">The name of the property to search for. Cannot be null or empty.</param>
        /// <param name="value">The value of the property to match. Cannot be null.</param>
        /// <returns>An instance of <typeparamref name="TDestination"/> mapped from the matching <typeparamref name="TSource"/>
        /// entity, or <see langword="null"/> if no matching entity is found or if an error occurs.</returns>
        public async Task<TDestination> FindByPropertyAsync<TSource, TDestination>(string propertyName, object value, List<string> includings = null)
            where TSource : class
            where TDestination : class
        {
            if (String.IsNullOrEmpty(propertyName) || value == null)
            {
                _responseMessage = ResponseStatusCodes.BadRequest;
                return null;
            }

            try
            {
                var entity = await _repository.FilterByPropertyAsync<TSource>(propertyName, value, includings);

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
                _logger.LogError(ex, "Error finding entity of type {EntityType} by property {PropertyName} with value {Value}", typeof(TSource).Name, propertyName, value);
                _responseMessage = ResponseStatusCodes.InternalServerError;
                return null;
            }
        }

        /// <summary>
        /// Filters a collection of entities of type <typeparamref name="TSource"/> based on the specified property name
        /// and value, and maps the filtered results to a list of type <typeparamref name="TDestination"/>.
        /// </summary>
        /// <remarks>If no entities match the specified filter criteria, an empty list is returned, and a
        /// "NotFound" response status is set internally. If the mapping operation fails, an empty list is returned, and
        /// a "BadRequest" response status is set internally. In the event of an exception, an empty list is returned,
        /// and a "InternalServerError" response status is set internally.</remarks>
        /// <typeparam name="TSource">The type of the source entities to filter. Must be a reference type.</typeparam>
        /// <typeparam name="TDestination">The type of the destination entities to map the results to. Must be a reference type.</typeparam>
        /// <param name="propertyName">The name of the property to filter by. This must match a valid property name of <typeparamref
        /// name="TSource"/>.</param>
        /// <param name="value">The value to filter the property by. The comparison is typically performed using equality.</param>
        /// <returns>A list of entities of type <typeparamref name="TDestination"/> that match the specified filter criteria. 
        /// Returns an empty list if no matching entities are found or if an error occurs during processing.</returns>
        public async Task<List<TDestination>> FilterByPropertyAsync<TSource, TDestination>(string propertyName, object value, List<string> includings = null)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entities = await _repository.FilterByPropertyAsync<TSource>(propertyName, value, includings);

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
                _logger.LogError(ex, "Error filtering entities of type {EntityType} by property {PropertyName} with value {Value}", typeof(TSource).Name, propertyName, value);
                _responseMessage = ResponseStatusCodes.InternalServerError;
                return new List<TDestination>();
            }
        }

        /// <summary>
        /// Filters a collection of entities by a specified property and value, and returns a paginated result.
        /// </summary>
        /// <remarks>This method filters entities of type <typeparamref name="TSource"/> based on the
        /// specified property and value, maps the results to type <typeparamref name="TDestination"/>, and returns a
        /// paginated result. If an error occurs during filtering or mapping, the method logs the error and returns an
        /// empty result set.</remarks>
        /// <typeparam name="TSource">The type of the source entity to filter.</typeparam>
        /// <typeparam name="TDestination">The type of the destination entity to map the results to.</typeparam>
        /// <param name="propertyName">The name of the property to filter by. This must match a valid property of <typeparamref name="TSource"/>.</param>
        /// <param name="value">The value to filter the property by. The comparison is typically case-sensitive and exact, depending on the
        /// repository implementation.</param>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items per page. Must be greater than or equal to 1.</param>
        /// <returns>A <see cref="PaginatedResult{TDestination}"/> containing the filtered and paginated results. If no matching
        /// entities are found, the <see cref="PaginatedResult{TDestination}.Items"/> collection will be empty, and <see
        /// cref="PaginatedResult{TDestination}.TotalCount"/> will be 0.</returns>
        public async Task<PaginatedResult<TDestination>> FilterPaginatedAsync<TSource, TDestination>(string propertyName, object value, int pageNumber, int pageSize, List<string> includings = null)
            where TSource : class
            where TDestination : class
        {
            try
            {
                var entities = await _repository.FilterPaginatedAsync<TSource>(propertyName, value, pageNumber, pageSize, includings);

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
                _logger.LogError(ex, "Error filtering paginated entities of type {EntityType} by property {PropertyName} with value {Value}", typeof(TSource).Name, propertyName, value);
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
