using System;
using UnityEngine;

namespace DefaultNamespace
{
    public enum Side { Left, Right }

    public class Example : MonoBehaviour, IInterface
    {

        public const string ASSET_PATH = "AvatarUI";

        private const int MAX_TIMER = 10;
        public static readonly int WaveAnimHash = Animator.StringToHash("Wave");
        private static readonly int danceAnimHash = Animator.StringToHash("Dance");

        public int SomeField01;

        [HideInInspector] public int SomeField02;

        [SerializeField] private Collider collider;
        [NonSerialized] public int SomeField03;

        internal bool someField1; // Pascal or Camel ?
        private int someField14;
        protected int someField2;

        public bool Property1 { get; set; }
        public bool Property2 { private get; set; }
        public bool Property3 { get; }
        public bool Property4 { get; private set; }

        private void Awake()
        {

            // GetComponent
        }

        private void OnEnable()
        {
            // subscribe?
        }

        private void OnDisable()
        {
            // unsubscribe?
        }

        private void OnCollisionEnter(Collision collision) { }

        public void Hide() =>
            SetVisibility(isVisible: true);

        internal void OnChangedVisibility(bool current, bool _) =>
            someField1 = current;

        private void SetVisibility(bool isVisible) =>
            someField1 = isVisible;
    }

    public interface IInterface { }
}