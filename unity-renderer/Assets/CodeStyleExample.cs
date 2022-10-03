using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UIComponents.Scripts.Components.SectionSelector
{
    public enum Side { Left, Right }

    public delegate void Change<T> (T current);

    public interface IInitializable
    {
        bool IsInitialized { get; }

        void Initialize();
    }

    public class CodeStyleExample1 : MonoBehaviour, IInitializable
    {

        public const string ASSET_PATH = "AvatarPrefab";
        private const float MAX_TIMER = 10f;

        public static readonly int WaveAnimHash = Animator.StringToHash("Wave");
        private static readonly int danceAnimHash = Animator.StringToHash("Dance");

        public int PublicFieldA;

        [SerializeField] private AnimationClip[] clips;
        [SerializeField] private bool isVisitble;

        public readonly List<int> CachedNumbers = new List<int>();
        protected readonly List<Vector2> poistions = new List<Vector2>();
        [NonSerialized] public UnityEvent Closed;

        protected float cooldown;
        private Interaction<int> interactionsBuffer;
        public bool IsVisitble => isVisitble;

        private void Awake()
        {
            // get references - GetComponent 
        }

        private void Start()
        {
            // Initialization logic
        }

        private void OnEnable()
        {
            Changed += OnInteracted;
            // subscribe?
        }

        private void OnDisable()
        {
            // unsubscribe?
        }

        private void OnDestroy()
        {
            //    
        }

        private void OnCollisionEnter(Collision collision)
        {
            SomeField02++;
            SomeField03++;
        }

        public bool IsInitialized { get; private set; }

        public void Initialize()
            => IsInitialized = true;

        public event Change<bool> Changed;

        public void Play(float speed)
        {
            AnimationClip animationClip;
        }

        public void Hide() =>
            SetVisibility(visible: true);

        internal void OnChangedVisibility(bool current, bool _) =>
            isVisible = current;

        private void SetVisibility(bool visible) =>
            isVisible = visible;

        public void ApplyDamage(float damage)
        {
            float resultDamage = ComputeDamage(damage);
            TakeHit(resultDamage);
        }

        private void ComputeDamage() => A();
        public void DoSomething()
        {
            // Code
            // Code
            DoSomethingSpecifics1();
            DoSomethingSpecifics2();
            // Code
        }

        // Only called by DoSomething
        private void DoSomethingSpecifics1()
        {
            // Code
        }
        private enum Direction { North, South, West, East }
        private delegate void Interaction<T> (T current);

        private struct Data
        {
            public int A;
            public bool B;
        }
    }
}