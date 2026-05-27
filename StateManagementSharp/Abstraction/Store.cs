using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using StateManagementSharp.Exceptions;
using StateManagementSharp.Extensions;

namespace StateManagementSharp
{
    //QA-AS-2018-11-18
    public abstract class Store<TR> where TR : RootState
    {

        #region auto-properties

        public TR? State { get; protected set; }

        private bool WasCacheBuilt { get; set; }
        private Dictionary<Type, MethodInfo>? DispatchDictionary { get; set; }
        private Dictionary<Type, MethodInfo>? CommitDictionary { get; set; }

        private Dictionary<Type, Type>? ActionModuleDict { get; set; }
        private Dictionary<Type, Type>? MutationModuleDict { get; set; }
        private Dictionary<Type, Type>? CommitPayloadDictionary { get; set; }


        private Dictionary<Type, PropertyInfo>? ModulePropertyDict { get; set; }

        protected IEnumerable<StateManagementSharp.Core.Module>? Modules { get; set; }

        #endregion

        #region abstract methods

        protected abstract void BindModules();

        #endregion

        #region access methods

        public void BootstrapModules()
        {
            if (Modules is not null && Modules.Any())
            {
                foreach (var module in Modules)
                {
                    module.DisposeState();
                    module.CreateState();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No Modules have been created");
            }
        }

        //QA-AS-2018-11-18
        //throws
        public async Task Dispatch(string actionName)
        {
            await Dispatch(actionName, null);
        }

        public async Task Dispatch(string actionName, object? payload)
        {
            try
            {
                if (!WasCacheBuilt) BuildTypesCache();

                if (DispatchDictionary is null) throw new DispatchFailedException($"{nameof(DispatchDictionary)} is null.");
                if (ActionModuleDict is null) throw new DispatchFailedException($"{nameof(ActionModuleDict)} is null.");
                if (ModulePropertyDict is null) throw new DispatchFailedException($"{nameof(ModulePropertyDict)} is null.");

                bool wasDispatched = false;

                var key = DispatchDictionary.Keys.FirstOrDefault(k => k.Name == actionName);
                if (key != null)
                {
                    if (DispatchDictionary.TryGetValue(key, out var method))
                    {
                        var generic = method.MakeGenericMethod(key);

                        if (ActionModuleDict.TryGetValue(key, out var actionModuleType))
                        {
                            if (ModulePropertyDict.ContainsKey(actionModuleType))
                            {
                                var moduleInstance = ModulePropertyDict[actionModuleType].GetValue(this);
                                if (moduleInstance is not null)
                                {
                                    // TODO IMPROVE THIS
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                                    await (Task)generic.Invoke(moduleInstance, [payload]);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.


                                    /*
									object? invokation = generic.Invoke(moduleInstance, [payload]);
									if (generic.ReturnType == typeof(Task) && invokation is Task task)
									{
										await task;
									}
									else if (generic.ReturnType.IsGenericType &&
									         generic.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
									{
										// Se è un Task<T>, otteniamo il valore generico
										var resultProperty = generic.ReturnType.GetProperty("Result");
										var taskResult = resultProperty?.GetValue(invokation);
										Console.WriteLine($"Task<T> restituisce: {taskResult}");
									}
									else if (generic.ReturnType == typeof(void) || invokation is null)
									{
										// Il metodo è void o restituisce null
										Console.WriteLine($"Il metodo è void o restituisce null");
									}
									else
									{
										// Qualsiasi altro tipo (string, int, bool, object, etc.)
										Console.WriteLine(
											$"Il metodo ha restituito un valore di tipo {invokation.GetType()}: {invokation}");
									}
									*/

                                    wasDispatched = true;
                                }
                            }
                        }
                    }
                }

                if (!wasDispatched)
                {
                    throw new DispatchFailedException($"Action {actionName} is not implemented");
                }
            }
            catch (DispatchFailedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DispatchFailedException(ex.Message, ex);
            }
        }

        //QA-AS-2018-11-18
        //throws
        public void Commit(string mutationName, object? payload)
        {
            try
            {
                if (!WasCacheBuilt) BuildTypesCache();

                if (CommitDictionary is null) throw new CommitFailedException($"{nameof(CommitDictionary)} is null.");
                if (MutationModuleDict is null)
                    throw new CommitFailedException($"{nameof(MutationModuleDict)} is null.");
                if (CommitPayloadDictionary is null)
                    throw new CommitFailedException($"{nameof(CommitPayloadDictionary)} is null.");
                if (ModulePropertyDict is null)
                    throw new CommitFailedException($"{nameof(ModulePropertyDict)} is null.");

                bool wasCommitted = false;

                var key = CommitDictionary.Keys.FirstOrDefault(k => k.Name == mutationName);
                if (key != null)
                {
                    var method = CommitDictionary[key];
                    var payloadType = CommitPayloadDictionary[key];
                    var generic = method.MakeGenericMethod(key, payloadType);

                    if (MutationModuleDict.TryGetValue(key, out var mutationModuleType))
                    {
                        if (ModulePropertyDict.ContainsKey(mutationModuleType))
                        {
                            var moduleInstance = ModulePropertyDict[mutationModuleType].GetValue(this);

                            //throws
                            generic.Invoke(moduleInstance, [payload]);

                            wasCommitted = true;

                        }
                    }

                }

                if (!wasCommitted)
                {
                    throw new CommitFailedException($"Mutation {mutationName} is not implemented");
                }
            }
            catch (CommitFailedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CommitFailedException(ex.Message, ex);
            }
        }

        public abstract void InitState();

        #endregion

        #region helper methods



        //QA-AS-2018-11-18
        private void BuildTypesCache()
        {
            WasCacheBuilt = true;

            DispatchDictionary = new Dictionary<Type, MethodInfo>();
            CommitDictionary = new Dictionary<Type, MethodInfo>();
            CommitPayloadDictionary = new Dictionary<Type, Type>();

            ActionModuleDict = new Dictionary<Type, Type>();
            MutationModuleDict = new Dictionary<Type, Type>();

            ModulePropertyDict = new Dictionary<Type, PropertyInfo>();

            var actionType = typeof(Action<,>);
            var moduleType = typeof(ModuleBase<,>);
            var mutationType = typeof(Mutation<,>);

            var thisType = this.GetType();
            var thisAssembly = thisType.Assembly;

            var modules = thisAssembly.GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == moduleType);

            var moduleStateDict = new Dictionary<Type, Type>();

            foreach (var module in modules)
            {
                var props = module.GetProperties();
                foreach (var prop in props)
                {
                    if (typeof(State).IsAssignableFrom(prop.PropertyType))
                    {
                        moduleStateDict.Add(prop.PropertyType, module);
                    }
                }
            }

            var mutations = mutationType.GetAllImplementingTypes(thisAssembly);
            foreach (var mutation in mutations)
            {
                var interf = mutation.GetInterface(mutationType.Name);
                if (interf is not null)
                {
                    foreach (var genericTypeArgument in interf.GetGenericArguments())
                    {
                        if (moduleStateDict.TryGetValue(genericTypeArgument, out var dispatchedModule))
                        {
                            MutationModuleDict.Add(mutation, dispatchedModule);

                            var method = dispatchedModule.GetMethods()
                                .FirstOrDefault(m => m is { Name: "Commit", IsGenericMethod: true });
                            if (method != null)
                            {
                                CommitDictionary.Add(mutation, method);


                            }
                        }
                        else
                        {
                            CommitPayloadDictionary.Add(mutation, genericTypeArgument);
                        }
                    }
                }
            }

            var actions = actionType.GetAllImplementingTypes(thisAssembly);
            foreach (var action in actions)
            {
                var interf = action.GetInterface(actionType.Name);
                if (interf is not null)
                {
                    foreach (var genericTypeArgument in interf.GetGenericArguments())
                    {
                        if (moduleStateDict.TryGetValue(genericTypeArgument, out var dispatchedModule))
                        {
                            ActionModuleDict.Add(action, dispatchedModule);

                            var method = dispatchedModule.GetMethod("Dispatch", [typeof(object)]);

                            if (method is not null) DispatchDictionary.Add(action, method);
                            //var generic = method.MakeGenericMethod(action);
                        }
                    }
                }
            }

            var rootStateProps = this
             .GetType()
             .GetProperties();

            foreach (var rootStatepProp in rootStateProps)
            {
                var propType = rootStatepProp.PropertyType;
                if (propType.BaseType != null && propType.BaseType.IsGenericType && propType.BaseType.GetGenericTypeDefinition() == moduleType)
                {
                    ModulePropertyDict.Add(propType, rootStatepProp);
                }
            }

        }


        #endregion

        #region abstract methods

        public abstract void InitializeStore();

        #endregion
    }
}
