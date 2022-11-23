using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.Binders;

/// <summary>
/// Implements <see cref="IModelBinder"/>
/// </summary>
public class SearchFiltersBinder : IModelBinder
{
	/// <summary>
	/// Implementation of <see cref="IModelBinder.BindModelAsync(ModelBindingContext)"/>.
	/// </summary>
	/// <param name="bindingContext">See <see cref="ModelBindingContext"/>.</param>
	/// <returns><see cref="Task.CompletedTask"/>.</returns>
	/// <exception cref="ArgumentNullException">When <paramref name="bindingContext"/> is null.</exception>
	/// <exception cref="InvalidOperationException">When data mocdel is not <see cref="Models.SearchFiltersModel"/>.</exception>
	/// <exception cref="InvalidCastException">When a value for a data model property cannot be interpreted.</exception>
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		if (bindingContext == null)
			throw new ArgumentNullException(nameof(bindingContext));

		var queryStringDictionary = bindingContext.HttpContext.Request.Query;

		var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;
		if (!modelType.Equals(typeof(Models.SearchFiltersModel)))
			throw new InvalidOperationException($"{typeof(SearchFiltersBinder).FullName} cannot bind  {modelType.FullName} model type.");

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
						bindingContext.ModelState.AddModelError(property.Name, $"Cannot get value for {property.Name}");
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
				bindingContext.ModelState.AddModelError(property.Name, $"{property.Name} is required");
			}

			if (property.Name.Equals(nameof(Models.SearchFiltersModel.filterAttribute)) || property.Name.Equals(nameof(Models.SearchFiltersModel.secondFilterAttribute)))
			{
				var parseStatus = Enum.TryParse<LDAPHelper.DTO.EntryAttribute>(propertyValue, true, out var filterAttributeEnum);

				if (!parseStatus && propertyValueRequired)
					throw new BadRequestException($"'{propertyValue}' not valid for {property.Name}");

				if (parseStatus)
					property.SetValue(modelInstance, filterAttributeEnum);
				else
					property.SetValue(modelInstance, new LDAPHelper.DTO.EntryAttribute?());
			}
			else if (property.Name.Equals(nameof(Models.SearchFiltersModel.combineFilters)))
			{
				var parseStatus = bool.TryParse(propertyValue, out var combineFilters);

				if (!parseStatus && propertyValueRequired)
					throw new BadRequestException($"'{propertyValue}' not valid for {property.Name}");

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
			var secondFilterAttribute = modelType.GetProperty(nameof(Models.SearchFiltersModel.secondFilterAttribute))!.GetValue(modelInstance);

			if (secondFilterAttribute != null)
			{
				var secondFilterValue = modelType.GetProperty(nameof(Models.SearchFiltersModel.secondFilterValue))!.GetValue(modelInstance);

				if (secondFilterValue == null || secondFilterValue.ToString() == string.Empty)
					bindingContext.ModelState.AddModelError($"{nameof(Models.SearchFiltersModel.secondFilterValue)}", $"{nameof(Models.SearchFiltersModel.secondFilterAttribute)} has been set, therefore {nameof(Models.SearchFiltersModel.secondFilterValue)} is required.");

				var combineFilters = modelType.GetProperty(nameof(Models.SearchFiltersModel.combineFilters))!.GetValue(modelInstance);

				if (combineFilters == null)
					bindingContext.ModelState.AddModelError($"{nameof(Models.SearchFiltersModel.combineFilters)}", $"{nameof(Models.SearchFiltersModel.secondFilterAttribute)} has been set, therefore {nameof(Models.SearchFiltersModel.combineFilters)} is required.");
			}
		}

		if (bindingContext.ModelState.IsValid)
			bindingContext.Result = ModelBindingResult.Success(modelInstance);

		return Task.CompletedTask;
	}
}
