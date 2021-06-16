My Additions to Extenject framework.
These are considered **Extras** since they do not neccesarilly extend the base functionality, same as original Extenject extras like [Signals](https://github.com/svermeulen/Extenject/blob/master/Documentation/Signals.md), [Async](https://github.com/svermeulen/Extenject/blob/master/Documentation/Async.md), etc...


## Commands
Inspired by [Strange IOC commands](http://strangeioc.github.io/strangeioc/TheBigStrangeHowTo.html#h.wvehwwtgkcqn) this extension aims to organize codebase by providing standardized pattern for where to implement the Controller layer in classic MVC pattern. It adds another option to what can respond to Extenject [Signals](https://github.com/svermeulen/Extenject/blob/master/Documentation/Signals.md). So with it it is possible to bind command class to signal. The command class will get instantiated (from pool) upon signal call and it's `Execute` method gets called injecting Signal class as a member.
For example:
```csharp
    public struct FooSignal
    {
        public int fooParam;
    }

    public class FooCommand : Command<FooSignal>
    {
        protected override void Execute()
        {
            Debug.Log($"signal param is {signal.fooParam}");
        }
    }

	...
	// separate file
    public class CommandTestInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.BindSignal<FooSignal>().ToCommand<FooCommand>();
        }
    }
	
	...
	/// separate file
    public class RaiseFooSignal : MonoBehaviour
    {
        private SignalBus _signalBus;
	
        [ContextMenu(nameof(Raise))]
        public void Raise()
        {
            _signalBus.Fire(new FooSignal() {fooParam = 9001});
        }
    }
```

If you put last two MB in separate files and then add the `CommandTestInstaller` into `SceneContext` and the `RaiseFooSignal` on GameObject that is child of `SceneContext` GameObject and then run the scene and run the Raise function via script context menu, you should get the command debug log and it should be understandable how commands work.

By default, command is disposed and returned to the pool upon exiting the `Execute` function, however, you can `RunDelayed` any number of additional functions. 

```csharp
protected void RunDelayed(float delay, Action action)
```

Command will get automatically disposed upon finishing last of `RunDelayed` functions. You can `RunDelayed` another functions that was `RunDelayed`, however don't make this into persistent class with Update-like behaviour since Commands are meant to be react-and-throw away kind of functionality. If you need to invoke actions with update time different than Update Time.deltaTime use IInvoker and if you need to schedule behaviours with lots of delays and irregularities use CoroutineRunner. Both are part of Extenject-Core.

The way I like to use Commands is as glue-classes that inject Models and upon firing of signal they manipulate data over Models and maybe raise some signals themselves to let View know what happened.
Note that even though Commands are pooled and allocate no garbage, it is more efficient to just bind class as Single and use .net events, direct method calls or polling if you want to react on change of data happening every frame.


## ViewMediator 
Inpired by [StrangeIOC View Mediator extension](http://strangeioc.github.io/strangeioc/TheBigStrangeHowTo.html#h.sjblqrdytark) I implemented similar counterpart for Extenject to provide yet another implementation of layer in MVC pattern. View-Mediator implements View layer obviously but with a certain split. 
The purpose of View class is to be a MonoBehaviour that takes care of Unity-specific functionalities like having references to visual elements like UGUI texts, buttons, Sprites, Renderers and simultaneously providing domain-agnostic (meaning they make no assumptions about the functionality of your app domain, e.g. the business logic) interface for Mediator to manipulate.
Mediator, on the other hands acts like a intermediatory element between rest of the application and the View. It usually reacts to signals or events happening on models. More strict implementation like the one in [StrangeIOC](http://strangeioc.github.io/strangeioc/exec.html) shy away from connecting Mediator with Models directly opting for re-raising Signals from Commands that Mediators react to but I find that approach unnecessarily restrictive. It creates a need for making even more Signal classes which have a sole purpose of resending the same data to mediator. Instead, I think it's very much ok for Mediator to inject Model class and subsribe to it's OnChange .net events. As always, situation might dictate otherwise and if there is a for example some reuse found for those resending Signals then that approach might make sense. However I would refrain from statements like "don't ever do this, you'll thank me later" because I trust user judgement and also because I believe that getting burned a little is a best way to learn :).

So let's look at how we go around implementing ViewMediator pattern using this Extenject extension on a simple example.
We need couple things.
1. View
```csharp
    public class TestView : View<TestViewMediatorParams>
    {
        public TextMeshProUGUI textMesh;

        protected override void Initialize()
        {
            base.Initialize();

            textMesh.fontSize = 25;
        }
		
        public void SetText(string text)
        {
            textMesh.text = text;
        }

        public void SetColor(Color col)
        {
            textMesh.color = col;
        }
        
        public class Factory : PlaceholderFactory<TestViewMediatorParams, PrefabFactorySpawnParams, TestView> {}
    }
```
* Here we're creating a View with parameters class that will get passed to Mediator. These are domain (businness logic) specific and View should not know about these. The reason for that is that one View can have several different mediators that operate differently on the same set of params. Another reason is that by making this strict separation it is more clear where to put certain functionalites (setting up visual elements values and reference in View, manipulating those via View public interface into Mediator etc.) which makes codebase eaiser to debug and test.

* We're also including the Factory class since View is part of separate prefab with GameObjectContext and is using prefab factory very similar to Extenject-Core PrefabFactoryNameBased. I chose this approach because this factory can handle creating and pooling of dynamically spawned prefabs with GameObjectContext. We'll get to this in next steps.
		
* Any initialization code should go into `Initialize()` override. That way, it is ensured that View transform is setup properly via `PrefabFactorySpawnParams` that can be optionally passed into factory `Create()` method. The initialization method is also ran before `Mediator.OnEnable()` is called so that mediator can relly that by then it can safely call any View methods or properties.
		
	1a. ViewParams (optional)		
	```csharp
		public class TestViewMediatorParams
		{
			public string text;
			public Color color;
		}
	```
2. Mediator
```csharp
    public class TestMediator : MediatorBase<TestView, TestViewMediatorParams>
    {
        public override void OnEnable()
        {
            base.OnEnable();
            
            _view.SetColor(param.color);
            _view.SetText(param.text);
        }
    }
```
* Mediator has paramater as a private *param* member. In case of more paramaters they are named *param1*, *params2*, etc.
Mediator's initialization logic goes into `OnEnable()` method. It has `_view` member available and also `_signalBus` since it communicates with it often enough that it is worthy to include it into base class. It also has `OnDisable()` deinitialization method in case you need to `UnSubscribe()` from signals or do some other work on disposing.
	
* Note that unlike `View.Initialize()` that runs one time upon creation (like normal Initialize()) `Mediator.OnEnable()` and `OnDisable()` runs on every instance spawn (like `IPoolable.OnSpawned` and `IPoolable.OnDespawned`).
	
3. View Installer
```csharp
public class TestViewInstaller : ViewInstaller1Param1Mediator<TestViewMediatorParams, TestView, TestMediator> { }
```
* Create the installer by simply inheriting from proper ViewInstaller class and providing the template types.
There are more types of the installer depending on how many parameters do the mediators need and how many mediators are there.

4. Prefab
	* Create a prefab
	* Put GameObjectContext on it
	* Put installer on the GameObjectContext and then reference it in MonoInstallers part of GameObjectContext.
	* Put View on the prefab
	
5. Bind the View Factory
```csharp
public TestView testViewPrefab;
...
public override void InstallBindings()
{
	Container.BindViewFactory<TestViewMediatorParams, TestView, TestView.Factory>(testViewPrefab);
}
```
* Here we are binding the placeholder factory from the view to the concrete factory that will take care of business of creating and pooling the view instances, providing the prefab in the installer. Note that there is no dependency on the mediators so it is possible to have more prefabs with same View but different mediators that will manage the View interface differently. This is one of the advantages of View-Mediator separation in practice.
	
* Going even further with this concept, it is possible to use `PrefabFactoryPoolable` via `BindPoolablePrefabFactory` Extenject-Core extension instead of ViewFactory to bind the Views. You will lose the type checking for TView in `BindViewFactory` but you will gain the ability to not having to bind the prefab to View upfront in Installer but rather providing `IPrefaFactoryPooledPrefabResolver` class that will provide the prefab conforming to **TContract** based on TParam value. You can then for example pass an enum value in TParam of View that will serve as determinator of what specific View to select and to provide via factory `Create()` call. Check the sources for more details. In theory, even PrefabFactoryNameBased should be possible to use (which means that caller of `Factory.Create()` passes the prefab) because all three of these factories are made using the same mechanism but I personally haven't tried this one so I can't vouch for it :).
	
6. Inject the factory and **Create** and **Dispose** the instances
```csharp
[Inject] TestView.Factory _testViewFactory
...
var viewInstance = _testViewFactory.Create(new TestViewMediatorParams()
	{
		color = _settings.colors.Random(),
		text = _settings.texts.Random()
	}, new PrefabFactorySpawnParams()
	{
		parent = _canvas.transform,
		position = spawnPoint
	});
...
viewInstance.Dispose();
```

* The PrefabFactorySpawnParams is optional and it makes sure that the View transform is properly setup before View `Initialize()` method and Mediator `OnEnable()` is called.
	
## RaiseSignal
This handy utility allows you to raise signals directly from UnityEvent without need to do any intermediatory code.
For this to work, you signal must inherit from **ISignal** interface.
Then put RaiseSignal script on MonoBehaviour. It allows you to select from all signals implementing ISignal.
Bonus if you're using [Odin Serializer and Inspector](https://odininspector.com/) you can event fill in all signal members that are Unity serializable.
Then reference this from your [UnityEvent](https://docs.unity3d.com/ScriptReference/Events.UnityEvent.html) (like UI.Button and such).
Your signal will then be fired via `SignaBus.Fire()` call.
You can also check the **Fire Abstract** checkbox to use `SignaBus.AbstractFire()`. [Abstract signals](https://github.com/modesttree/Zenject/blob/master/Documentation/Signals.md#abstract-signals) are great way to add modularity to your codebase and signals.
!!! WARNING: currently this script does not support scene prefab overrides or prefab variants properly










