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

        public IAction? CreateAction<A>() where A : IAction
        {
            return ServiceProvider.GetRequiredService<A>();
        }
    }
}
