using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AtlasPremierProperties.Models.Entities
{
    public class Owners
    {
        // Add comments
        public int OwnerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        public string FullName { get { return FirstName + " " + LastName; } }
    }

}