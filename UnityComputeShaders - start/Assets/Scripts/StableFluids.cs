// StableFluids - A GPU implementation of Jos Stam's Stable Fluids on Unity
// adapted from https://github.com/keijiro/StableFluids

using UnityEngine;


public class StableFluids : MonoBehaviour
{
    public int resolution = 512;
    public float viscosity = 1e-6f;
    public float force = 300;
    public float exponent = 200;
    public Texture2D initial;
    public ComputeShader compute;
    public Material material;
   
    Vector2 previousInput;

    int kernelAdvect;
    int kernelForce;
    int kernelProjectSetup;
    int kernelProject;
    int kernelDiffuse1;
    int kernelDiffuse2;

    int threadCountX { get { return (resolution + 7) / 8; } }
    int threadCountY { get { return (resolution * Screen.height / Screen.width + 7) / 8; } }

    int resolutionX { get { return threadCountX * 8; } }
    int resolutionY { get { return threadCountY * 8; } }

    // Vector field buffers
    RenderTexture vfbRTV1;
    RenderTexture vfbRTV2;
    RenderTexture vfbRTV3;
    RenderTexture vfbRTP1;
    RenderTexture vfbRTP2;

    // Color buffers (for double buffering)
    RenderTexture colorRT1;
    RenderTexture colorRT2;

    RenderTexture CreateRenderTexture(int componentCount, int width = 0, int height = 0)
    {
        RenderTextureFormat format = RenderTextureFormat.ARGBHalf;
        if (componentCount == 1)
        {
            format = RenderTextureFormat.RHalf;
        }
        else if (componentCount == 2)
        {
            format = RenderTextureFormat.RGHalf;
        }
        
        if (width == 0)
        {
            width = resolutionX;
        }
        if (height == 0)
        {
            height = resolutionY;
        }
          
        RenderTexture rt = new RenderTexture(width, height, 0, format);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    
    void OnValidate()
    {
        resolution = Mathf.Max(resolution, 8);
    }

    void Start()
    {
        InitBuffers();
        InitShader();

        Graphics.Blit(initial, colorRT1);
    }

    void InitBuffers()
    {
        vfbRTV1 = CreateRenderTexture(2);
        vfbRTV2 = CreateRenderTexture(2);
        vfbRTV3 = CreateRenderTexture(2);
        vfbRTP1 = CreateRenderTexture(1);
        vfbRTP2 = CreateRenderTexture(1);
        colorRT1 = CreateRenderTexture(4, Screen.width, Screen.height);
        colorRT2 = CreateRenderTexture(4, Screen.width, Screen.height);
    }

    void InitShader()
    {
        // Find the kernels in the compute shader
        kernelAdvect = compute.FindKernel("Advect");
        kernelForce = compute.FindKernel("Force");
        kernelProjectSetup = compute.FindKernel("ProjectSetup");
        kernelProject = compute.FindKernel("Project");
        kernelDiffuse1 = compute.FindKernel("Diffuse1");
        kernelDiffuse2 = compute.FindKernel("Diffuse2");

        // Set the textures for the kernels
        compute.SetTexture(kernelAdvect, "U_in", vfbRTV1);
        compute.SetTexture(kernelAdvect, "W_out", vfbRTV2);
        
        compute.SetTexture(kernelDiffuse2, "B2_in", vfbRTV1);
        
        compute.SetTexture(kernelForce, "W_in", vfbRTV2);
        compute.SetTexture(kernelForce, "W_out", vfbRTV3);
        
        compute.SetTexture(kernelProjectSetup, "W_in", vfbRTV3);
        compute.SetTexture(kernelProjectSetup, "DivW_out", vfbRTV2);
        compute.SetTexture(kernelProjectSetup, "P_out", vfbRTP1);
        
        compute.SetTexture(kernelDiffuse1, "B1_in", vfbRTV2);
        
        compute.SetTexture(kernelProject, "W_in", vfbRTV3);
        compute.SetTexture(kernelProject, "P_in", vfbRTP1);
        compute.SetTexture(kernelProject, "U_out", vfbRTV1);

        compute.SetFloat("ForceExponent", exponent);
        
        material.SetFloat("_ForceExponent", exponent);
        material.SetTexture("_VelocityField", vfbRTV1);
    }

    void OnDestroy()
    {
        Destroy(vfbRTV1);
        Destroy(vfbRTV2);
        Destroy(vfbRTV3);
        Destroy(vfbRTP1);
        Destroy(vfbRTP2);
        Destroy(colorRT1);
        Destroy(colorRT2);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float dx = 1.0f / resolutionY;

        // Input point
        Vector2 input = new Vector2(
            (Input.mousePosition.x - Screen.width * 0.5f) / Screen.height,
            (Input.mousePosition.y - Screen.height * 0.5f) / Screen.height
        );

        // Common variables
        compute.SetFloat("Time", Time.time);
        compute.SetFloat("DeltaTime", dt);

        //Add code here



        previousInput = input;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(colorRT1, destination, material, 1);
    }
}
