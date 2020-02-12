using System.Collections.Generic;

namespace CourseLibrary.API.Services
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<Tsource, TDestination>();
        bool ValidMappingExistsFor<Tsource, TDestination>(string fields);
    }
}