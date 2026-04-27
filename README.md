# Monoservice Registry

A small service registry and locator for Unity.

Monoservice lets you register and retrieve global services with a minimal API. It supports both plain C# service classes and scene-based `MonoBehaviour` services using the same `[Service]` attribute.

## Features

- Simple static service registry
- `Registry.Get<T>()` lookup API
- Manual registration with `Registry.Register(Type, object)`
- Automatic registration for plain C# classes marked with `[Service]`
- Automatic scene discovery for `MonoBehaviour` components marked with `[Service]`
- Registry is cleared and bootstrapped before scene load
- Lightweight runtime-only package

## Installation

Install with Unity Package Manager using the Git URL:

```text
https://github.com/ZellyDev-Games/monoservice.git
```

Or add it to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.github.zellydev-games.monoservice": "https://github.com/ZellyDev-Games/monoservice.git"
  }
}
```

## Package Info

```json
{
  "name": "com.github.zellydev-games.monoservice",
  "version": "1.0.0",
  "displayName": "Monoservice Registry",
  "unity": "6000.3"
}
```

## Plain C# Services

Plain C# services are automatically created during bootstrap if they:

- are marked with `[Service]`
- are not abstract
- are not interfaces
- do not inherit from `MonoBehaviour`
- have a default constructor

```csharp
using ZellyDevGames.Monoservice;

[Service]
public class SaveService
{
    public void Save()
    {
        // Save game data
    }
}
```

Retrieve the service from anywhere:

```csharp
var saveService = Registry.Get<SaveService>();
saveService.Save();
```

## MonoBehaviour Services

`MonoBehaviour` services are discovered when a scene loads. Add the `[Service]` attribute to a component and place it on a GameObject in the scene.

```csharp
using UnityEngine;
using ZellyDevGames.Monoservice;

[Service]
public class AudioService : MonoBehaviour
{
    public void PlaySound()
    {
        // Play audio
    }
}
```

Then retrieve it:

```csharp
var audioService = Registry.Get<AudioService>();
audioService.PlaySound();
```

## Manual Registration

You can also register an instance manually:

```csharp
var service = new SaveService();

Registry.Register(typeof(SaveService), service);
```

The registered instance must be assignable to the type being registered.

## API

### `Registry.Register`

```csharp
public static bool Register(Type type, object instance)
```

Registers an instance for the given service type.

Returns `true` if registration succeeds. If a service of the same type is already registered, registration fails and a warning is logged.

### `Registry.Get<T>`

```csharp
public static T Get<T>()
```

Returns the registered service of type `T`.

Throws an exception if the service has not been registered.

## Lifecycle

Monoservice bootstraps before scene load. During bootstrap it:

1. clears the registry
2. scans loaded assemblies for plain C# classes marked with `[Service]`
3. creates those services with `Activator.CreateInstance`
4. subscribes to Unity scene-loaded events
5. scans loaded scene root objects for `[Service]` `MonoBehaviour` components

## Notes

- Only one service instance can be registered per concrete type.
- Plain C# services need a default constructor.
- `MonoBehaviour` services must exist in the loaded scene.
- Duplicate service registration logs a warning and keeps the first registered instance.
- `Registry.Get<T>()` throws if the requested service is missing.

## License

MIT