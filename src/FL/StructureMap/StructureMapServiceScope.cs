using System;
using Microsoft.Framework.DependencyInjection;
using StructureMap;

namespace FL.StructureMap
{
    public class StructureMapServiceScope : IServiceScope
    {
        public StructureMapServiceScope(IContainer container)
        {
            Container = container;
            ServiceProvider = container.GetInstance<IServiceProvider>();
        }

        private IContainer Container { get; }

        public void Dispose()
        {
            Container.Dispose();
        }

        public IServiceProvider ServiceProvider { get; }
    }
}