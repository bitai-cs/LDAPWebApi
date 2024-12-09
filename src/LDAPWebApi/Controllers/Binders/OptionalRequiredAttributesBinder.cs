using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.Binders;


/// <summary>
/// Provides a model binder for <see cref="LDAPHelper.DTO.RequiredEntryAttributes"/> model.
/// </summary>
/// <remarks>
/// This binder is used to bind a nullable <see cref="LDAPHelper.DTO.RequiredEntryAttributes"/> model.
/// If the model is not found, it assigns the default value <see cref="LDAPHelper.DTO.RequiredEntryAttributes.Few"/>.
/// </remarks>
public class OptionalRequiredAttributesBinder : IModelBinder
{
	/// <summary>
	/// Implements <see cref="IModelBinder"/> to bind a nullable <see cref="LDAPHelper.DTO.RequiredEntryAttributes"/> model.
	/// If the model is not found, it assigns the default value <see cref="LDAPHelper.DTO.RequiredEntryAttributes.Few"/>.
	/// </summary>
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		if (bindingContext == null)
			throw new ArgumentNullException(nameof(bindingContext));

		var defaultRequiredAttributes = LDAPHelper.DTO.RequiredEntryAttributes.Few;

		var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;
		if (!modelType.Equals(typeof(LDAPHelper.DTO.RequiredEntryAttributes)))
			throw new InvalidOperationException($"{typeof(OptionalRequiredAttributesBinder).FullName} cannot bind {modelType.FullName} model type.");

		var modelInstance = new LDAPHelper.DTO.RequiredEntryAttributes?();

		if (bindingContext.ModelMetadata.Name != null && !bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelMetadata.Name))
		{
			//Assign default value.
			modelInstance = LDAPHelper.DTO.RequiredEntryAttributes.Few;

			bindingContext.Result = ModelBindingResult.Success(modelInstance);

			return Task.CompletedTask;
		}

		var argumentValue = bindingContext.ModelMetadata.Name == null ? null : bindingContext.ValueProvider.GetValue(bindingContext.ModelMetadata.Name).FirstOrDefault();

		LDAPHelper.DTO.RequiredEntryAttributes requiredEntryAttributes;
		if (string.IsNullOrEmpty(argumentValue))
			requiredEntryAttributes = defaultRequiredAttributes;
		else
			if (!Enum.TryParse(argumentValue, true, out requiredEntryAttributes))
				throw new BadRequestException($"Cannot get '{nameof(LDAPHelper.DTO.RequiredEntryAttributes)}' from value '{argumentValue}'");

		modelInstance = requiredEntryAttributes;

		bindingContext.Result = ModelBindingResult.Success(modelInstance);

		return Task.CompletedTask;
	}
}
