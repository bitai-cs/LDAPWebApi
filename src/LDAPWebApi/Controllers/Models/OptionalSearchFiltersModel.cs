using Bitai.LDAPHelper.DTO;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bitai.LDAPWebApi.Controllers.Models;

public class OptionalSearchFiltersModel
{
	/// <summary>
	/// Default constructor.
	/// DO NOT REMOVE. Commonly used for deserialization of data.
	/// </summary>
	public OptionalSearchFiltersModel()
	{
		//
	}

	public OptionalSearchFiltersModel(EntryAttribute filterAttribute, string filterValue)
	{
		this.filterAttribute = filterAttribute;
		this.filterValue = filterValue;
		this.secondFilterAttribute = null;
		this.secondFilterValue = null;
		this.combineFilters = null;
	}




	/// <summary>
	/// <see cref="EntryAttribute"/>
	/// </summary>
	/// <remarks>
	/// Do not add [Required] annotation. It is not required to bind part of the URL query string to this model.
	/// </remarks>
	public EntryAttribute? filterAttribute { get; set; }

	/// <summary>
	/// Filter value
	/// </summary>
	/// /// <remarks>
	/// Do not remove [Required] annotation. It is required to bind part of the URL query string to this model.
	/// </remarks>
	[Required]
	public string filterValue { get; set; }

	public EntryAttribute? secondFilterAttribute { get; set; }

	public string? secondFilterValue { get; set; }

	public bool? combineFilters { get; set; }

}
