using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.Binders
{
	public class OptionalQueryStringBinder : IModelBinder
	{
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
}
