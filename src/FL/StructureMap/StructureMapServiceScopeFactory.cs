using Microsoft.Framework.DependencyInjection;
using StructureMap;

namespace FL.StructureMap
{
    public class StructureMapServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IContainer _container;

        public StructureMapServiceScopeFactory(IContainer container)
        {
            _container = container;
        }

        public IServiceScope CreateScope()
        {
            return new StructureMapServiceScope(_container.GetNestedContainer());
        }
    }
}