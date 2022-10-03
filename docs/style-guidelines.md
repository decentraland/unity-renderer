# Coding Guidelines

Maintaining a common code style will boost our productivity by reducing the amount of noise in Code Reviews, improving code navigation and allowing new members to on-board faster.

When working with the Unity Project these guidelines must be followed.

### Notes

* For all our style needs we are using an [`.editorconfig`](https://editorconfig.org/) file.
* It's recommended to use a `Format On Save` extension on your IDE of choice so we avoid styling feedback noise on pull requests.
* We don't use the `.resharper` extensions for `.editorconfig`. So if you use `resharper` plugin or `rider` you must set the `resharper` specific style settings to match those of `VS Code` and `VS Community`.

### Rider
You can find a settings export file in the root of the project called "rider_codeStyleDCLSetting". Bear in mind that the conversion between VS and Rider is not 1 on 1 but it's good enough.

# Code conventions
[Microsoft’s guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/) are used as a baseline. Here you will find short summary of our adaptation of it, highlights and specifics related to Unity, which we suggest to follow.
## Naming conventions
Use
* `PascalCase` - Namespace, Class, Struct, Interface, Enumeration and its Enumerators, Method, Delegate declaration, Event, public Property, and public Field;
* `camelCase` - non-public Property, non-public Field, methods Parameter, local Variable;
* `CAPITALS_SNAKE_CASE` - Constants;
* `I` prefix in front of Interface name;
* Events name is in past tens and without `On` prefix

```csharp
namespace MyProject                                     // Namespace -> PascalCase
{
    public enum Side { Left, Right }                    // Enumeration and Enumerators -> PascalCase 
    public delegate void Interaction<T> (T current);    // Delegate declaration -> PascalCase
    public interface IInitializable { }                 // Interface -> PascalCase, starts with 'I'
    
    public class MyClass : IInitializable               // Class/Struct -> PascalCase.
    {
        public const string ASSET_PATH = "AvatarUI";    // Constant -> CAPITALS_SNAKE_CASE
 
        public int PublicField;                         // public Field -> PascalCase
        private bool isVisible;                         // Field -> camelCase

        public bool IsInitialized { get; set; }           // public Property -> PascalCase
        private bool isVisitble { get; }                  // non-public Property -> camelCase

        public event Interaction<bool> Interacted;      // event -> PascalCase, without On prefix
        
        public void Play(float speed)                   // Method -> PascalCase. Method parameters -> camelCase
        {
            AnimationClip animationClip;                // local variable -> camelCase
            Interacted += OnInteracted;                 // for event subscribers 'On' prefix can be used
        }              
    }
}
```

### Suggestions:
* `Interface` - try to name it with adjective phrases (`IDamageable`). If it is difficult, then use descriptive noun (`IBaseVariable`) or noun phrase (`IAssetProvider`).
* `Class`/`Struct` -  name with nouns (`Health`) or noun phrases (`InputService`).
* `Delegate type` - try to use nouns or noun phrases (take example from .NET built-in delegate types - `Action`/`Function`/`Predicate`)  

## Ordering conventions
* `using` go first and placed alphabetically
* Class members grouped and appears in the following order
### Groups order:
  * Enums, delegates declarations
  * `const` and `static readonly` Fields
  * Events (and `UnityEvent`'s)
  * Fields (other `const` and `static readonly`)
  * Properties
  * Methods
  * Nested classes
### Order inside group:
  * `public` 
  * `public` with `[PublicApi]` and `[UsedImplicitly]` attributes
  * `internal`
  * `protected internal`
  * `protected`
  * `private`
  * methods has additional rule for ordering
### Fields specifics
  * `Const`, `static` and `readonly` goes first (see example below 👇)
  * `public` fields with `[HideInInspector]` and `[NonSerialized]` attribute goes after `public`'s
  * `[SerializedFields]` attribute fields goes after all `public` fields and before `internal`-`private`-`protected`)
```csharp
public const string ASSET_PATH = "AvatarPrefab";                              // Constants  
internal const float MIN_TIME = 0.1f;

public static readonly int WAVE_ANIM_HASH = Animator.StringToHash("Wave");    // Static Read-only 
private static readonly int DANCE_ANIM_HASH = Animator.StringToHash("Dance");

public static bool IsOpen;                                                    // Static 
internal static bool isAvtive;                                             

public readonly List<int> CachedNumbers = new List<int>();                    // Read-only 
protected readonly List<Section> sections = new List<Section>();

public int PublicFieldA;                                                      // Public  
[HideInInspector] public int SomeField02;
[NonSerialized] public int SomeField03;

[SerializedField] internal Animator animator;                                 // [SerializedField]'s 
[SerializedField] private AnimationClip[] clips;  

protected float cooldown;                                                     // internal-protected-private's
private bool isVisible;                         
```
### Properties specifics
  * static and readonly goes first (similar as for fields, see example above 👆)
  * `get`-only and `set`-only properties goes last.
  * Access modifiers for `set` considered to have higher priority than `get` (considered to be more exposed). 
    * For example, property with `public set` and `private get` goes before `get`-only Property or Property with `private set` and `public get`
    
```csharp
public bool Property1 { get; set; }                                        // get-set order
public bool Property2 { private get; set; }
public bool Property3 { get; private set; }
public bool Property4 { get; }

protected bool Property5 { get; set; }
private bool Property6 { get; set; }
```
### **Methods specifics**
  * Helper and supplementary methods which called from another method should be placed after method that calls it (in most cases it is `private` functions).
```csharp
// Note: indentaion for helper methods is used only for clarity. It is not a part of our formating style 
public void Test1()                   // called by other class
{
    Test1Helper1();
    Test1Helper2();
}

  private void Test1Helper1() => A();  // called by Test1() 
    internal void A() { }              // called by Test1Helper1()
  private void Test1Helper2() => B();  // called by Test1()
    public void B() { }                // called by Test1Helper2()

private void Awake() { }               // called by Unity
```
  * Not-helper methods should follow the order (where its helper methods follows previous rule and allowed to be placed in between of this order):
    * constuctor
    * destructor 
    * `public`
    * `public` with `[PublicApi]` and `[UsedImplicitly]` attributes
    * Unity-callbacks 
      * `Awake`, `Start`, `OnEnable`, `OnDisable`, `OnDestroy`,
      * other callbacks (with respect to `Enter`-`Stay`-`Exit` order)
    * `internal`
    * `protected`
  * For more detailed example on the methods ordering rules see methods organization in `CodeStyleExample.cs` file