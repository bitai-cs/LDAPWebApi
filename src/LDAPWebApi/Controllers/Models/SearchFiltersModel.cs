using System.ComponentModel.DataAnnotations;

namespace Bitai.LDAPWebApi.Controllers.Models
{
    public class SearchFiltersModel
    {
        [Required]
        public LDAPHelper.DTO.EntryAttribute? filterAttribute { get; set; }

        [Required]
        public string filterValue { get; set; }

        public LDAPHelper.DTO.EntryAttribute? secondFilterAttribute { get; set; }

        public string secondFilterValue { get; set; }

        public bool? combineFilters { get; set; }

    }
}
