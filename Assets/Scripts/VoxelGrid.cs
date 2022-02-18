using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class VoxelGrid : MonoBehaviour
{
    public GameObject voxelPrefab;
    public int resolution;
    private float voxelSize;
    private bool[] voxels;

    private Material[] voxelMaterials;

    

    internal void Initialize(int resolution, float size)
    {
        this.resolution = resolution;
        voxelSize = size / resolution;
        voxels = new bool[resolution * resolution];
        voxelMaterials = new Material[voxels.Length];

        for (int i = 0, y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
            {
                CreateVoxel(i, x, y);
            }
        }

        SetVoxelColors();
    }

    private void CreateVoxel(int i, int x, int y)
    {
        GameObject o = Instantiate(voxelPrefab);
        o.transform.parent = transform;
        o.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize);
        o.transform.localScale = Vector3.one * voxelSize * 0.9f;
        voxelMaterials[i] = o.GetComponent<MeshRenderer>().material;
    }

    private void SetVoxelColors()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            voxelMaterials[i].color = voxels[i] ? Color.black : Color.white;
        }
    }

    internal void Apply(VoxelStencil stencil)
    {
        int XStart = stencil.XStart;
        XStart = XStart < 0 ? 0 : XStart;

        int XEnd = stencil.XEnd;
        XEnd = XEnd >= resolution ? resolution - 1 : XEnd;

        int YStart = stencil.YStart;
        YStart = YStart < 0 ? 0 : YStart;

        int YEnd = stencil.YEnd;
        YEnd = YEnd >= resolution ? resolution - 1 : YEnd;

        for (int y = YStart; y <= YEnd; y++)
        {
            int i = y * resolution + XStart;
            for (int x = XStart; x <= XEnd; x++, i++)
            {
                voxels[i] = stencil.Apply(x, y, voxels[i]);
            }
        }
        SetVoxelColors();
    }
}
