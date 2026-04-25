# Unity-Injector

Lightweight Dependency Injection container for Unity with a fluent binding API, lifetime management, and seamless Unity integration.

---

## Installation

### Unity Package Manager

Open the Unity Package Manager, click **Add package from git URL...** and paste:

```
https://github.com/ostrzolekpawel/UnityInjector.git?path=Assets/Injector
```

### manifest.json

Add the entry directly to your project's `manifest.json`:

```json
{
    "dependencies": {
        "com.osirisgames.dicontainer": "https://github.com/ostrzolekpawel/UnityInjector.git?path=Assets/Injector"
    }
}
```

---

## Architecture

UnityInjector uses a two-level container hierarchy that mirrors Unity's scene structure:

```
AppContext          (global, ExecutionOrder -5100)
  └─ DiContainer   (root: app-wide bindings)
        └─ SceneContext          (per-scene, ExecutionOrder -5000)
              └─ DiContainer     (scene - inherits root bindings)
                    └─ MonoBehaviourInjectable instances
```

- **AppContext** - a single `MonoBehaviour` placed in your bootstrap/persistent scene. Runs all `MonoInstaller`s and exposes `AppContext.Container`.
- **SceneContext** - placed in each scene. Creates a child container (parent = `AppContext.Container`) so scene objects can resolve both app-wide and scene-level bindings.
- **MonoBehaviourInjectable** - base class for `MonoBehaviour`s that need injection. Dependencies are resolved automatically in `Awake`.

---

## Quick Setup

1. Create an empty `GameObject` in your scene, add **AppContext** and assign your `MonoInstaller`(s) to it.
2. Create another `GameObject`, add **SceneContext** and assign scene-level `MonoInstaller`(s).
3. Inherit your injectable `MonoBehaviour`s from `MonoBehaviourInjectable`.

---

## Creating an Installer

Derive from `MonoInstaller` and register all bindings inside `Install`:

```csharp
public class GameInstaller : MonoInstaller
{
    public override void Install(DiContainer container)
    {
        container.Bind<IScoreService>().To<ScoreService>().AsSingle();
        container.Bind<IAudioService>().To<AudioService>().AsSingle();
        container.Bind<IEnemy>().To<Enemy>().WithArguments(100).AsSingle();
    }
}
```

---

## Lifetimes

| Method | Behaviour |
|--------|-----------|
| `AsSingle()` | One shared instance for the lifetime of the container |
| `AsCached()` | One instance created on first resolve, reused afterwards |
| `AsTransient()` | A new instance is created on every resolve |

```csharp
container.Bind<IService>().To<Service>().AsSingle();    // singleton
container.Bind<IService>().To<Service>().AsCached();    // lazy singleton
container.Bind<IService>().To<Service>().AsTransient(); // new each time
```

---

## Binding Sources

### FromNew - container constructs the instance (default)

```csharp
container.Bind<IEnemy>().To<Enemy>().AsSingle();
// explicit:
container.Bind<IEnemy>().To<Enemy>().FromNew().AsSingle();
```

### FromInstance - use an already-created object

```csharp
var config = new GameConfig { MaxEnemies = 10 };
container.Bind<GameConfig>().FromInstance(config).AsSingle();
```

### FromFactory - delegate construction to a factory function

```csharp
container.Bind<IEnemy>().FromFactory(args => new Enemy(42)).AsSingle();
```

### WithArguments - pass explicit constructor arguments

Constructor parameters that match the provided argument types are filled from the list; remaining parameters are resolved from the container.

```csharp
container.Bind<IEnemy>().To<Enemy>().WithArguments(115).AsSingle();
// Enemy(int health) - health = 115, other deps resolved automatically
```

---

## Injection Types

### Field injection

```csharp
public class PlayerController : MonoBehaviourInjectable
{
    [Inject] private IScoreService _scoreService;
    [Inject] private IAudioService _audioService;

    private void Start()
    {
        _scoreService.Reset();
    }
}
```

### Property injection

```csharp
public class HudController : MonoBehaviourInjectable
{
    [Inject] public IScoreService ScoreService { get; private set; }
}
```

### Method injection

```csharp
public class EnemySpawner : MonoBehaviourInjectable
{
    private IEnemyFactory _factory;
    private IScoreService _score;

    [Inject]
    public void Init(IEnemyFactory factory, IScoreService score)
    {
        _factory = factory;
        _score = score;
    }
}
```

### Constructor injection (plain C# classes)

The container automatically picks the constructor marked `[Inject]`, or the one with the most parameters. All parameters are resolved from the container.

```csharp
public class ScoreService : IScoreService
{
    private readonly IAudioService _audio;

    public ScoreService(IAudioService audio)
    {
        _audio = audio;
    }
}
```

You can also mark a specific constructor explicitly:

```csharp
public class Enemy : IEnemy
{
    [Inject]
    public Enemy(int health, IAudioService audio) { ... }
}
```

---

## MonoBehaviourInjectable

### What it is

`MonoBehaviourInjectable` is an abstract base class for `MonoBehaviour`s that need dependencies injected. You place it in the scene like any other `MonoBehaviour` - **it does not need to be registered or bound in any container**. The container injects into an already-existing Unity instance rather than constructing a new one.

```csharp
public class HudController : MonoBehaviourInjectable
{
    [Inject] private IScoreService _scoreService;

    private void Start()
    {
        _scoreService.Reset();
    }
}
```

### How injection is triggered

`Awake` calls the internal `Inject()` method automatically. The execution order guarantees that by the time any `MonoBehaviourInjectable.Awake` runs, both contexts are already initialized:

```
AppContext.Awake    (ExecutionOrder -5100) - creates root DiContainer
SceneContext.Awake (ExecutionOrder -5000) - creates scene DiContainer, registers in ContextRegistry
MonoBehaviourInjectable.Awake  (default 0) - looks up context, injects
```

### How it finds the right container

`ContextRegistry` is a static dictionary keyed by `UnityEngine.SceneManagement.Scene`. When `SceneContext` initializes it registers itself:

```
ContextRegistry[scene] = this   // done by SceneContext.Awake
```

When `MonoBehaviourInjectable.Inject()` runs, it looks up its own scene:

```
context = ContextRegistry.GetContext(gameObject.scene)
context.Container.Inject(this)
```

This means **each scene resolves dependencies from its own `SceneContext`**. A `MonoBehaviourInjectable` in Scene A will never accidentally use Scene B's container.

### Dependency resolution order

The scene container is a child of `AppContext.Container`. When resolving a type the lookup goes:

```
1. SceneContext.Container  (scene-level bindings)
2. AppContext.Container    (app-level bindings, fallback)
```

So a `MonoBehaviourInjectable` can receive both scene-specific and app-wide dependencies transparently.

### What happens if SceneContext is missing

If the scene has no `SceneContext`, `ContextRegistry.GetContext` returns `null` and Unity logs:

```
No Context found in scene for <ClassName>
```

No exception is thrown, but none of the `[Inject]` members will be populated.

### Overriding Awake

If you need your own `Awake` logic, call `base.Awake()` first so injection completes before you use any injected field:

```csharp
protected override void Awake()
{
    base.Awake(); // injection happens here
    _scoreService.Reset(); // safe to use injected deps now
}
```

### Triggering injection manually

If you instantiate a `MonoBehaviourInjectable` at runtime (e.g. via `Instantiate`), `Awake` fires automatically and injection happens the same way. If for any reason you need to re-inject an existing instance, call the protected `Inject()` method directly:

```csharp
Inject(); // re-runs field/property/method injection against the scene container
```

---

## Manual Resolve & Inject

You can interact with the container directly when needed.

```csharp
// resolve a registered type
IScoreService score = container.Resolve<IScoreService>();

// inject dependencies into an already-existing object
container.Inject(existingObject);
```

---

## Child Containers

`DiContainer` supports a parent–child hierarchy. Resolving a type first checks the child container; if not found it falls back to the parent.

```csharp
var childContainer = new DiContainer(parentContainer);
childContainer.Bind<ILocalService>().To<LocalService>().AsSingle();

// resolves ILocalService from child, IScoreService from parent
var local = childContainer.Resolve<ILocalService>();
var score = childContainer.Resolve<IScoreService>();
```

This is how `SceneContext` works internally - its container is a child of `AppContext.Container`.
