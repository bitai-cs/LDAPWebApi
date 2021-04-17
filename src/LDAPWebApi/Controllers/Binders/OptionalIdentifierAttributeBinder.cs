using Bitai.LDAPHelper.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.Binders
{
    public class OptionalIdentifierAttributeBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;
            if (!modelType.Equals(typeof(EntryAttribute)))
                throw new InvalidOperationException($"{typeof(OptionalIdentifierAttributeBinder).FullName} cannot bind {modelType.FullName} model type.");

            var modelInstance = new EntryAttribute?();

            if (!bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelMetadata.Name))
            {
                //Assign default value
                modelInstance = EntryAttribute.sAMAccountName;

                bindingContext.Result = ModelBindingResult.Success(modelInstance);

                return Task.CompletedTask;
            }

            var argumentValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelMetadata.Name).FirstOrDefault();

            if (!Enum.TryParse<EntryAttribute>(argumentValue, true, out var entryAttribute))
                throw new InvalidCastException($"Cannot get '{nameof(EntryAttribute)}' from value '{argumentValue}'");

            modelInstance = entryAttribute;

            bindingContext.Result = ModelBindingResult.Success(modelInstance);

            return Task.CompletedTask;
        }
    }
}
