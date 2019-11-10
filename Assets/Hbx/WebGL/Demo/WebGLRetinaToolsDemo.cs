using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace Hbx.WebGL
{

	/// <summary>
	/// Simple demo displaying current resolution, mouse/touch coord info and current device pixel ratio info
	/// The slider allows user to dynamically alter the device pixel ratio used in webgl builds, enable Auto
	/// the active the DynamicResolutionManager and automatically adjust resolution based on performance
	/// </summary>

	public class WebGLRetinaToolsDemo : MonoBehaviour
    {
		/// <summary>
		/// Reference to the scenes resolution manager so we can toggle it on/off
		/// </summary>

		public DynamicResolutionManger _resolutionManager;
	
		/// <summary>
		/// Reference to the main UI canvas
		/// </summary>

		public Canvas _canvas;

		/// <summary>
		/// Text object to display view resolution
		/// </summary>

		public Text _resolutionText;

		/// <summary>
		/// Text object to display device pixel ratio information
		/// </summary>

		public Text _dprText;

		/// <summary>
		/// Text object to display current mouse/touch coords
		/// </summary>

		public Text _cursorText;

		/// <summary>
		/// Reference to the exit fullscreen ui button
		/// </summary>

		public Button _fullscreenButton;

		/// <summary>
		/// Reference to the slider used to control device pixel ratio
		/// </summary>

		public Slider _dprSlider;
	
		/// <summary>
		/// Used to track fps
		/// </summary>

		float _deltaTime = 0f;


		// Ripples

		/// <summary>
		/// Max number of ripples allow, should match the value in the shader
		/// </summary>
		const int MaxRipples = 10;

		/// <summary>
		/// Index of the ripple we should update next
		/// </summary>

		int _lastRippleIndex = 0;

		/// <summary>
		/// The ripple start times to pass to the shader
		/// </summary>

		float[] _rippleStartTimes = new float[MaxRipples];

		/// <summary>
		/// The ripple start/origin points in uv coords to pass to the shader
		/// </summary>
		Vector4[] _rippeStartPoints = new Vector4[MaxRipples];	

		/// <summary>
		/// Prefab object to use to spawn drop balls
		/// </summary>

		public GameObject _ballPrefab;
		
		/// <summary>
		/// Pool of pre instantiated ball prefabs to prevent stalls when droping them
		/// </summary>
		List<GameObject> _ballPool = new List<GameObject>();


		/// <summary>
		/// Get current device pixel ratio etc
		/// </summary>

		void Start()
		{
			// we want independant touches so we can test it's all working properly
			Input.simulateMouseWithTouches = false;

            // update slider with current dpr
            float nativedpr = Hbx.WebGL.DevicePixelRatio.GetWindowDevicePixelRatio();
			float activedpr = Hbx.WebGL.DevicePixelRatio.GetDevicePixelRatio();
			_dprSlider.value = (activedpr / nativedpr) * _dprSlider.maxValue;

			// set some initial ripple values
			for(int i=0; i<MaxRipples; i++)
			{
				_rippleStartTimes[i] = -100.0f;
				_rippeStartPoints[i] = Vector4.zero; //((Random.insideUnitCircle * 0.5f) + new Vector2(0.5f,0.5f));
			}

			// the background image has a material using the RippleShader, here we set some global shader props to control the ripples
			Shader.SetGlobalFloatArray("_RippleStartTimes", _rippleStartTimes);
			Shader.SetGlobalVectorArray("_RippleStartPoints", _rippeStartPoints);

			// fill our ball pool
			int poolsize = 10;
			for(int i=0; i<poolsize; i++)
			{
				// create a ball at the start position
				GameObject db = GameObject.Instantiate<GameObject>(_ballPrefab, new Vector3(0f,0f,-1000f), Quaternion.identity);
	
				// push the ball in the direction of the screen ray
				Rigidbody b = db.GetComponent<Rigidbody>();
				b.isKinematic = true;
				Renderer r = db.GetComponent<Renderer>();
				r.enabled = false;
				_ballPool.Add(db);
			}
			Destroy(_ballPrefab);

			// drop a ball in the middle of the screen
			TriggerDropBallAtScreenCoords(new Vector2(Screen.width, Screen.height) * 0.5f);
		}
	
		/// <summary>
		/// Update labels, hits etc.
		/// </summary>

		void Update()
		{
			// calc fps
			_deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
			float msec = _deltaTime * 1000.0f;
			float fps = 1.0f / _deltaTime;

			// update the resolutuion text with current screen res and viewport dimensions
			_resolutionText.text = Screen.width + "x" + Screen.height + "\nFps: " + fps.ToString("00.00");

			// update the dpr text with current dpr info
			_dprText.text = "Native dpr: " + Hbx.WebGL.DevicePixelRatio.GetWindowDevicePixelRatio().ToString("0.0") + "\nActive dpr: " + Hbx.WebGL.DevicePixelRatio.GetDevicePixelRatio().ToString("0.0");
	
			// only show the exit fullscreen button if we are in fullscreen mode
			_fullscreenButton.gameObject.SetActive(Screen.fullScreen);

            //HandleInput();

        }

        /// <summary>
        /// Triggered by pointer events on the Screen Overlay Object in the scene
        /// </summary>

        public void OnScreenOverlayPointerDownEvent(BaseEventData eventData)
        {
            PointerEventData pointerEvent = eventData as PointerEventData;

            Vector2 screenpoint = pointerEvent.position;

            // update cursor text
            _cursorText.text = Mathf.FloorToInt(screenpoint.x) + "x" + Mathf.FloorToInt(screenpoint.y);

            // position cursor text in canvas rect
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, screenpoint, _canvas.worldCamera, out pos);
            _cursorText.gameObject.transform.position = _canvas.transform.TransformPoint(pos) + new Vector3(0, 10, 0);

            TriggerDropBallAtScreenCoords(screenpoint);
        }

        /// <summary>
        /// Handle the standard Unity Input events per frame (not used at the moment)
        /// </summary>

        public void HandleInput()
        {

            // get the mouse position or touches
            if (Input.touchCount == 0) {
				// update cursor text
				_cursorText.text = Mathf.FloorToInt(Input.mousePosition.x) + "x" + Mathf.FloorToInt(Input.mousePosition.y);
		
				// position cursor text in canvas rect
				Vector2 pos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, Input.mousePosition, _canvas.worldCamera, out pos);
				_cursorText.gameObject.transform.position = _canvas.transform.TransformPoint(pos) + new Vector3(0,10,0);

				/*if(Input.GetMouseButtonDown(0))*/ TriggerDropBallAtScreenCoords(Input.mousePosition);

			}
            else
            {
				// get the first touch
				Touch t = Input.touches[0];

				// update cursor text
				_cursorText.text = Mathf.FloorToInt(t.position.x) + "x" + Mathf.FloorToInt(t.position.y);
		
				// position cursor text in canvas rect
				Vector2 pos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, t.position, _canvas.worldCamera, out pos);
				_cursorText.gameObject.transform.position = _canvas.transform.TransformPoint(pos) + new Vector3(0,10,0);

				/*if(t.phase == TouchPhase.Began)*/ TriggerDropBallAtScreenCoords(t.position);
			}
		}

		/// <summary>
		/// Instantiate a ballprefab at screen coords and fire it at the background
		/// </summary>
		/// <param name="screenCoords">Screen coords.</param>

		public void TriggerDropBallAtScreenCoords(Vector2 screenCoords)
		{
			if(_ballPool.Count == 0)
			{
				// could create a new one, but we should have enough at the start, just wait for one on next click
				return;
			}

			// get start position
			Ray screenray = Camera.main.ScreenPointToRay(screenCoords);	
			Vector3 startpoint = screenray.origin + (screenray.direction * 1f);

			// create a ball at the start position
			GameObject db =  _ballPool[0];// GameObject.Instantiate<GameObject>(_ballPrefab, startpoint, Quaternion.identity);
			db.transform.position = startpoint;

			Renderer r = db.GetComponent<Renderer>();
			r.enabled = true;

			// push the ball in the direction of the screen ray
			Rigidbody b = db.GetComponent<Rigidbody>();
			b.isKinematic = false;
			b.AddForce(screenray.direction*1000f);

			// litte hack to detect when it passes the background plane
			StartCoroutine(_DetectCollisionWithBackground(b, screenCoords));

			_ballPool.RemoveAt(0);
		}

		/// <summary>
		/// Keep checking for a collision with background. When it collides create a ripple and pool the ball
		/// </summary>
		/// <returns>Coroutine IEnumerator.</returns>
		/// <param name="aBody">A body.</param>
		/// <param name="aScreenCoord">A screen coordinate.</param>

		IEnumerator _DetectCollisionWithBackground(Rigidbody aBody, Vector2 aScreenCoord)
		{
			SphereCollider collider = aBody.gameObject.GetComponent<SphereCollider>();
			float planedist = -(collider.radius*1.1f);
			while(true)
			{
				if(aBody.position.z > planedist) 
				{
					TriggerRippleAtScreenCoords(aScreenCoord);
					yield return new WaitForSeconds(0.01f);
					aBody.velocity = Vector3.zero;
					aBody.isKinematic = true;
					_ballPool.Add(aBody.gameObject);
					Renderer r = aBody.GetComponent<Renderer>();
					r.enabled = false;

					yield break;
				}
				yield return null;
			}
		}

		/// <summary>
		/// Start a ripple in the shader at screen coords
		/// convets screen coords to uv coords, uses the current time as start time then passes the values to the shader
		/// </summary>
		/// <param name="screenCoords">Screen coords.</param>

		public void TriggerRippleAtScreenCoords(Vector2 screenCoords)
		{
			Vector2 uvcoord = new Vector2(screenCoords.x / Screen.width, screenCoords.y / Screen.height);

			_rippleStartTimes[_lastRippleIndex] = Time.time;
			_rippeStartPoints[_lastRippleIndex] = uvcoord;

			// update the shader values
			Shader.SetGlobalFloatArray("_RippleStartTimes", _rippleStartTimes);
			Shader.SetGlobalVectorArray("_RippleStartPoints", _rippeStartPoints);

			// increment the ripple index
			_lastRippleIndex = (_lastRippleIndex + 1) % MaxRipples;
		}

		//
		// UI Events

		/// <summary>
		/// Triggered when the exit fullscreen button is clicked
		/// </summary>

		public void OnFullscreenClicked()
		{
			Screen.fullScreen = !Screen.fullScreen;
		}

		/// <summary>
		/// Triggered when the dpr slider is changed
		/// </summary>
		/// <param name="aValue">A slider value.</param>

		public void OnDPRScaleSliderChanged(float aValue)
		{
			// scale the device pixel ratio based on the windows max/native value
			Hbx.WebGL.DevicePixelRatio.ScaleDevicePixelRatio( aValue / _dprSlider.maxValue );
		}

		public void OnAutoResolutionToggleChanged(bool isOn)
		{
			_resolutionManager.enabled = isOn;
			_resolutionManager.ResetQuality();
			
			_dprSlider.interactable = !isOn;

			//if(_dprSlider.interactable)
			{
				// update slider with current dpr
				float nativedpr = Hbx.WebGL.DevicePixelRatio.GetWindowDevicePixelRatio();
				float activedpr = Hbx.WebGL.DevicePixelRatio.GetDevicePixelRatio();
				_dprSlider.value = (activedpr / nativedpr) * _dprSlider.maxValue;
			}
		}
	}

} // end Hbx WebGL namespace
