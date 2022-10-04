using UnityEngine;

namespace OutlineAdvance
{
    [RequireComponent(typeof(Renderer))]
    public class OutlineEdgeURP : MonoBehaviour
    {
        // Outline activation method enum
        [SerializeField] private enum ActivationMethod { MouseEnter = 0, MouseLeftClick, MouseRightClick };

        // Outline draw method enum
        [SerializeField] private enum OutlineRenderMethod { OutlineStandard = 0 , OutlineVisible , OutlineOccluded, OutlineAndFill };

        [Header("Activation Methods")]
        [Space]
        [SerializeField] private ActivationMethod m_ActivationMethod = ActivationMethod.MouseEnter;

        [Header("Outline Rendering Methods")]
        [Space]
        [SerializeField] private OutlineRenderMethod m_OutlineRenderMethod = OutlineRenderMethod.OutlineStandard;
        [SerializeField] private bool m_isOutlineConstant = false;

        [Header("Outline Properties")]
        [Space]

        // outline color width , amount
        [SerializeField] private Color m_OutlineColor = Color.green;

        [SerializeField] [Range(1f, 10f)] private float m_OutlineWidth = 2f;
        [SerializeField] [Range(0f, 1f)] private float m_OutlineAmount = 1f;

        [Space]
        // overlay color
        [SerializeField] private Color m_OverlayColor = Color.black;
        // overlay amount
        [SerializeField] [Range(0f, 1f)] private float m_OverlayAmount = 1f;

        [Space]
        // vertex color sensitive case
        [SerializeField] private bool m_VertexColorRedChannel = false;

        [Header("Shader")]
        [Space]
        // original gameobject shader
        Shader m_ShaderOriginal;

        // outline shader
        [SerializeField] private Shader m_ShaderOutline;

        private Renderer m_Renderer;
        private bool IsMouseCursorOn = false;

        private Material[] materials;

        [Tooltip("Set TRUE if you want to change parameters in Runtime")]
        [Space]
        [Header("Runtime Control")]
        [SerializeField] private bool isMaterialParmsRuntime = true;

        [Header("Debug")]
        [SerializeField] private bool isDebug;

        #region Awake - Start - Update

        void Awake()
        {
            // init 
            Init();
        }

        void Start()
        {
            // checks
            ActivationChecks();

            // pass material parameters
            MaterialParms();
        }
        void Update()
        {
            if (isMaterialParmsRuntime == true)
            {
                // checks
                ActivationChecks();

                // pass material parameters
                MaterialParms();
            }
        }

        #endregion

        #region Non Runtime Init

        void Init()
        {
            m_Renderer = GetComponent<Renderer>();
            m_ShaderOriginal = m_Renderer.material.shader;

            // material parameters
            materials = m_Renderer.materials;
        }

        #endregion

        #region Material Parameters

        // pass material parameters
        void MaterialParms()
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (m_OutlineRenderMethod == OutlineRenderMethod.OutlineAndFill)
                {
                    // overlay color
                    materials[i].SetColor("_OverlayColor", m_OverlayColor);
                    // overlay amount
                    materials[i].SetFloat("_Overlay", m_OverlayAmount);
                }

                if (m_OutlineRenderMethod != OutlineRenderMethod.OutlineAndFill)
                {
                    // overlay amount
                    materials[i].SetFloat("_Overlay", m_OverlayAmount);
                }

                // overlay color
                materials[i].SetColor("_OverlayColor", m_OverlayColor);

                materials[i].SetFloat("_OutlineWidth", m_OutlineWidth);
                materials[i].SetColor("_OutlineColor", m_OutlineColor);

                materials[i].SetFloat("_OutlineAmount", m_OutlineAmount);
                materials[i].SetFloat("_OutlineBasedVertexColorR", m_VertexColorRedChannel ? 0f : 1f);

                if (isDebug == true)
                {
                    Debug.Log("Executing on material : " + materials[i].name.ToString());

                }

                // control passes
                ControlShaderPass(i);

            }
        }

        #endregion

        #region Control Shader Pass for Material Secondary passes

        void ControlShaderPass(int i)
        {
            switch (m_OutlineRenderMethod)
            {
                case OutlineRenderMethod.OutlineStandard:

                    materials[i].SetInt("_ZTest2ndPass", (int)UnityEngine.Rendering.CompareFunction.Always);
                    materials[i].SetInt("_ZTest3rdPass", (int)UnityEngine.Rendering.CompareFunction.Always);
                    break;

                case OutlineRenderMethod.OutlineVisible:

                    materials[i].SetInt("_ZTest2ndPass", (int)UnityEngine.Rendering.CompareFunction.Always);
                    materials[i].SetInt("_ZTest3rdPass", (int)UnityEngine.Rendering.CompareFunction.LessEqual);

                    break;
                case OutlineRenderMethod.OutlineOccluded:

                    materials[i].SetInt("_ZTest2ndPass", (int)UnityEngine.Rendering.CompareFunction.Always);
                    materials[i].SetInt("_ZTest3rdPass", (int)UnityEngine.Rendering.CompareFunction.Greater);

                    break;
                case OutlineRenderMethod.OutlineAndFill:

                    materials[i].SetInt("_ZTest2ndPass", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                    materials[i].SetInt("_ZTest3rdPass", (int)UnityEngine.Rendering.CompareFunction.Always);
                    break;

                default:
                    materials[i].SetInt("_ZTest2ndPass", (int)UnityEngine.Rendering.CompareFunction.Always);
                    materials[i].SetInt("_ZTest3rdPass", (int)UnityEngine.Rendering.CompareFunction.Always);
                    break;
            }

        }

        #endregion

        #region Activation Methods Mouse Input

        // activation checks based on mouse input
        void ActivationChecks()
        {
            if (m_ActivationMethod == ActivationMethod.MouseRightClick)
            {
                bool mouseOnTarget = IsMouseCursorOn && Input.GetMouseButton(1);

                if (mouseOnTarget == true)
                {
                    SetOutlineOn();
                }

                else
                {
                    SetOutlineOff();
                }

            }
            else if (m_ActivationMethod == ActivationMethod.MouseLeftClick)
            {
                bool mouseOnTarget = IsMouseCursorOn && Input.GetMouseButton(0);

                if (mouseOnTarget == true)
                {
                    SetOutlineOn();
                }

                else
                {
                    SetOutlineOff();
                }

            }
        }

        #endregion

        #region Outline Methods On - Off

        void SetOutlineOn()
        {
            materials = m_Renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].shader = m_ShaderOutline;
            }

        }
        void SetOutlineOff()
        {
            if (m_isOutlineConstant == true)
            {
                return;
            }

            materials = m_Renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].shader = m_ShaderOriginal;
            }

        }

        #endregion

        #region Mouse Methods

        void OnMouseEnter()
        {
            IsMouseCursorOn = true;

            if (m_ActivationMethod == ActivationMethod.MouseEnter)
            {
                SetOutlineOn();
            }

        }
        void OnMouseExit()
        {
            IsMouseCursorOn = false;

            SetOutlineOff();
        }

        #endregion

    }
}