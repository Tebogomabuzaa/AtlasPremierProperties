using System.Collections.Generic;
using AtlasPremierProperties.Models.Entities;

namespace AtlasPremierProperties.Repositories
{
    public interface IPropertyManagerRepository
    {
        List<PropertyManager> GetAll();
        PropertyManager GetById(int managerId);
        bool EmailExists(string email, int? excludeManagerId = null);
        int Add(PropertyManager manager);
        void Update(PropertyManager manager);
        void Delete(int managerId);
    }
}
