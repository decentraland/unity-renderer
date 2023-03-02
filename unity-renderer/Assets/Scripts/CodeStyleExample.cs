// NOTE: following separation lines (//----- some group) are used only for clarity and do not take part of our commenting code style!

//----------------------------------------------------------------------------------------------------  Usings group
using EasyButtons;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DCL.UIComponents
{
    //------------------------------------------------------------------------------------------------  Enum, delegates, interfaces declaration group (public/internal) 
    public enum Side { Left, Right }

    public delegate void Destroction<T> (T current);

    public interface IInitializable
    {
        bool IsInitialized { get; }

        void Initialize();
    }

    [RequireComponent(typeof(Animation))]
    public class CodeStyleExample : MonoBehaviour, IInitializable
    {
        //----------------------------------------------------------------------------------------  Enum, delegates declaration group (protected/private) 
        private enum Direction { North, South, West, East }
        private delegate void Interaction<T> (T current);
        
        //----------------------------------------------------------------------------------------  Const and static readonly Fields group
        public const string ASSET_PATH = "AvatarPrefab";
        private const float MAX_TIMER = 10f;

        public static readonly int WAVE_ANIM_HASH = Animator.StringToHash("Wave");
        private static readonly int DANCE_ANIM_HASH = Animator.StringToHash("Dance");

        //----------------------------------------------------------------------------------------  Events and UnityEvents group
        public event Destroction<CodeStyleExample> Destroyed;
        [NonSerialized] public UnityEvent<bool> Closed; // use [NonSerialized] or [HideInInspector] for UnityEvents

        //----------------------------------------------------------------------------------------  Fields group (other than Const and Static readonly)  
        public readonly List<int> CachedNumbers = new List<int>();
        protected readonly List<Vector2> poistions = new List<Vector2>();
        
        public int Timer;

        [SerializeField] private AnimationClip[] clips;
        [SerializeField] private float armor;
        [SerializeField] private float health;
        [SerializeField] private bool isVisitble;

        protected float cooldown;
        
        private new Animation animation;
        private Interaction<int> interactionsBuffer;

        //----------------------------------------------------------------------------------------  Properties group
        public bool IsInitialized { get; private set; }

        [PublicAPI]
        public bool IsVisitble => isVisitble; // [PublicAPI] Public - used only from outside of Unity solution (by other solution)

        //----------------------------------------------------------------------------------------  Public methods group
        public void Initialize() =>
            IsInitialized = true;

        public void ApplyDamage(float damage) // Public - called from outside of the class (by other class)
        {
            float resultDamage = ComputeDamage(damage);
            TakeHit(resultDamage);
        }

        private float ComputeDamage(float damage) => // Called by ApplyDamage
            damage - armor;

        private void TakeHit(float amount) => // Called by ApplyDamage
            health -= amount;

        [UsedImplicitly]
        public void Hide() => // [UsedImplicitly] Public - called implicitly, for example, by Unity animation events or SendMessage
            SetVisibility(visible: true);

        private void SetVisibility(bool visible) => // Called by Hide
            isVisitble = visible;

        //----------------------------------------------------------------------------------------  Unity callbacks methods group
        private void Awake() // is used for getting references
        {
            animation = GetComponent<Animation>();
        }

        private void Start() // is used for initialization logic
        {
            foreach (AnimationClip clip in clips)
                animation.AddClip(clip, clip.name);
        }

        private void OnEnable() // is used for events subscription
        {
            Closed.AddListener(OnClosed);
        }

        private void OnClosed(bool _) => // use _, __, ___ for parameters name if required parameters are not used inside 
            cooldown = MAX_TIMER;

        private void OnDisable() // is used for events unsubscription
        {
            Closed.RemoveListener(OnClosed);
        }

        private void OnDestroy() // is used for any needed clean-up
        {
            Destroyed?.Invoke(this);
        }
        
        //----------------------------------------------------------------------------------------  internal-protected-private methods group
        // methods that weren't called inside methods above 👆   
        internal void Method1() { }
        protected void Method2() { }
        private void Method3() { }
        
        //----------------------------------------------------------------------------------------  Nested classes/struct group
        private struct Data
        {
            public int A;
            public bool B;
        }
    }
}
