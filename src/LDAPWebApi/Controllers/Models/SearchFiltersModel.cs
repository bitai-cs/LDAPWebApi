using Bitai.LDAPHelper.DTO;
using System.ComponentModel.DataAnnotations;

namespace Bitai.LDAPWebApi.Controllers.Models;

public class SearchFiltersModel
{
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
    public LDAPHelper.DTO.EntryAttribute filterAttribute { get; set; }

    /// <summary>
    /// Filter value
    /// </summary>
    /// /// <remarks>
    /// Do not remove [Required] annotation, it is required to bind part of the URL query string to this model.
    /// </remarks>
    [Required]
    public string filterValue { get; set; }

    public LDAPHelper.DTO.EntryAttribute? secondFilterAttribute { get; set; }

    public string? secondFilterValue { get; set; }

    public bool? combineFilters { get; set; }

}
