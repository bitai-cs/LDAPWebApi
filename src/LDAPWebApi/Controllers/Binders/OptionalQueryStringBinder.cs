using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.Binders;


/// <summary>
/// Implements <see cref="IModelBinder"/> to bind nullable string model properties 
/// from query string.
/// </summary>
public class OptionalQueryStringBinder : IModelBinder
{
	/// <summary>
	/// Implementation of <see cref="IModelBinder"/>.
	/// </summary>
	/// <param name="bindingContext">See <see cref="ModelBindingContext"/>.</param>
	/// <returns><see cref="Task.CompletedTask"/>See <see cref="Task.CompletedTask"/>.</returns>
	/// <exception cref="ArgumentNullException">When <paramref name="bindingContext"/> is null.</exception>
	/// <exception cref="InvalidOperationException">When data model is not of type <see cref="string"/>.</exception>
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		if (bindingContext == null)
			throw new ArgumentNullException(nameof(bindingContext));

		var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;
		if (!modelType.Equals(typeof(string)))
			throw new InvalidOperationException($"{typeof(OptionalQueryStringBinder).FullName} cannot bind  {modelType.FullName} model type.");

		var modelInstance = System.Activator.CreateInstance(modelType, new char[] { });

		if (bindingContext.ModelMetadata.Name != null && !bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelMetadata.Name))
		{
			bindingContext.Result = ModelBindingResult.Success(modelInstance);

			return Task.CompletedTask;
		}

		var argumentValues = bindingContext.ValueProvider.GetValue(bindingContext.ModelMetadata.Name!);

		modelInstance = argumentValues.FirstOrDefault();

		bindingContext.Result = ModelBindingResult.Success(modelInstance);

		return Task.CompletedTask;
	}
}
