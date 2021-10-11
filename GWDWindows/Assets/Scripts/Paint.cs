using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class Paint : MonoBehaviour{
    public Vector2 positionOffset;
    public Vector2 size;
    public Color windowDirtColor = Color.black;
    private static readonly int Corners = Shader.PropertyToID("_Corners");
    [SerializeField]
    private Material _paintMaterial;
    [SerializeField]
    //public CustomRenderTexture _rt;
    //[SerializeField,Range(0f, 100.0f)]
    //WASNT SURE IF THIS WAS THE ONE TO REMOVE SO I COMMENTED IT. APPLY CHANGES IF NEEDED
    private CustomRenderTexture _rt;
    [SerializeField,Range(0f, 1f)]
    private float dirtRate;
    [SerializeField]
    private float spongeRadius;

    private int textureSize = 1024;
    private Vector2 uvCenter = Vector2.zero;

    void FixedUpdate(){
        _paintMaterial.SetVector("_CirclePosition", uvCenter);
        _paintMaterial.SetFloat("_DirtRate", 1 - dirtRate * Time.fixedDeltaTime);
        _rt.Update();
    }

    private void Start(){
        _paintMaterial = new Material(Shader.Find("Unlit/Painter"));
        _rt = new CustomRenderTexture(textureSize, textureSize, GraphicsFormat.B8G8R8A8_UNorm)
        {
            dimension = TextureDimension.Tex2D,
            doubleBuffered = true,
            initializationMode = CustomRenderTextureUpdateMode.OnLoad,
            initializationColor = windowDirtColor,
            updateMode = CustomRenderTextureUpdateMode.OnLoad,
            updateZoneSpace = CustomRenderTextureUpdateZoneSpace.Normalized,
            filterMode = FilterMode.Point,
            material = _paintMaterial,
            shaderPass = 0,
        };
        
        _rt.material = _paintMaterial;
        {
            RenderTexture.active = _rt;
            Texture2D tex = new Texture2D(textureSize, textureSize);
        }
        _rt.Create();
        _paintMaterial.SetTexture("_Tex", _rt);
        _paintMaterial.SetFloat("_DirtRate", 1 - dirtRate * Time.fixedDeltaTime); //update the dirt frequency
        _paintMaterial.SetFloat("_Radius", spongeRadius);
        MeshRenderer mr = GetComponent<MeshRenderer>(); 
        mr.material.SetTexture("_BaseMap", _rt);
        
        mr.material.SetColor("_BaseColor", Color.white);
    }

    public void UpdateCircle(Vector2 center)
    {
        //Vector2 uvCenter = new Vector2((center.x + positionOffset.x) / size.x, ) ;
        uvCenter = -(center + positionOffset) / size;
    }
}
