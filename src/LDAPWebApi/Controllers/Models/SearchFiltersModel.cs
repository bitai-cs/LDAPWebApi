using Bitai.LDAPHelper.DTO;
using System.ComponentModel.DataAnnotations;

namespace Bitai.LDAPWebApi.Controllers.Models;

/// <summary>
/// A model containing the filters for a search.
/// </summary>
public class SearchFiltersModel
{
	/// <summary>
	/// Default constructor.
	/// DO NOT REMOVE. Commonly used for deserialization of data.
	/// </summary>
	public SearchFiltersModel()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SearchFiltersModel"/> class.
	/// </summary>
	/// <param name="filterAttribute">The attribute to filter by.</param>
	/// <param name="filterValue">The value to filter by.</param>
	/// <remarks>
	/// This constructor is used to initialize the model with the specified filter attribute and value.
	/// </remarks>
	public SearchFiltersModel(EntryAttribute filterAttribute, string filterValue)
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
	/// Do not remove [Required] annotation, it is required to bind part of the URL query string to this model.
	/// </remarks>
	[Required]
	public required LDAPHelper.DTO.EntryAttribute filterAttribute { get; set; }

	/// <summary>
	/// Filter value
	/// </summary>
	/// /// <remarks>
	/// Do not remove [Required] annotation, it is required to bind part of the URL query string to this model.
	/// </remarks>
	[Required]
	public required string filterValue { get; set; }

	
	/// <summary>
	/// Second <see cref="EntryAttribute"/> that will condition the search according to the <see cref="secondFilterValue"/>.
	/// </summary>
	/// <remarks>
	/// This optional attribute is used to add a second filter to the search. The second filter will be combined with the first filter using the <see cref="combineFilters"/> value.
	/// </remarks>
	public LDAPHelper.DTO.EntryAttribute? secondFilterAttribute { get; set; }

	/// <summary>
	/// Second filter value.
	/// </summary>
	/// <remarks>
	/// This optional attribute is used to add a second filter to the search. The second filter will be combined with the first filter using the <see cref="combineFilters"/> value.
	/// </remarks>
	public string? secondFilterValue { get; set; }

	/// <summary>
	/// Whether it is combined in a conjunction or not the search filters.
	/// </summary>
	/// <remarks>
	/// This optional attribute is used to define the manner in which the search filters will be combined. 
	/// The search filters will be combined using the logical conjunction when the value is <c>true</c> and using the logical disjunction when the value is <c>false</c>.
	/// </remarks>
	public bool? combineFilters { get; set; }

}
