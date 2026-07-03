using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using StateManagementSharp.Extensions;

namespace StateManagementSharp.Maui
{
    /// <summary>
    /// MAUI host integration for StateManagementSharp.
    /// </summary>
    public static class MauiStateManagementExtensions
    {
        /// <summary>
        /// Registers StateManagementSharp factories, scans <paramref name="assemblies"/> (or the
        /// assembly of <typeparamref name="TStore"/> when none are supplied) for actions, and registers
        /// <typeparamref name="TStore"/> as a singleton.
        /// </summary>
        public static MauiAppBuilder UseStateManagementSharp<TStore>(this MauiAppBuilder builder, params Assembly[] assemblies)
            where TStore : class
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            var scanAssemblies = assemblies is { Length: > 0 } ? assemblies : new[] { typeof(TStore).Assembly };

            builder.Services.AddStateManagementSharp(scanAssemblies);
            builder.Services.AddSingleton<TStore>();

            return builder;
        }
    }
}
