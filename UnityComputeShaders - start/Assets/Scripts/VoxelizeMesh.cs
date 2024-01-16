﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelizeMesh : MonoBehaviour 
{
    public Mesh meshToVoxelize;
    public int yParticleCount = 4;
    public int layer = 9;

    float particleSize = 0;

    public float ParticleSize{
        get{
            return particleSize; 
        }
    }

    List<Vector3> positions = new List<Vector3>();

    public List<Vector3> PositionList
    {
        get
        {
            return positions;
        }
    }

    public void Voxelize(Mesh mesh)
    {
        GameObject go = new GameObject("Voxelized Mesh");
        go.layer = layer;
        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        Vector3 minExtents = mc.bounds.center - mc.bounds.extents;
        Vector3 maxExtents = mc.bounds.center + mc.bounds.extents;
        RaycastHit hit;
        float radius = mesh.bounds.extents.y / yParticleCount;
        particleSize = radius * 2;
        Vector3 rayOffset = minExtents;
        Vector3 counts = mesh.bounds.extents / radius;
        Vector3Int particleCounts = new Vector3Int((int)counts.x, (int)counts.y, (int)counts.z);
        
        if(particleCounts.x % 2 == 0)
        {
            minExtents.x+=(mesh.bounds.extents.x - particleCounts.x * radius);
        }
        float offsetZ = 0;
        if(particleCounts.z % 2 == 0)
        {
            offsetZ+=(mesh.bounds.extents.z - particleCounts.z * radius);
        }

        rayOffset.y += radius;
        
        int layerMask = 1 << layer;
        while (rayOffset.y < maxExtents.y)
        {
            rayOffset.x = minExtents.x;
            while (rayOffset.x < maxExtents.x)
            {
                Vector3 rayOrigin = go.transform.position + rayOffset;
                
                if(Physics.Raycast(rayOrigin, Vector3.forward, out hit, 100, layerMask))
                {
                    Vector3 frontPt = hit.point;
                    rayOrigin.z += maxExtents.z * 2;
                    if(Physics.Raycast(rayOrigin, Vector3.back, out hit, 100, layerMask))
                    {
                        Vector3 backPt = hit.point;
                        int n = Mathf.CeilToInt(frontPt.z / ParticleSize);
                        frontPt.z = n * ParticleSize;
                        while (frontPt.z < backPt.z)
                        {
                            float gap = backPt.z - frontPt.z;
                            if(gap < radius*.5f)
                            {
                                break;  
                            }
                            
                            positions.Add(frontPt);
                            frontPt.z += ParticleSize;
                        }
                    }
                }

                rayOffset.x += particleSize;
            }
            rayOffset.y += particleSize;

        }

    }
}
