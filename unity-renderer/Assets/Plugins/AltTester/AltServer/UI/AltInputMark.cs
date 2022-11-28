public class AltInputMark : UnityEngine.MonoBehaviour
{
    public UnityEngine.CanvasGroup CanvasGroup;
    public UnityEngine.AnimationCurve VisibilityCurve;

    public int Id { get; private set; }
    public UnityEngine.Transform Transform
    {
        get
        {
            if (_transform == null)
                _transform = GetComponent<UnityEngine.Transform>();
            return _transform;
        }
    }
    private UnityEngine.Transform _transform;

    private System.Action<AltInputMark> _onFinished;
    private float _time;
    private float _currentTime;

    private void Awake()
    {
        Id = GetInstanceID();

        if (CanvasGroup != null)
            CanvasGroup.alpha = 0;
    }

    public void Init(float time, System.Action<AltInputMark> onFinished)
    {
        _time = time;
        _onFinished = onFinished;
    }

    public void Show(UnityEngine.Vector2 pos)
    {
        Transform.localPosition = pos;
        CanvasGroup.alpha = 1;
        _currentTime = 0;
    }

    private void Update()
    {
        CanvasGroup.alpha = VisibilityCurve.Evaluate(_currentTime);

        _currentTime += UnityEngine.Time.unscaledDeltaTime / _time;
        if (_currentTime < _time)
            return;

        Finish();
    }

    private void Finish()
    {
        if (_onFinished != null)
            _onFinished.Invoke(this);
        else
            gameObject.SetActive(false);
    }
}