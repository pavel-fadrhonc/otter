Core version of [Extenject DI framework](https://github.com/svermeulen/Extenject)
Not necessarily up to newest version. I check once in a while and merge if I find it's worth (it's a bit of a hussle cause this package has different structure).

## My additions
<details>
<summary>Details</summary>

 * PrefabFactoryNameBased [#prefabfactorynamebased]
 * PrefabFactoryPoolable [#prefabfactorypoolable]
 * [CoroutineRunner] (#coroutinerunner)
 
</details>

## PrefabFactoryNameBased
Special kind of factory that allows for dynamic instantiation and pooling of prefabs with GameObject context and MonoBehaviour Facade class.
Read about Extenject [Factories](https://github.com/svermeulen/Extenject/blob/master/Documentation/Factories.md) and [Subcontainers](https://github.com/svermeulen/Extenject/blob/master/Documentation/SubContainers.md) first.

In original Extenject there is a an option for [PrefabFactory](#https://github.com/svermeulen/Extenject/blob/master/Documentation/Factories.md#prefab-factory) that allows for dynamic spawning of prefabs that are passed during runtime. However there is no option for making those prefabs poolable so the instances can be reused. PrefabFactoryNameBased attempts to fill this gap.

Basic use case is that you have a prefab that has to have a MonoBehaviour on it that implements `IPoolable<IMemoryPool>` and `IDisposable`.
```csharp
public class FooFacade : MonoBehaviour, IPoolable<IMemoryPool>, IDisposable
{
	private IMemoryPool _pool;

	public void OnDespawned()
	{
		_pool = null;
	}

	public void OnSpawned(IMemoryPool pool)
	{
		_pool = pool;
	}

	public void Dispose()
	{
		_pool?.Despawn(this);
	}
}
```

This prefab doesn't has to have GameObjectContext on it but if it does, it can elegantly act as a Facade class that wraps the prefab into its own Context where all binding all local (although the ones from parent containers are still inherited). It can also inform all interested parties (local classes) about spawning and despawning so they can set their state appropriatelly.

The affix **NameBased** refers to the fact that prefabs will be pooled based on the **name of the prefab**. That allows for different prefabs to have the same Facade class but still be treated as different objects.

There are convenience methods added into `DiContainer` to easily declare your `PrefabFactoryNameBased`.
```csharp
public void BindNameBasedPoolablePrefabFactory<TContract, TPlaceHolderFactory>()
            where TContract : Component, IPoolable<IMemoryPool>, IPoolable
            where TPlaceHolderFactory : PlaceholderFactory<UnityEngine.Object, PrefabFactorySpawnParams, TContract>
```
where `TContract` is you Facade class and TPlaceHolderFactory is the placeholder factory that you use to spawn prefab instances.

It expects at least prefab gameobject and `PrefabFactorySpawnParams` class. This class allows to setup prefab `position, rotation, scale` and `parent` **before** the Facade OnSpawned gets called. That is because lot of times, the initialization might rely on prefab being certain size, on certain position or at certain point in hierachy, so setting this after calling `factory.Create()` is too late. If, however, you dont need this, you can pass null without worries.

There are other versions with an option to pass arguments into your facade `OnSpawned` method. 
```csharp
public void BindPoolablePrefabFactory<TParam1, TContract, TPlaceHolderFactory>()
	where TContract : Component, IPoolable<TParam1, IMemoryPool> 
	where TPlaceHolderFactory : PlaceholderFactory<TParam1, PrefabFactorySpawnParams, TContract>
```
there are versions with up to 4 parameters.

Your Facede class then becomes, for example

```csharp
public class FooFacade : MonoBehaviour, IPoolable<float, int, string, IMemoryPool>, IDisposable
{
	private IMemoryPool _pool;
	
	private float _f;
	private int _i;
	private string _s;

	public void OnDespawned()
	{
		_pool = null;
	}

	public void OnSpawned(float f, int i, string s, IMemoryPool pool)
	{
		_f = f;
		_i = i;
		_s = s;
		_pool = pool;
	}

	public void Dispose()
	{
		_pool?.Despawn(this);
	}
}
```

when you no longer need to prefab instance, you just call `fooFacade.Dispose`.

So to wrap this up, in order to use this, you need
1. Prefab
2. Facade class (MonoBehaviour)
3. Factory class (recommended to be nested class of Facade class as per [Extenject best practices](#https://github.com/svermeulen/Extenject/blob/master/Documentation/Factories.md#example))
4. Instantiate the prefab by injecting the factory and calling `Create`, passing the prefab and optionally PrefabFactorySpawnParams and/or parameters
5. Profit
6. When profit no longer yields, call `FacadeClass.Dispose()` to free instance for further use.

 
## PrefabFactoryPoolable
 This class is the same as `PrefabFactoryNameBased` except there is no need to pass the prefab dynamically when calling create but rather `IPrefaFactoryPooledPrefabResolver` 
 class is injected in installer when binding the factory which will determine the prefab based on the TParam passed in Create. That and also I made version for just 1 parameter so far cause that's as much as I needed at the time.
 
 the Binding parameters are the same except you use `DiContainer.BindPoolablePrefabFactory<TParam1, TContract, TPlaceHolderFactory>()` method.
 Then you need to bind `IPrefaFactoryPooledPrefabResolver` yourself. The interface looks like this
 
```csharp
public interface IPrefaFactoryPooledPrefabResolver
{
	TContract ResolvePrefab(TParam param);
}
```

therefore the class needs to accept parameter of the prefab facade class and based on that decide which prefab to return and instantiate.
Internally, it still pools the prefab based on name of the prefab, so you can have more prefabs with same facade and parameter but different prefabs.

 
## CoroutineRunner
Allows for running coroutines from non-MonoBehaviour classes and some extended functionality over coroutines like Pausing them.
Usage: Inject CoroutineRunner and call RunCoroutine

```csharp
public class Foo : IInitializable
{
	[Inject]
	CoroutineRunner _runner;
	
	ofCoroutine _coroutine;
	int i = 0;
	
	public void Initialize()
	{
		_coroutine = _runner.RunCoroutine(DoSomething());
	}
	
	IEnumerator DoSomething()
	{
		while(true)
		{
			i++; // cause why not
		}
	}
}
```

then, at any point you can call 

```csharp
_runner.PauseCoroutine(_coroutine);
```

to pause the coroutine, or

```csharp
_runner.PauseCoroutineFor(_coroutine, 1.0f);
```

to pause it for given amount of seconds. Subsequent pausing cancels previous pausing. Unpausing cancels pause timer.
You can resume paused coroutine with

```csharp
_runner.ResumeCoroutine(_coroutine);
```

you can stop the coroutine by 

```csharp
_runner.StopCoroutine(_coroutine);
```

or the coroutine can stop naturally by

```csharp
yield break;
```

in both cases `ofCoroutine.CoroutineFinished` event is called.

CoroutineRunner uses Extenject [MemoryPools](#https://github.com/svermeulen/Extenject/blob/master/Documentation/MemoryPools.md) to provide `ofCoroutine` wrappers. After coroutine has been stopped the wrapper is returned back to pool to further reuse. Therefore it is possible to keep reference to already stopped coroutine and then manipulate it. That results in `NullReferenceException`. Even worse is the case when the `ofCoroutine` gets returned and then reused again and the same wrapper class is used to manipulate different Unity Coroutine. Therefore it is recommended best practice to always null your reference to the wrapper when coroutine stops.

```csharp
...
public void Initialize()
{
	_coroutine = _runner.RunCoroutine(DoSomething());
	_coroutine.CoroutineFinished += () => {_coroutine = null;};
}
...
```

Alternatively, you can use `ofCoroutine.IsValid` property but the problem of that is that it gets reseted when it is reused as new instance.

## Invoker
Similar to CoroutineRunner, Invoker is aimed to extend functionality provided by MonoBehaviour.Invoke()

Just inject IInvoker and use methods from this interface

Invoker operates on similar "interface" as MB.Invoke() and works with ids that identify running tasks.

These tasks can be invoked

`int Invoke(InvokerTask task_, float delay_, bool ignorePause)`

`int InvokeRepeating(InvokerTask task_, float delay_, float interval_, float cancelTime = 0, bool ignoreWorldPause = false)`

stopped

`void StopInvoke(int taskId, float delay)`

querried

`bool HasTask(int taskId)`

paused and resumed

`void Pause(int taskId)`

`void PauseFor(int taskId, float time)`

`bool IsPaused(int taskId)`

`void Resume(int taskId)`

aside from that WorlInvoker, which is default implementation of IInvoker also implements `IPausable` which is an easy way to pause the game without manipulating Unity.timescale (so unity running systems can still run if you want, like animations etc.). The `ingoreWorldPause` argument of `Invoke` methods is aimed at this pause and makes the task ingore it. All tasks, regardless of what `ignoreWorldPause` were they ran with, can be paused and resumed individually via dedicated methods.

## LateInitializer
Came from a fact that in Extenject, when there is a Context hierarchy, the nested Contexts are evaluated first before the upper ones do. This means, if there is some class that is initializing / fetching / preparing some data on a top level, the classes created in child Context cannot effectively use that class in Initialize because it is called **before** the parent Context Initialize. In this case, the execution order setting via `Container.BindExecutionOrder()` does not work, because it only works in space of single Context. For this purpose, I created **LateInitializer**.

Use it exactly as initialize, i.e. implement **ILateInitializable** interface and provide **LateInitialize** method.

However, in order to solve the problem stated above, i.e. to make sure that all `LateInitialize` are called after all `Initialize` the way this is implemented is that the call for `LateInitialize` is one frame delayed. This is usually fine but in case you are pausing the game via setting Unity timescale to 0 in Initialize/Start for some reason (like waiting for Addressables to load or some asynchronous operation to finish) this might cause issues.

Also, in a same way as `Initialize`, classes spawned via factories (except those that are on GameObject with GameObjectContext or MonoKernel derived script) do **not** have their `Initialize/LateInitialize` called automatically by the Manager but you need to call them yourself.


