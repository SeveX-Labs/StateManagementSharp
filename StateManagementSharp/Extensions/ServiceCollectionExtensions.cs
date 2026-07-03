using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StateManagementSharp.Extensions;

namespace StateManagementSharp
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStateManagementSharp(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies is null) throw new ArgumentNullException(nameof(assemblies));

            services.TryAddSingleton<MutationFactory, MutationFactoryActivator>();
            services.TryAddSingleton<StateFactory, StateFactoryActivator>();
            services.TryAddTransient<ActionFactory, ServiceProviderActionFactory>();

            var openActionType = typeof(IAction<,>);
            var actionTypes = assemblies
                .Where(assembly => assembly is not null)
                .SelectMany(assembly => openActionType.GetAllImplementingTypes(assembly))
                .Where(type => type is { IsClass: true, IsAbstract: false } && !type.ContainsGenericParameters)
                .Distinct();

            foreach (var actionType in actionTypes)
            {
                services.TryAdd(ServiceDescriptor.Transient(actionType, actionType));
            }

            return services;
        }
    }
}
