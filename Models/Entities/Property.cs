using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AtlasPremierProperties.Models.Entities
{
    // Add comment
    public class Property
    {
        public int PropertyID { get; set; }
        public string PropertyDescription { get; set; }
        public string PhysicalAddress { get; set; }
        public int OwnerID { get; set; }
        public decimal MonthlyRent { get; set; }
        public bool VacancyStatus { get; set; }

        // Not a database column - populated via JOIN
        public string OwnerName { get; set; }
    }
}