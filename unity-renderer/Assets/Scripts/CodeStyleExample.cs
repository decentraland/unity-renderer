// NOTE: following separation lines are used only for clarity and do not take part of our commenting code style!
//----------------------------------------------------------------------------------------------------  Usings group

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace UIComponents.Scripts.Components.SectionSelector
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

        //----------------------------------------------------------------------------------------  Const and static readonly Fields group
        public const string ASSET_PATH = "AvatarPrefab";
        private const float MAX_TIMER = 10f;

        public static readonly int WAVE_ANIM_HASH = Animator.StringToHash("Wave");
        private static readonly int DANCE_ANIM_HASH = Animator.StringToHash("Dance");

        public int Timer;

        [SerializeField] private AnimationClip[] clips;
        [SerializeField] private float armor;
        [SerializeField] private float health;
        [SerializeField] private bool isVisitble;

        //----------------------------------------------------------------------------------------  Fields group (other than Const and Static readonly)  
        public readonly List<int> CachedNumbers = new List<int>();
        protected readonly List<Vector2> poistions = new List<Vector2>();

        private Animation animation;
        [NonSerialized] public UnityEvent<bool> Closed; // use [NonSerialized] or [HideInInspector] for UnityEvents

        protected float cooldown;
        private Interaction<int> interactionsBuffer;

        [PublicAPI]
        public bool IsVisitble => isVisitble; // [PublicAPI] Public - used only from outside of Unity solution (by other solution)

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

        private void OnDisable() // is used for events unsubscription
        {
            Closed.RemoveListener(OnClosed);
        }

        private void OnDestroy() // is used for any needed clean-up
        {
            Destroyed?.Invoke(this);
        }

        //----------------------------------------------------------------------------------------  Properties group
        public bool IsInitialized { get; private set; }

        //----------------------------------------------------------------------------------------  Public methods group
        public void Initialize() =>
            IsInitialized = true;

        //----------------------------------------------------------------------------------------  Events and UnityEvents group
        public event Destroction<CodeStyleExample> Destroyed;

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

        private void OnClosed(bool _) => // use _, __, ___ for parameters name if required parameters are not used inside 
            cooldown = MAX_TIMER;

        //----------------------------------------------------------------------------------------  internal-protected-private methods group
        // methods that weren't called inside methods above 👆   
        internal void Method1() { }
        protected void Method2() { }
        private void Method3() { }

        //----------------------------------------------------------------------------------------  Enum, delegates declaration group (protected/private) 
        private enum Direction { North, South, West, East }
        private delegate void Interaction<T> (T current);

        //----------------------------------------------------------------------------------------  Nested classes/struct group
        private struct Data
        {
            public int A;
            public bool B;
        }
    }
}