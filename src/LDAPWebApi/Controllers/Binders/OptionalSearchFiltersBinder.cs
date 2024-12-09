using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.Binders;

/// <summary>
/// Implements <see cref="IModelBinder"/> to bind <see cref="Models.OptionalSearchFiltersModel"/> 
/// from query string.
/// </summary>
public class OptionalSearchFiltersBinder : IModelBinder
{
	/// <summary>
	/// Implementation of <see cref="IModelBinder.BindModelAsync(ModelBindingContext)"/>.
	/// </summary>
	/// <param name="bindingContext">See <see cref="ModelBindingContext"/>.</param>
	/// <returns><see cref="Task.CompletedTask"/>.</returns>
	/// <exception cref="ArgumentNullException">When <paramref name="bindingContext"/> is null.</exception>
	/// <exception cref="InvalidOperationException">When data mocdel is not <see cref="Models.OptionalSearchFiltersModel"/>.</exception>
	/// <exception cref="InvalidCastException">When a value for a data model property cannot be interpreted.</exception>
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		if (bindingContext == null)
			throw new ArgumentNullException(nameof(bindingContext));

		var queryStringDictionary = bindingContext.HttpContext.Request.Query;

		var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;
		if (!modelType.Equals(typeof(Models.OptionalSearchFiltersModel)))
			throw new InvalidOperationException($"{typeof(OptionalSearchFiltersBinder).FullName} cannot bind  {modelType.FullName} model type.");

		var modelInstance = System.Activator.CreateInstance(modelType);

		var bindRequiredAttributeType = typeof(RequiredAttribute);

		foreach (var property in modelType.GetProperties())
		{
			var propertyValueRequired = (property.GetCustomAttributes(bindRequiredAttributeType, false).Count() > 0);

			var propertyValue = string.Empty;
			if (!queryStringDictionary.ContainsKey(property.Name))
			{
				if (propertyValueRequired)
				{
					bindingContext.ModelState.AddModelError(property.Name, $"{property.Name} is required");
				}
			}
			else
			{
				if (!queryStringDictionary.TryGetValue(property.Name, out var argumentValues))
				{
					if (propertyValueRequired)
					{
						bindingContext.ModelState.AddModelError(property.Name, $"Cannot get the value of parameter {property.Name} from the query string.");
					}
				}
				else if (argumentValues.Count == 0)
				{
					if (propertyValueRequired)
					{
						bindingContext.ModelState.AddModelError(property.Name, $"{property.Name} is required");
					}
				}
				else
				{
					propertyValue = argumentValues.ToString();
				}
			}

			if (propertyValueRequired && string.IsNullOrEmpty(propertyValue))
			{
				bindingContext.ModelState.AddModelError(property.Name, $"Value of query string parameter {property.Name} is required.");
			}
			else if (!propertyValueRequired && string.IsNullOrEmpty(propertyValue))
			{
				if (property.Name.Equals(nameof(Models.OptionalSearchFiltersModel.filterAttribute)))
					propertyValue = LDAPHelper.DTO.EntryAttribute.sAMAccountName.ToString();
			}

			if (property.Name.Equals(nameof(Models.OptionalSearchFiltersModel.filterAttribute)) || property.Name.Equals(nameof(Models.OptionalSearchFiltersModel.secondFilterAttribute)))
			{
				var parseStatus = Enum.TryParse<LDAPHelper.DTO.EntryAttribute>(propertyValue, true, out var filterAttributeEnum);

				if (!parseStatus && propertyValueRequired)
					throw new BadRequestException($"'{propertyValue}' is not a valid value for query string parameter  {property.Name}.");
				else if (!parseStatus && !propertyValueRequired && !string.IsNullOrEmpty(propertyValue))
					throw new BadRequestException($"'{propertyValue}' is not a valid value for query string parameter  {property.Name}.");

				if (parseStatus)
					property.SetValue(modelInstance, filterAttributeEnum);
				else
					property.SetValue(modelInstance, new LDAPHelper.DTO.EntryAttribute?());
			}
			else if (property.Name.Equals(nameof(Models.OptionalSearchFiltersModel.combineFilters)))
			{
				var parseStatus = bool.TryParse(propertyValue, out var combineFilters);

				if (!parseStatus && propertyValueRequired)
					throw new BadRequestException($"'{propertyValue}' is not a valid value for {property.Name}.");

				if (parseStatus)
					property.SetValue(modelInstance, combineFilters);
				else
					property.SetValue(modelInstance, new bool?());
			}
			else
			{
				property.SetValue(modelInstance, propertyValue);
			}
		}

		if (bindingContext.ModelState.IsValid)
		{
			var secondFilterAttribute = modelType.GetProperty(nameof(Models.OptionalSearchFiltersModel.secondFilterAttribute))!.GetValue(modelInstance);

			if (secondFilterAttribute != null)
			{
				var secondFilterValue = modelType.GetProperty(nameof(Models.OptionalSearchFiltersModel.secondFilterValue))!.GetValue(modelInstance);

				if (secondFilterValue == null || secondFilterValue.ToString() == string.Empty)
					bindingContext.ModelState.AddModelError($"{nameof(Models.OptionalSearchFiltersModel.secondFilterValue)}", $"{nameof(Models.OptionalSearchFiltersModel.secondFilterAttribute)} has been set, therefore {nameof(Models.OptionalSearchFiltersModel.secondFilterValue)} is required.");

				var combineFilters = modelType.GetProperty(nameof(Models.OptionalSearchFiltersModel.combineFilters))!.GetValue(modelInstance);

				if (combineFilters == null)
					bindingContext.ModelState.AddModelError($"{nameof(Models.OptionalSearchFiltersModel.combineFilters)}", $"{nameof(Models.OptionalSearchFiltersModel.secondFilterAttribute)} has been set, therefore {nameof(Models.OptionalSearchFiltersModel.combineFilters)} is required.");
			}
		}

		if (bindingContext.ModelState.IsValid)
			bindingContext.Result = ModelBindingResult.Success(modelInstance);

		return Task.CompletedTask;
	}
}
