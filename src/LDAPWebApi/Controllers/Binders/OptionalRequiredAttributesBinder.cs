using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.Binders
{
	public class OptionalRequiredAttributesBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (bindingContext == null)
				throw new ArgumentNullException(nameof(bindingContext));

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

			if (!Enum.TryParse<LDAPHelper.DTO.RequiredEntryAttributes>(argumentValue, true, out var requiredEntryAttributes))
				throw new InvalidCastException($"Cannot get '{nameof(LDAPHelper.DTO.RequiredEntryAttributes)}' from value '{argumentValue}'");

			modelInstance = requiredEntryAttributes;

			bindingContext.Result = ModelBindingResult.Success(modelInstance);

			return Task.CompletedTask;
		}
	}
}
