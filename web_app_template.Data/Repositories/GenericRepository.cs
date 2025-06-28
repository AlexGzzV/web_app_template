using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using web_app_template.Domain.Models;

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
        /// Asynchronously retrieves an entity of the specified type by its unique identifier, with optional navigation
        /// properties included.
        /// </summary>
        /// <remarks>This method uses Entity Framework Core to query the database. The navigation
        /// properties specified in  <paramref name="includings"/> are included in the query if they exist on the entity
        /// type.  If the entity is not found, a warning is logged.</remarks>
        /// <typeparam name="T">The type of the entity to retrieve. Must be a reference type.</typeparam>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <param name="includings">An optional list of navigation property names to include in the query.  If a specified property does not
        /// exist on the entity type, it will be ignored, and a warning will be logged.</param>
        /// <returns>The entity of type <typeparamref name="T"/> with the specified identifier, or <see langword="null"/> if no
        /// such entity exists.</returns>
        public async Task<T> GetByIdAsync<T>(int id, List<string> includings = null) where T : class
        {
            _logger.LogInformation($"Fetching entity of type {typeof(T).Name} with ID {id}");
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
            {
                var entityType = typeof(T);
                foreach (var including in includings)
                {
                    // Check if the navigation property exists (case-insensitive)
                    var prop = entityType.GetProperty(including, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (prop != null)
                        query = query.Include(including);
                    else
                        _logger.LogWarning($"Navigation property '{including}' does not exist on entity '{entityType.Name}'.");
                }
            }

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<object>(e, "Id").Equals(id));

            if (entity == null)
                _logger.LogWarning($"Entity of type {typeof(T).Name} with ID {id} not found");
            return entity;
        }

        /// <summary>
        /// Asynchronously retrieves all entities of the specified type from the database,  with optional navigation
        /// properties included.
        /// </summary>
        /// <remarks>This method logs information about the retrieval process, including warnings if
        /// specified navigation properties  do not exist on the entity type. Ensure that the provided navigation
        /// property names are valid for the entity type.</remarks>
        /// <typeparam name="T">The type of the entities to retrieve. Must be a reference type.</typeparam>
        /// <param name="includings">A list of navigation property names to include in the query.  If null or empty, no navigation properties are
        /// included.  Navigation property names are case-insensitive.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities of type
        /// <typeparamref name="T"/>. If no entities are found, the list will be empty.</returns>
        public async Task<List<T>> GetAllAsync<T>(List<string> includings = null) where T : class
        {
            _logger.LogInformation($"Fetching all entities of type {typeof(T).Name}");
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
            {
                var entityType = typeof(T);
                foreach (var including in includings)
                {
                    // Check if the navigation property exists (case-insensitive)
                    var prop = entityType.GetProperty(including, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (prop != null)
                        query = query.Include(including);
                    else
                        _logger.LogWarning($"Navigation property '{including}' does not exist on entity '{entityType.Name}'.");
                }
            }

            var entities = await query.ToListAsync();

            if (entities == null || !entities.Any())
                _logger.LogWarning($"No entities of type {typeof(T).Name} found");
            return entities;
        }

        /// <summary>
        /// Asynchronously retrieves the first entity of the specified type that matches the given property filters.
        /// </summary>
        /// <remarks>This method dynamically applies the specified filters and includes navigation
        /// properties in the query. If no entity matches the filters, a warning is logged. The method is
        /// case-insensitive when checking for the existence of navigation properties.</remarks>
        /// <typeparam name="T">The type of the entity to retrieve. Must be a reference type.</typeparam>
        /// <param name="propertyFilters">A list of <see cref="PropertyFilter"/> objects that define the filtering criteria. Each filter specifies a
        /// property name, a comparison operator, and a value to match.</param>
        /// <param name="includings">An optional list of navigation property names to include in the query. These properties will be eagerly
        /// loaded if they exist on the entity type. If a specified property does not exist, a warning will be logged.</param>
        /// <returns>The first entity of type <typeparamref name="T"/> that matches the specified filters, or <see
        /// langword="null"/> if no such entity is found.</returns>
        public async Task<T> FindByPropertiesAsync<T>(List<PropertyFilter> propertyFilters, List<string> includings = null) where T : class
        {
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
            {
                var entityType = typeof(T);
                foreach (var including in includings)
                {
                    // Check if the navigation property exists (case-insensitive)
                    var prop = entityType.GetProperty(including, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (prop != null)
                        query = query.Include(including);
                    else
                        _logger.LogWarning($"Navigation property '{including}' does not exist on entity '{entityType.Name}'.");
                }
            }

            // Apply filters dynamically
            foreach (var filter in propertyFilters)
            {
                if (String.IsNullOrEmpty(filter.PropertyName) || filter.Value == null)
                    continue;
                var predicate = BuildPredicate<T>(filter);
                query = query.Where(predicate);
            }

            var entity = await query.FirstOrDefaultAsync();

            if (entity == null)
                _logger.LogWarning($"No entity of type {typeof(T).Name} found with the specified condition");
            return entity;
        }

        /// <summary>
        /// Filters a collection of entities of type <typeparamref name="T"/> based on the specified property filters,
        /// optionally including related navigation properties and applying sorting.
        /// </summary>
        /// <remarks>- If the <paramref name="includings"/> parameter contains navigation properties that
        /// do not exist on the entity type, a warning is logged, and those properties are ignored. - If no filters are
        /// provided, all entities of type <typeparamref name="T"/> are returned. - If the <paramref name="order"/>
        /// parameter is invalid, no specific sorting is applied.</remarks>
        /// <typeparam name="T">The type of the entity to filter. Must be a reference type.</typeparam>
        /// <param name="propertyFilters">A list of <see cref="PropertyFilter"/> objects that define the filtering criteria. Each filter specifies a
        /// property name, a comparison operator, and a value.</param>
        /// <param name="order">The sorting order to apply to the results. Use <c>1</c> for ascending order, <c>2</c> for descending order,
        /// or any other value for no specific order.</param>
        /// <param name="includings">A list of navigation property names to include in the query. Navigation properties are included only if they
        /// exist on the entity type <typeparamref name="T"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities of type
        /// <typeparamref name="T"/> that match the specified filters. If no entities match, an empty list is returned.</returns>
        public async Task<List<T>> FilterByPropertiesAsync<T>(List<PropertyFilter> propertyFilters, int order = 1, List<string> includings = null) where T : class
        {
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
            {
                var entityType = typeof(T);
                foreach (var including in includings)
                {
                    // Check if the navigation property exists (case-insensitive)
                    var prop = entityType.GetProperty(including, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (prop != null)
                        query = query.Include(including);
                    else
                        _logger.LogWarning($"Navigation property '{including}' does not exist on entity '{entityType.Name}'.");
                }
            }

            // Apply filters dynamically
            foreach (var filter in propertyFilters)
            {
                if (String.IsNullOrEmpty(filter.PropertyName) || filter.Value == null)
                    continue;
                var predicate = BuildPredicate<T>(filter);
                query = query.Where(predicate);
            }

            // Order by the first filter property, if it exists
            if (propertyFilters.Count > 0)
            {
                var firstFilter = propertyFilters[0];
                switch (order)
                {
                    case 1:
                        query = query.OrderBy(e => EF.Property<object>(e, firstFilter.PropertyName));
                        break;
                    case 2:
                        query = query.OrderByDescending(e => EF.Property<object>(e, firstFilter.PropertyName));
                        break;
                    default:
                        _logger.LogWarning($"Invalid order specified: {order}. Defaulting to no specific order.");
                        break;
                }
            }

            var entities = await query.ToListAsync();

            if (entities == null || !entities.Any())
                _logger.LogWarning($"No entities of type {typeof(T).Name} found with the specified condition");
            return entities;
        }

        /// <summary>
        /// Retrieves a paginated and filtered list of entities from the database.
        /// </summary>
        /// <remarks>This method dynamically applies filters and includes navigation properties based on
        /// the provided parameters. If a specified navigation property in <paramref name="includings"/> does not exist
        /// on the entity type, a warning is logged, and the property is ignored.  The method orders the results by the
        /// first filter's property name if filters are provided and a valid <paramref name="order"/> value is
        /// specified. If no valid order is provided, the results are returned without a specific order.</remarks>
        /// <typeparam name="T">The type of the entity to query. Must be a class.</typeparam>
        /// <param name="propertyFilters">A list of <see cref="PropertyFilter"/> objects that define the filtering criteria. Each filter specifies a
        /// property name and a condition to apply.</param>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items to include in each page. Must be greater than 0.</param>
        /// <param name="order">Specifies the sort order for the results. Use <see langword="1"/> for ascending order and <see
        /// langword="2"/> for descending order. Defaults to <see langword="1"/>.</param>
        /// <param name="includings">A list of navigation property names to include in the query. These properties must exist on the entity type
        /// <typeparamref name="T"/>. If <see langword="null"/>, no navigation properties are included.</param>
        /// <returns>A tuple containing the following: <list type="bullet"> <item> <description> <see cref="List{T}"/>: The list
        /// of entities that match the specified filters and pagination criteria. </description> </item> <item>
        /// <description> <see cref="int"/>: The total count of entities that match the specified filters, ignoring
        /// pagination. </description> </item> </list></returns>
        public async Task<(List<T> Items, int TotalCount)> FilterPaginatedAsync<T>(List<PropertyFilter> propertyFilters, int pageNumber, int pageSize, int order = 1, List<string> includings = null) where T : class
        {
            var query = _context.Set<T>().AsQueryable();

            if (includings != null)
            {
                var entityType = typeof(T);
                foreach (var including in includings)
                {
                    // Check if the navigation property exists (case-insensitive)
                    var prop = entityType.GetProperty(including, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (prop != null)
                        query = query.Include(including);
                    else
                        _logger.LogWarning($"Navigation property '{including}' does not exist on entity '{entityType.Name}'.");
                }
            }

            // Apply filters dynamically
            foreach (var filter in propertyFilters)
            {
                if (String.IsNullOrEmpty(filter.PropertyName) || filter.Value == null)
                    continue;
                var predicate = BuildPredicate<T>(filter);
                query = query.Where(predicate);
            }

            // Order by the first filter property, if it exists
            if (propertyFilters.Count > 0)
            {
                var firstFilter = propertyFilters[0];
                switch (order)
                {
                    case 1:
                        query = query.OrderBy(e => EF.Property<object>(e, firstFilter.PropertyName));
                        break;
                    case 2:
                        query = query.OrderByDescending(e => EF.Property<object>(e, firstFilter.PropertyName));
                        break;
                    default:
                        _logger.LogWarning($"Invalid order specified: {order}. Defaulting to no specific order.");
                        break;
                }
            }

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

        private static Expression<Func<T, bool>> BuildPredicate<T>(PropertyFilter filter)
        {
            var param = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(param, filter.PropertyName);
            var constant = Expression.Constant(filter.Value);
            Expression body;

            switch (filter.Operator)
            {
                case "==":
                    body = Expression.Equal(property, Expression.Convert(constant, property.Type));
                    break;
                case "!=":
                    body = Expression.NotEqual(property, Expression.Convert(constant, property.Type));
                    break;
                case ">":
                    body = Expression.GreaterThan(property, Expression.Convert(constant, property.Type));
                    break;
                case "<":
                    body = Expression.LessThan(property, Expression.Convert(constant, property.Type));
                    break;
                case ">=":
                    body = Expression.GreaterThanOrEqual(property, Expression.Convert(constant, property.Type));
                    break;
                case "<=":
                    body = Expression.LessThanOrEqual(property, Expression.Convert(constant, property.Type));
                    break;
                default:
                    throw new NotSupportedException($"Operador '{filter.Operator}' no soportado.");
            }

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}
