using System.Collections.Generic;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Models.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public List<Property> Properties { get; set; }
        public List<Tenant> Tenants { get; set; }
        public List<LeaseAgreement> Leases { get; set; }
        public List<Owners> Owners { get; set; }

        public int Total
        {
            get { return Properties.Count + Tenants.Count + Leases.Count + Owners.Count; }
        }

        public SearchViewModel()
        {
            Properties = new List<Property>();
            Tenants = new List<Tenant>();
            Leases = new List<LeaseAgreement>();
            Owners = new List<Owners>();
        }
    }
}
