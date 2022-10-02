using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Events;

namespace UIComponents.Scripts.Components.SectionSelector
{
    public interface IInitializable
    {
        void Initialize();
    }

    public delegate void Interaction<T> (T current);

    public enum Side { Left, Right }

    public class CodeStyleExample1 : MonoBehaviour
    {

        public const string ASSET_PATH = "AvatarPrefab";                                // Constants group 
        internal const float MIN_TIME = 0.1f;
        private const float MAX_TIMER = 10f;

        public static readonly int WaveAnimHash = Animator.StringToHash("Wave");    // Static Read-only group
        private static readonly int danceAnimHash = Animator.StringToHash("Dance");

        public static bool IsOpen;                                                       // Static group
        internal static bool isAvtive;

        public int PublicFieldA;                                                          // Public Fields group
        [HideInInspector] public int SomeField02;

        [SerializeField] internal Animator animator;                                      // [SerializedField]'s group
        [SerializeField] private AnimationClip[] clips;

        public readonly List<int> CachedNumbers = new List<int>();                        // Read-only group
        protected readonly List<BitVector32.Section> sections = new List<BitVector32.Section>();
        [NonSerialized] public UnityEvent Closed;

        protected float cooldown;                                                         // internal-protected-private Fields group  
        private bool isVisible;
        [NonSerialized] public int SomeField03;

        public bool Property3 { get; private set; }
        protected bool Property5 { get; set; }
        private bool Property6 { get; set; }

        private void Awake()
        {
            // GetComponent
            SomeField03++;
        }

        private void OnEnable()
        {
            // subscribe?
        }

        private void OnDisable()
        {
            // unsubscribe?
        }
        private void OnCollisionEnter(Collision collision)
        {
            SomeField02++;
            SomeField03++;
        }

        public event Interaction<bool> Interacted;                                      // Events group 
        private void OnDisable2() { }

        public void Hide() =>
            SetVisibility(visible: true);

        internal void OnChangedVisibility(bool current, bool _) =>
            isVisible = current;

        private void SetVisibility(bool visible) =>
            isVisible = visible;
        private enum Direction { North, South, West, East }

        private delegate void Change<T> (T current);

        private struct Data
        {
            public int A;
            public bool B;
        }
    }
}