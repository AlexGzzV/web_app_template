using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace web_app_template.Data.Repositories
{
    public class GenericRepository
    {
        private readonly WebAppTemplateDbContext _context;
        private readonly ILogger<GenericRepository> _logger;

        public GenericRepository(WebAppTemplateDbContext context, ILogger<GenericRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously retrieves an entity of the specified type by its unique identifier, with optional related
        /// data included.
        /// </summary>
        /// <remarks>This method logs information about the retrieval process, including whether the
        /// entity was found. If the entity is not found, a warning is logged.</remarks>
        /// <typeparam name="T">The type of the entity to retrieve. Must be a reference type.</typeparam>
        /// <typeparam name="TIncluding">The type of the related data to include. Typically represents navigation properties.</typeparam>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <param name="includings">An optional list of related data to include in the query. Each item in the list represents a navigation
        /// property to be included. If <paramref name="includings"/> is null, no related data will be included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity of type <typeparamref
        /// name="T"/> if found; otherwise, <see langword="null"/>.</returns>
        public async Task<T> GetByIdAsync<T>(int id, List<string> includings = null) where T : class
        {
            _logger.LogInformation($"Fetching entity of type {typeof(T).Name} with ID {id}");
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
                foreach (var including in includings)
                    query = query.Include(including);

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<object>(e, "Id").Equals(id));

            if (entity == null)
                _logger.LogWarning($"Entity of type {typeof(T).Name} with ID {id} not found");
            return entity;
        }

        /// <summary>
        /// Asynchronously retrieves all entities of the specified type from the database,  with optional navigation
        /// property inclusion.
        /// </summary>
        /// <remarks>This method logs informational messages when fetching entities and logs a warning if
        /// no  entities of the specified type are found.</remarks>
        /// <typeparam name="T">The type of the entities to retrieve. Must be a reference type.</typeparam>
        /// <typeparam name="TIncluding">The type of the navigation properties to include. This is used to specify related entities  to load along
        /// with the main entities.</typeparam>
        /// <param name="includings">A list of navigation properties to include in the query. Each item in the list represents  a related entity
        /// to be eagerly loaded. If null, no related entities are included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of  entities of type
        /// <typeparamref name="T"/>. If no entities are found, an empty list is returned.</returns>
        public async Task<List<T>> GetAllAsync<T>(List<string> includings = null) where T : class
        {
            _logger.LogInformation($"Fetching all entities of type {typeof(T).Name}");
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
                foreach (var including in includings)
                    query = query.Include(including);

            var entities = await query.ToListAsync();

            if (entities == null || !entities.Any())
                _logger.LogWarning($"No entities of type {typeof(T).Name} found");
            return entities;
        }

        /// <summary>
        /// Retrieves a paginated list of entities of the specified type, along with the total count of entities in the
        /// data source.
        /// </summary>
        /// <remarks>This method uses Entity Framework to query the data source. If <paramref
        /// name="includings"/> is provided, the specified related entities will be included in the query using the <see
        /// cref="Microsoft.EntityFrameworkCore.Query.IQueryableExtensions.Include{TEntity, TProperty}"/>
        /// method.</remarks>
        /// <typeparam name="T">The type of the entities to retrieve.</typeparam>
        /// <typeparam name="TIncluding">The type of the related entities to include in the query.</typeparam>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items per page. Must be greater than or equal to 1.</param>
        /// <param name="includings">An optional list of related entities to include in the query. If null, no related entities are included.</param>
        /// <returns>A tuple containing a list of entities of type <typeparamref name="T"/> and the total count of entities in
        /// the data source. The list will contain up to <paramref name="pageSize"/> items, or fewer if there are not
        /// enough entities to fill the page.</returns>
        public async Task<(List<T> Items, int TotalCount)> GetPaginatedAsync<T>(int pageNumber, int pageSize, List<string> includings = null) where T : class
        {
            _logger.LogInformation($"Fetching paginated entities of type {typeof(T).Name} - Page {pageNumber}, Size {pageSize}");
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
                foreach (var including in includings)
                    query = query.Include(including);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        /// <summary>
        /// Asynchronously finds an entity of the specified type by a given property name and value.
        /// </summary>
        /// <remarks>This method uses Entity Framework Core to query the database. The <paramref
        /// name="propertyName"/> must correspond to a valid property of the entity type <typeparamref name="T"/>. The
        /// <paramref name="includings"/> parameter allows eager loading of related entities to reduce subsequent
        /// database queries.</remarks>
        /// <typeparam name="T">The type of the entity to search for.</typeparam>
        /// <typeparam name="TIncluding">The type of the related entities to include in the query.</typeparam>
        /// <param name="propertyName">The name of the property to filter by. This must match the property name in the entity type <typeparamref
        /// name="T"/>.</param>
        /// <param name="value">The value of the property to match.</param>
        /// <param name="includings">An optional list of related entities to include in the query. Each item in the list should correspond to a
        /// navigation property of the entity type <typeparamref name="T"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity of type
        /// <typeparamref name="T"/> that matches the specified property name and value, or <see langword="null"/> if no
        /// such entity is found.</returns>
        public async Task<T> FindByPropertyAsync<T>(string propertyName, object value, List<string> includings = null) where T : class
        {
            _logger.LogInformation($"Searching for entity of type {typeof(T).Name} with {propertyName} = {value}");
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
                foreach (var including in includings)
                    query = query.Include(including);

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<object>(e, propertyName).Equals(value));

            if (entity == null)
                _logger.LogWarning($"Entity of type {typeof(T).Name} with {propertyName} = {value} not found");
            return entity;
        }

        /// <summary>
        /// Filters entities of the specified type based on a property value and returns the matching results.
        /// </summary>
        /// <remarks>This method uses Entity Framework Core to query the database. The <paramref
        /// name="propertyName"/> must correspond to a valid property in the entity type <typeparamref name="T"/>. The
        /// <paramref name="includings"/> parameter allows eager loading of related entities to reduce the number of
        /// database queries.</remarks>
        /// <typeparam name="T">The type of the entities to filter.</typeparam>
        /// <typeparam name="TIncluding">The type of the related entities to include in the query.</typeparam>
        /// <param name="propertyName">The name of the property to filter by. This must match the property name in the entity type <typeparamref
        /// name="T"/>.</param>
        /// <param name="value">The value to match against the specified property.</param>
        /// <param name="includings">An optional list of related entities to include in the query. Each item in the list should correspond to a
        /// navigation property in the entity type <typeparamref name="T"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities of type
        /// <typeparamref name="T"/> that match the specified property value. If no entities match, an empty list is
        /// returned.</returns>
        public async Task<List<T>> FilterByPropertyAsync<T>(string propertyName, object value, List<string> includings = null) where T : class
        {
            _logger.LogInformation($"Finding entities of type {typeof(T).Name} with {propertyName} = {value}");
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
                foreach (var including in includings)
                    query = query.Include(including);

            var entities = await query.Where(e => EF.Property<object>(e, propertyName).Equals(value)).ToListAsync();

            if (entities == null || !entities.Any())
                _logger.LogWarning($"No entities of type {typeof(T).Name} found with the specified condition");
            return entities;
        }

        /// <summary>
        /// Retrieves a paginated list of entities of the specified type that match a given property value.
        /// </summary>
        /// <remarks>This method uses Entity Framework Core to query the database. The <paramref
        /// name="propertyName"/> must correspond to a valid property of the entity type <typeparamref name="T"/>. The
        /// method supports eager loading of related entities specified in the <paramref name="includings"/>
        /// parameter.</remarks>
        /// <typeparam name="T">The type of the entities to retrieve.</typeparam>
        /// <typeparam name="TIncluding">The type of the related entities to include in the query.</typeparam>
        /// <param name="propertyName">The name of the property to filter by. Must match a property of the entity type <typeparamref name="T"/>.</param>
        /// <param name="value">The value to filter the specified property by.</param>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items per page. Must be greater than or equal to 1.</param>
        /// <param name="includings">A list of related entities to include in the query. Each item in the list represents a navigation property
        /// to include. If null, no related entities are included.</param>
        /// <returns>A tuple containing the following: <list type="bullet"> <item> <description><see cref="List{T}"/>: A list of
        /// entities of type <typeparamref name="T"/> that match the filter criteria for the specified
        /// page.</description> </item> <item> <description><see cref="int"/>: The total count of entities that match
        /// the filter criteria across all pages.</description> </item> </list></returns>
        public async Task<(List<T> Items, int TotalCount)> FilterPaginatedAsync<T>(string propertyName, object value, int pageNumber, int pageSize, List<string> includings = null) where T : class
        {
            _logger.LogInformation($"Fetching paginated entities of type {typeof(T).Name} with {propertyName} = {value} - Page {pageNumber}, Size {pageSize}");
            var query = _context.Set<T>().Where(e => EF.Property<object>(e, propertyName).Equals(value)).AsQueryable();

            if (includings != null)
                foreach (var including in includings)
                    query = query.Include(including);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        /// <summary>
        /// Asynchronously creates a new entity in the database.
        /// </summary>
        /// <remarks>This method logs the outcome of the operation, including any errors that occur during
        /// the save process.</remarks>
        /// <typeparam name="T">The type of the entity to create. Must be a reference type.</typeparam>
        /// <param name="entity">The entity to be added to the database. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the entity was successfully created and saved to the database; otherwise, <see
        /// langword="false"/> if an error occurred during the save operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <see langword="null"/>.</exception>
        public async Task<bool> AddEntityAsync<T>(T entity) where T : class
        {
            if (entity == null)
            {
                _logger.LogError("Attempted to create a null entity");
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            _context.Set<T>().Add(entity);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Entity of type {typeof(T).Name} created successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating entity of type {typeof(T).Name}");
                return false;
            }
        }

        /// <summary>
        /// Updates the specified entity in the database asynchronously.
        /// </summary>
        /// <remarks>This method updates the provided entity in the database context and attempts to save
        /// the changes. If an error occurs during the save operation, the method logs the error and returns <see
        /// langword="false"/>.</remarks>
        /// <typeparam name="T">The type of the entity to update. Must be a reference type.</typeparam>
        /// <param name="entity">The entity to update. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the entity was updated successfully; otherwise, <see langword="false"/> if an
        /// error occurred during the update.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <see langword="null"/>.</exception>
        public async Task<bool> UpdateEntityAsync<T>(T entity) where T : class
        {
            if (entity == null)
            {
                _logger.LogError("Attempted to update a null entity");
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            _context.Set<T>().Update(entity);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Entity of type {typeof(T).Name} updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating entity of type {typeof(T).Name}");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified entity from the database.
        /// </summary>
        /// <remarks>This method removes the provided entity from the database context and attempts to save
        /// the changes. If an error occurs during the save operation, the method logs the error and returns <see
        /// langword="false"/>.</remarks>
        /// <typeparam name="T">The type of the entity to delete. Must be a reference type.</typeparam>
        /// <param name="entity">The entity to delete. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the entity was deleted successfully; otherwise, <see langword="false"/> if an
        /// error occurred during the deletion.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <see langword="null"/>.</exception>
        public async Task<bool> DeleteEntityAsync<T>(T entity) where T : class
        {
            if (entity == null)
            {
                _logger.LogError("Attempted to delete a null entity");
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            _context.Set<T>().Remove(entity);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Entity of type {typeof(T).Name} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting entity of type {typeof(T).Name}");
                return false;
            }
        }
    }
}
