# Coding Guidelines

Maintaining a common code style will boost our productivity by reducing the amount of noise in Code Reviews and allowing new members to on-board faster.
When working with the Unity Project these guidelines (to be documented) must be followed and to ease the process we have a pre-commit hook to deal with it automatically.
The repo `https://github.com/decentraland/reshaper-pre-commit-hook` contains both the installing bash script and the pre-commit hook itself.

To install it go to the root of your explorer repository and run the following command on it:
`curl -s https://raw.githubusercontent.com/decentraland/reshaper-pre-commit-hook/master/install-git-hook.sh | bash`

### Notes 

* For all our style needs we are using an [`.editorconfig`](https://editorconfig.org/) file. 

* It's recommended to use a `Format On Save` extension on your IDE of choice so we avoid styling feedback noise on pull requests.

* We don't use the `.resharper` extensions for `.editorconfig`. So if you use `resharper` plugin or `rider` you must set the `resharper` specific style settings to match those of `VS Code` and `VS Community`.


### Rider
You can find a settings export file in the root of the project called "rider_codeStyleDCLSetting". Bear in mind that the conversion between VS and Rider is not 1 on 1 but it's good enough.

# Code conventions
### Naming conventions
Use
* `PascalCase` - Namespace, Class, Struct, Interface, Enumeration and its Enumerators, Method, Delegate declaration, Event, Property and public Field;
* `camelCase` - Field, local Variable, methods Parameter;
* `CAPITALS_SNAKE_CASE` - Constants;
* `I` prefix in front of Interface name;
```csharp
namespace MyProject                                     // Namespace -> PascalCase
{
    public enum Side { Left, Right }                    // Enumeration and Enumerators -> PascalCase 
    public delegate void Change<T> (T current);         // Delegate declaration -> PascalCase
    public interface IInitializable { }                 // Interface -> PascalCase, starts with 'I'
    
    public class MyClass : IInitializable               // Class/Struct -> PascalCase.
    {
        public const string ASSET_PATH = "AvatarUI";    // Constant -> CAPITALS_SNAKE_CASE 
        public int PublicField;                         // public Field -> PascalCase
        private bool isVisible;                         // Field -> camelCase
        public bool IsInitialized {get; set;}           // Property -> PascalCase
        
        public void Play(float speed)                   // Method -> PascalCase. Method parameters -> camelCase
        {
            var animationClip;                          // local variable -> camelCase
        };              
    }
}
```
Suggestions:
* `Interface` - try to name it with adjective phrases (`IDamageable`). If it is difficult, then use descriptive noun (`IBaseVariable`) or noun phrase (`IAssetProvider`).
* `Class`/`Struct` -  name with nouns (`Health`) or noun phrases (`InputService`).

### Ordering conventions
* `using` go first and placed alphabetically
* Class members grouped and appears in the following order
  * Enums, delegates declarations
  * Fields
  * Properties
  * Events
  * Methods
  * Nested classes
* Order inside group:
  * `public`
  * `internal`
  * `protected internal`
  * `protected`
  * `private`
* Fields specifics
  * Const, static and readonly goes first
  * Public `[HideInInspector]` and `[NonSerialized]` attribute goes after `public`
  * `[SerializedFields]` attribute fields goes after all `public`
* Properties specifics
  * static and readonly goes first
  * Property with public `set` and private `get` goes before get-only Property or Property with private `set` and public `get` (considered to be more exposed)
```csharp
public const string ASSET_PATH = "AvatarPrefab";                            // Constants group 
internal const float MIN_TIME = 0.1f;
private const float MAX_TIMER = 10f;

public static readonly int WaveAnimHash = Animator.StringToHash("Wave");    // Static Read-only group
private static readonly int danceAnimHash = Animator.StringToHash("Dance");

internal static bool isOpen;                                                // Static group
protected static bool isAvtive;                                             

public readonly List<int> CachedNumbers = new List<int>();                  // Read-only group
private readonly List<Section> Sections = new List<Section>();

public int PublicFieldA;                                                    // Public Fields group

[HideInInspector] public int SomeField02;
[NonSerialized] public int SomeField03;

[SerializedField] internal Animator animator;                               // [SerializedField]'s group
[SerializedField] private AnimatorClip[] clips[];  

protected float cooldown;                                                   // internal-protected-private Fields group  
private bool isVisible;                         
                       
public bool Property1 { get; set; }                                        // Properties groups  
public bool Property2 { private get; set; }
public bool Property3 { get; }
public bool Property4 { get; private set; }

protected bool Property5 { get; set; }
private bool Property6 { get; set; }

public event Action Started;                                               // Events group  
public event Func<float> Completed;
```
