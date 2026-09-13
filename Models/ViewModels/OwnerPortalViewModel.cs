using System.Collections.Generic;
using System.Linq;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Models.ViewModels
{
    public class OwnerPortalViewModel
    {
        public Owners Owner { get; set; }
        public List<Property> Properties { get; set; }

        public int VacantCount => Properties.Count(p => p.VacancyStatus);
        public int OccupiedCount => Properties.Count - VacantCount;
        public decimal TotalMonthlyRent => Properties.Sum(p => p.MonthlyRent);
    }
}
