using System;
using Microsoft.Extensions.DependencyInjection;

namespace StateManagementSharp
{
    public class ServiceProviderActionFactory : ActionFactory
    {
        private IServiceProvider ServiceProvider { get; }

        public ServiceProviderActionFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public Action? CreateAction<A>() where A : Action
        {
            return ServiceProvider.GetRequiredService<A>();
        }
    }
}
