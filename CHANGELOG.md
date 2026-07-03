# Changelog

All notable changes to this project will be documented in this file.

## [2.0.0] - 2026-07-03

### Breaking changes

* Renamed public marker interfaces to the conventional `I` prefix:
  * `State` -> `IState`
  * `RootState` -> `IRootState`
  * `Action` / `Action<TState, TRootState>` -> `IAction` / `IAction<TState, TRootState>`
  * `Mutation` / `Mutation<TState, TPayload>` -> `IMutation` / `IMutation<TState, TPayload>`
  * `Module` -> `IModule`
* Removed the `StateManagementSharp.Action<...>` naming collision with `System.Action`.

### Added

* Added `StateChanged` notifications on `Store` and `ModuleBase`.
* Added `Observe(selector, onChanged)` with initial emission and distinct-until-changed semantics.
* Added the companion `StateManagementSharp.Maui` package.
* Added `StoreBindableObject` and `BindableValue<T>` for automatic UI-thread-marshaled MAUI bindings.
* Added a reactive MAUI sample demonstrating `Bind(...)` and `Observe(...)`.

### Changed

* Updated README with migration notes, reactive state usage, MAUI integration, and known limitations.
* Updated CI and NuGet publishing workflow for the MAUI package.

### Known limitations

* Source-generated registration for full iOS AOT/trimming is not implemented yet.
* Redux DevTools, middleware, persistence, and time-travel debugging are not implemented.
* Action/mutation discovery still expects store, modules, actions, and mutations to live in the same assembly.

## [1.1.0] - 2026-07-03

### Changed

* Added multi-targeting for `netstandard2.0`, `net8.0`, and `net10.0`.
* Improved NuGet/package metadata.
* Added XML documentation output.
* Cleaned up source comments and removed unused public API.
* Reworked README positioning and adoption-focused documentation.

## [1.0.0] - 2026-05-27

### Added

* Initial release of StateManagementSharp.
* Store, module, action, mutation, and dependency injection based state management model.
* MAUI sample app.
* Unit tests and NuGet packaging workflow.
