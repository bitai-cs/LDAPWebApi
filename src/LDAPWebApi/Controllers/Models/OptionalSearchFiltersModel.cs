using Bitai.LDAPHelper.DTO;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bitai.LDAPWebApi.Controllers.Models;

/// <summary>
/// Represents optional filters for searching LDAP entries.
/// </summary>
public class OptionalSearchFiltersModel
{
	/// <summary>
	/// Default constructor.
	/// DO NOT REMOVE. Commonly used for deserialization of data.
	/// </summary>
	public OptionalSearchFiltersModel()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OptionalSearchFiltersModel"/> class.
	/// </summary>
	/// <param name="filterAttribute">The attribute to filter by.</param>
	/// <param name="filterValue">The value to filter by.</param>
	/// <remarks>
	/// This constructor is used to filter by a single attribute.
	/// </remarks>
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
	public required string filterValue { get; set; }

	/// <summary>
	/// <see cref="EntryAttribute"/>
	/// Only one of this and <see cref="secondFilterAttribute"/> can be provided.
	/// </summary>
	/// <remarks>
	/// Do not add [Required] annotation. It is not required to bind part of the URL query string to this model.
	/// </remarks>
	public EntryAttribute? secondFilterAttribute { get; set; }

	/// <summary>
	/// Filter value for the second filter condition.
	/// Only one of this and <see cref="secondFilterAttribute"/> can be provided.
	/// </summary>
	/// <remarks>
	/// Do not add [Required] annotation. It is not required to bind part of the URL query string to this model.
	/// </remarks>
	public string? secondFilterValue { get; set; }

	
	/// <summary>
	/// If <see langword="true"/>, the search will be combined using logical AND between the two filters.
	/// If <see langword="false"/>, the search will be combined using logical OR between the two filters.
	/// If <see langword="null"/>, the <see cref="secondFilterAttribute"/> will be ignored.
	/// Only one of this and <see cref="secondFilterAttribute"/> can be provided.
	/// </summary>
	/// <remarks>
	/// Do not add [Required] annotation. It is not required to bind part of the URL query string to this model.
	/// </remarks>
	public bool? combineFilters { get; set; }
}
