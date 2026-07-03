using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StateManagementSharp.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetAllImplementingTypes(this Type openGenericType, Assembly assembly)
        {
            return from x in assembly.GetTypes()
                   from z in x.GetInterfaces()
                   let y = x.BaseType
                   where
                      (y != null && y.IsGenericType &&
                       openGenericType.IsAssignableFrom(y.GetGenericTypeDefinition())) ||
                      (z.IsGenericType &&
                       openGenericType.IsAssignableFrom(z.GetGenericTypeDefinition()))
                   select x;
        }
    }
}
