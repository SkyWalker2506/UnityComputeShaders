using UnityEngine;

public class BufferJoy : MonoBehaviour
{

    public ComputeShader shader;
    public int texResolution = 1024;

    Renderer rend;
    RenderTexture outputTexture;

    int circlesHandle;
    int clearHandle;

    public Color clearColor = new Color();
    public Color circleColor = new Color();

    int count = 10;
    Circle[] circleData;
    ComputeBuffer circleBuffer;
    
    void Start()
    {
        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;

        InitData();

        InitShader();
    }

    private void InitData()
    {
        circlesHandle = shader.FindKernel("Circles");
        
        uint threadGroupSizeX;
        shader.GetKernelThreadGroupSizes(circlesHandle, out threadGroupSizeX, out _, out _);
        
        int totalThreads = (int)threadGroupSizeX * count;
        
        circleData = new Circle[totalThreads];
        
        float speed = 100;
        float halfSpeed = speed * 0.5f;
        float minRadius = 10;   
        float maxRadius = 30;
        float radiusRange = maxRadius - minRadius;

        for (int i = 0; i < totalThreads; i++)
        {
            Circle circle = circleData[i];
            circle.center.x = Random.value* texResolution;
            circle.center.y = Random.value* texResolution;
            circle.velocity.x = Random.value * speed - halfSpeed;
            circle.velocity.y = Random.value * speed - halfSpeed;
            circle.radius = Random.value * radiusRange + minRadius;
            circleData[i] = circle;
        }
    }

    private void InitShader()
    {
    	clearHandle = shader.FindKernel("Clear");
    	
        shader.SetVector( "clearColor", clearColor );
        shader.SetVector( "circleColor", circleColor );
        shader.SetInt( "texResolution", texResolution );
		
		shader.SetTexture( clearHandle, "Result", outputTexture );
        shader.SetTexture( circlesHandle, "Result", outputTexture );
        int stride = (2+2+1)*sizeof(float);
        circleBuffer = new ComputeBuffer(circleData.Length, stride);
        circleBuffer.SetData(circleData);
        shader.SetBuffer(circlesHandle, "circlesBuffer", circleBuffer);
        rend.material.SetTexture("_MainTex", outputTexture);
    }
 
    private void DispatchKernels(int count)
    {
    	shader.Dispatch(clearHandle, texResolution/8, texResolution/8, 1);
        shader.SetFloat("time", Time.time);
        shader.Dispatch(circlesHandle, count, 1, 1);
    }

    void Update()
    {
        DispatchKernels(count);
    }

    void OnDestroy()
    {
        circleBuffer.Release();
    }
    
    struct Circle
    {
        public Vector2 center;
        public Vector2 velocity;
        public float radius;
    
    }
}

