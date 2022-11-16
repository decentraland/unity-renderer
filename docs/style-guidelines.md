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
[Microsoft’s guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/) are used as a baseline. Here you will find short summary of its adaptation to the current project, highlights and specifics related to Unity, which we suggest to follow.

## Naming conventions
Use
* `PascalCase` - Namespace, Class, Struct, Interface, Enumeration and its Enumerators, Method, Delegate declaration, Event, public Property, and public Field;
* `camelCase` - non-public Property, non-public Field, methods Parameter, local Variable;
* `ALL_UPPER_SNAKE_CASE` - Constants, static read-only fields;
* `I` prefix in front of Interface name;
* Asynchronous methods end with `Async`
* Events name is in past tens and without `On` prefix
* `_`, `__`, `___` for unused parameters of the method (for example, for events subscribers, interface implementation and inheritance overriden methods)

```csharp
namespace MyProject                                     // Namespace -> PascalCase
{
    public enum Side { Left, Right }                    // Enumeration and Enumerators -> PascalCase 
    public delegate void Interaction<T> (T current);    // Delegate declaration -> PascalCase
    public interface IInitializable { }                 // Interface -> PascalCase, starts with 'I'
    
    public class MyClass : IInitializable               // Class/Struct -> PascalCase.
    {
        public const string ASSET_PATH = "AvatarUI";    // Constant -> ALL_UPPER_SNAKE_CASE
 
        public int PublicField;                         // public Field -> PascalCase
        private bool isVisible;                         // non-public Field -> camelCase

        public bool IsInitialized { get; set; }         // public Property -> PascalCase
        private bool isVisitble { get; }                // non-public Property -> camelCase

        public event Interaction<bool> Interacted;      // event -> PascalCase, without On prefix
        
        public void Play(float speed, int _)            // Method -> PascalCase. Method parameters -> camelCase. Not-used parameter -> underscore
        {
            var velocity = speed;                       // local variable -> camelCase
            Interacted += OnInteracted;                 // for event subscribers 'On' prefix can be used
        }
        
        public async void PlayAsync(float speed) {..}   // Asynchronous methods end with `Async`             
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
  * Fields (`const` and `static readonly` goes first)
  * Properties
  * Events (and `UnityEvent`'s)
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
  * `Const`, `static` and `readonly` goes first (see example below)
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
  * `static` and `readonly` goes first (similar as for fields, see example above 👆)
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
### Methods specifics
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
  * Not-helper methods should follow the order (but its helper methods follows previous rule and allowed to be placed in between of this order):
    * entry-point/creation/setup methods, like `constuctor` and `initialize`
    * exit-point methods, like `destructor` and `dispose`
    * `public`
    * `public` with `[PublicApi]` and `[UsedImplicitly]` attributes
    * Unity-callbacks 
      * `Awake`, `Start`, `OnEnable`, `OnDisable`, `OnDestroy`,
      * other callbacks (with respect to `Enter`-`Stay`-`Exit` order)
    * `internal`
    * `protected`
  * consider using local function for your helper method if it is small 
    * do not use local functions inside local functions
  * for more detailed example on the methods ordering rules see methods organization in `CodeStyleExample.cs` file


## Formatting and other code conventions
Most of this formatting conventions will be known by your IDE thanks to the .EditorConfig and applied via auto-formatting on the fly. So there is no need of remembering it.  

### General
* keep each `public` type (like `class`/`enum`/`struct`/`interface`) in a separate file with the name equal to the type name
* access modifiers are obligatory to use: 🟢 `private void Awake() { }` 🔴 `void Awake() { }`
* use `var` only when it is evident. In all other cases specify the variable type explicitly.
* don't omit comparison to `null`: 🟢 `if (character == null)`/`if (character != null)` 🔴 `if (character)`/`if (!character)`
* use `nameof` where it is possible 
* use string interpolation instead of concatenation 🟢 `$"url = {url} / userid = {userId}"` 🔴 `"url" + url + " / userid=" + userId`;
* Use `Action`/`Func` delegate in most cases
  * when the amount of `event`/`delegate` parameters is higher than 3 then define a custom `EventArg` (either `System.EventArgs` or a custom `struct`) or define a custom `delegate`.
* one line - one statement. Split chain methods (like LINQ) in several lines starting  with `.` on each line
```csharp
List<string> filteredWords = new FilterLogic(listWords).
              FilterOutShortWords().
              FilterOnlyWordsContainingLetterC().
              FilterArbitraryComplexItems().
              FilterSomeMoreArbitraryComplexItems().
              GetWords();
```

### Namespaces
* namespaces are obligatory
    * Each type (`class`, `enum`, `interface`) should be inside a namespace.
* namespace name should be meaningful
    * 🔴 avoid too abstract namespaces like `Scripts`, `Components`, `Contexts`, etc.
    * 🟢 it should reflect the Domain, area to which the script is belonging - `DCL.UI`, `DCL.NPC`, `DCL.Social.Chat`
* 🔴 avoid very deep levels of namespaces
* consider using plural namespace names where appropriate
* folder structure should be aligned with the namespaces
    * 🔴 not every folder should be namespace provider, especially folders like `Scripts`, `MainScripts`, `Assets`
    * 🔴 folders that are deep in the folders hierarchy should be without namespace.

### Whitespaces
Most of this rules are saved in the `.editorconfig` and will be applied automatically on formating.
  * **Horizontal spaces**
    * indentation = `4 spaces` (no `tab`)
    * only one space between code elements is allowed
    * space after a comma between function arguments - 🟢 `CollectItem(myObject, 0, 1);` 🔴 `CollectItem(myObject,0,1);`
    * no space before flow control conditions - 🟢 `while(x < y)` 🔴 `while (x < y)`
    * space before and after comparison operators - 🟢 `if(x == y)` 🔴 `if(x==y)`
  * **Vertical spaces**
    * one blank line is used for vertical separation and grouping
    * one blank line is always used to separate groups (field, properties, method definitions)
    * 🔴 two and more blank lines successively are not allowed
  * **Line breaks and curly braces** `{}`
    * Curly brace is always placed on a new line (Allman/BSD style):
      * line break before the opening brace `{`
      * line break between the closing brace `}` and `else`
    * avoid brackets when the body is a one line
    * put the body on the new line in most cases 
    * it is allowed to put the body in the same line when it contains only one simple interruption (like `return`/ `break`/ `continue`)
```csharp
if(!IsInRange()) return;    // interruptions are in the same line

if(IsInRange())             // body on the new line; no brackets
    Fire()
else
{
    CalculateDistance();
    MoveToEnemy();
}
```
* **Expression-bodies**
  * remove brackets in most cases where it is possible (for loops, `if`-`else`, methods, and properties)
    * ⚠️exception: always use brackets for **Unity callbacks** 
  * **Properties** - placed on the same line 
  * **Methods** - placed on the new line
```csharp
public bool IsInitialized => isInitialized;    // Property - body placed on the same line =>

private void Awake()                           // Unity-callback - use brackets even when expression-body is possible  
{
    var collider = GetComponent<Collider>;
}

public void Initialize() =>                    // Methods - body placed on the new line after =>
    SubrcribeToEvents();

private void TrimAll()
{
    foreach (string device in InputDevices)    // Braces removed
        device.Trim();
}
```

### Attribute usages
* **Class**
  * Use `[RequireComponent(typeof(MyComponent))]` attribute when you cache components via `GetComponents` on `Awake` and `Start` callbacks
  * Use `[DisallowMultipleComponent]` attribute when there should be only one `Component` of such type on the `GameObject`
* **Methods**
  * Use `[PublicAPI]` attribute if `public method` is exposed to be called from **outside** of the solution 
  * Use `[UsedImplicitly]` attribute for implicitly called methods, such as calls from Unity animation events, via Unity `GameObject.SendMessage()`, `GameObject.BroadcastMessage()` and similar. 
  * Use `[Button]` attribute from `EasyButtons` (requires reference in `.asmdef` file) instead of `[ContextMenu]` attribute. Consider using `Mode` parameter of this attribute for `Editor`/`PlayMode`-only methods.
* **Variables (in Inspector)**
  * Use `[SerializedField]` for exposing variable in the inspector instead of converting it to `public` variable 
  * Use CAPITAL letters for `[Header]` attribute 
  * Use `[Space]` attribute for better grouping of exposed variables 
  * Use `[Tooltip]` instead of comments for exposed in inspector variables

### Scripting Symbols
* try to avoid using Scripting Symbols. Often it can be solved via polymorphism 
* when using it, try to decorate as less code as possible
* No tabs when starting Scripting Symbols 
* No line breaks between Scripting Symbols and its body
 ```csharp 
#if UNITY_WEBGL
    // Here goes WEBGL code
#elif UNITY_STANDALONE
    // Here goes Standalone code
#endif
```
### Comments
* always use XML comments `/// <summary>`  before the `public` class declaration providing appropriate descriptions of their behavior
 ```csharp 
/// <summary>
/// This is the InitialScene entry point.
/// Most of the application subsystems should be initialized from this class Awake() event.
/// </summary>
public class Main: MonoBehaviour
{
    /// ...
}
```
* **only** use comments `//` inside of the class when it is necessary and there is a need to provide additional information which cannot be covered by good naming:
  * description of not obvious hidden logic behind the solution (such as for complex logic or mathematical algorithms) 
  * reference to the bug-ticket
  * "why" description of the hack
  * XML comments `/// <summary>` for not obvious public methods 
* each comment starts with an uppercase letter and end with a period
* insert one space between the comment delimiter (`//`) and the comment text
* don't use asterix syntax for comments 🔴 `/* comment */`
* remove commented out code

### Tests
* **Class name** should contain Tests postfix after the feature name to the class that tests the current feature
* **Test method name**
  * try to reflect core of the **arrange**/**act**/**assert** part of the test in the test name (especially **assert**)
  * consider using word `Should` to describe **assert** part and word `When` to describe **act** and **arrange** part of the test
* The method body should be split by `// Arrange` - `// Act` - `// Assert` comments to respective blocks. 
  * `Arrange` comment could be omitted if there is no arrangement or it coincide with the acting.
 ```csharp 
public class NavmapTests
{
    [Test]
    public void CorectSceneDataShouldBeDisplayedWhenPlayerCoordinatesChanged()
    {
        // Arrange
        var navmapView = Object.FindObjectOfType<NavmapView>();
        MinimapMetadata.GetMetadata();
        
        // Act
        CommonScriptableObjects.playerCoords.Set(new Vector2Int(-77, -77));
        
        // Assert
        Assert.AreEqual("SCENE_NAME", navmapView.currentSceneNameText.text);
        Assert.AreEqual("-77,-77", navmapView.currentSceneCoordsText.text);
    }
}
```