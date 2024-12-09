using Bitai.LDAPHelper.DTO;
using Bitai.WebApi.Server;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bitai.LDAPWebApi.Controllers.Binders;

/// <summary>
/// Implements <see cref="IModelBinder"/> to bind <see cref="EntryAttribute"/> model 
/// that identifies a user account.
/// If the model is not found, it assigns the default value <see cref="EntryAttribute.sAMAccountName"/>.
/// </summary>
public class OptionalUserAccountIdentifierAttributeBinder : IModelBinder
{
    /// <summary>
    /// Binds the specified <see cref="EntryAttribute"/> model property.
    /// If the model is not found, it assigns the default value <see cref="EntryAttribute.sAMAccountName"/>.
    /// </summary>
    /// <param name="bindingContext">The model binding context.</param>
    /// <returns>A task that represents the asynchronous model binding operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="bindingContext"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the model type is not <see cref="EntryAttribute"/>.</exception>
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		if (bindingContext == null)
			throw new ArgumentNullException(nameof(bindingContext));

		var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;
		if (!modelType.Equals(typeof(EntryAttribute)))
			throw new InvalidOperationException($"{typeof(OptionalIdentifierAttributeBinder).FullName} cannot bind {modelType.FullName} model type.");

		var modelInstance = new EntryAttribute?();

		if (bindingContext.ModelMetadata.Name != null && !bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelMetadata.Name))
		{
			//Assign default value
			modelInstance = EntryAttribute.sAMAccountName;

			bindingContext.Result = ModelBindingResult.Success(modelInstance);

			return Task.CompletedTask;
		}

		var argumentValue = bindingContext.ModelMetadata.Name == null ? null : bindingContext.ValueProvider.GetValue(bindingContext.ModelMetadata.Name).FirstOrDefault();

		EntryAttribute entryAttribute;
		if (string.IsNullOrEmpty(argumentValue))
			entryAttribute = EntryAttribute.sAMAccountName;
		else
			if (!Enum.TryParse(argumentValue, true, out entryAttribute))
				throw new BadRequestException($"Cannot instantiate an '{nameof(EntryAttribute)}' using the value '{argumentValue}'");

		if (!(entryAttribute == EntryAttribute.sAMAccountName || entryAttribute == EntryAttribute.distinguishedName))
			throw new WebApi.Server.BadRequestException($"The LDAP attribute to identify a user account must be only {EntryAttribute.sAMAccountName} or {EntryAttribute.distinguishedName}");

		modelInstance = entryAttribute;

		bindingContext.Result = ModelBindingResult.Success(modelInstance);

		return Task.CompletedTask;
	}
}
