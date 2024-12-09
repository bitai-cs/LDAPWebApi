using Bitai.LDAPHelper.DTO;
using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.Binders;


/// <summary>
/// Implements <see cref="IModelBinder"/> to bind optional <see cref="EntryAttribute"/> model.
/// If the model is not found, it assigns the default value <see cref="EntryAttribute.sAMAccountName"/>.
/// </summary>
public class OptionalIdentifierAttributeBinder : IModelBinder
{
	/// <summary>
	/// Implements <see cref="IModelBinder"/> to bind LDAPHelper.DTO.EntryAttribute model.
	/// If the model is not found, it assigns the default value.
	/// </summary>
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		if (bindingContext == null)
			throw new ArgumentNullException(nameof(bindingContext));

		var defaultIdentifierAttribute = EntryAttribute.sAMAccountName;

		var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;
		if (!modelType.Equals(typeof(EntryAttribute)))
			throw new InvalidOperationException($"{typeof(OptionalIdentifierAttributeBinder).FullName} cannot bind {modelType.FullName} model type.");

		var modelInstance = new EntryAttribute?();

		if (bindingContext.ModelMetadata.Name != null && !bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelMetadata.Name))
		{
			//Assign default value
			modelInstance = defaultIdentifierAttribute;

			bindingContext.Result = ModelBindingResult.Success(modelInstance);

			return Task.CompletedTask;
		}

		var argumentValue = bindingContext.ModelMetadata.Name == null ? null : bindingContext.ValueProvider.GetValue(bindingContext.ModelMetadata.Name).FirstOrDefault();

		EntryAttribute entryAttribute;
		if (string.IsNullOrEmpty(argumentValue))
			entryAttribute = defaultIdentifierAttribute;
		else
			if (!Enum.TryParse(argumentValue, true, out entryAttribute))
				throw new BadRequestException($"Cannot instantiate an '{nameof(EntryAttribute)}' using the value '{argumentValue}'");

		modelInstance = entryAttribute;

		bindingContext.Result = ModelBindingResult.Success(modelInstance);

		return Task.CompletedTask;
	}
}
