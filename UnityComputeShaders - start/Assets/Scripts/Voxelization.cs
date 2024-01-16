using UnityEngine;

public class Voxelization : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        VoxelizeMesh vm = GetComponent<VoxelizeMesh>();
        vm.Voxelize(vm.meshToVoxelize);
        float ps = vm.ParticleSize;
        Vector3 scale = new Vector3(ps, ps, ps);
        
        foreach(Vector3 pos in vm.PositionList)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position = pos;
            particle.transform.localScale = scale;
            particle.transform.parent = transform;
        }
    }
}
