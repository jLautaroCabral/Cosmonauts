using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomMarchingSquares
{
    public class VoxelMap : MonoBehaviour
    {
        public float size = 2f;
        public int voxelResolution = 8;
        public int chunkResolution = 2;

        public VoxelGrid voxelGridPrefab;

        private VoxelGrid[] chunks;
        private float chunkSize, voxelSize, halfSize;

        private static string[] fillTypeNames = { "Filled", "Empty" };
        private int fillTypeIndex;

        private static string[] radiusNames = { "0", "1", "2", "3", "4", "5" };
        private int radiusIndex;

        private static string[] stencilNames = { "Square", "Circle" };
        private int stencilIndex;
        private VoxelStencil[] stencils;



        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
            GUILayout.Label("Fill Type");
            fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames, 2);
            GUILayout.Label("Radius");
            radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 6);
            GUILayout.Label("Stencil");
            stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames, 2);
            GUILayout.EndArea();
        }

        private void Awake()
        {
            stencils = new VoxelStencil[] { new VoxelStencil(), new VoxelStencilCircle() };

            halfSize = size * 0.5f;
            chunkSize = size / chunkResolution;
            voxelSize = chunkSize / voxelResolution;

            chunks = new VoxelGrid[chunkResolution * chunkResolution];
            for (int i = 0, y = 0; y < chunkResolution; y++)
            {
                for (int x = 0; x < chunkResolution; x++, i++)
                {
                    CreateChunk(i, x, y);
                }
            }

            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(size, size);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
                {
                    if (hitInfo.collider.gameObject == gameObject)
                    {
                        EditVoxels(transform.InverseTransformPoint(hitInfo.point));
                    }
                }
            }
        }

        private void EditVoxels(Vector3 point)
        {
            int centerX = (int)((point.x + halfSize) / voxelSize);
            int centerY = (int)((point.y + halfSize) / voxelSize);

            int xStart = (centerX - radiusIndex - 1) / voxelResolution;
            xStart = xStart < 0 ? 0 : xStart;

            int XEnd = (centerX + radiusIndex) / voxelResolution;
            XEnd = XEnd >= chunkResolution ? chunkResolution - 1 : XEnd;

            int YStart = (centerY - radiusIndex - 1) / voxelResolution;
            YStart = YStart < 0 ? 0 : YStart;

            int YEnd = (centerY + radiusIndex) / voxelResolution;
            YEnd = YEnd >= chunkResolution ? chunkResolution - 1 : YEnd;

            VoxelStencil activeStencil = stencils[stencilIndex];
            activeStencil.Initialize(fillTypeIndex == 0, radiusIndex);


            int voxelYOffset = YEnd * voxelResolution;
            for (int y = YEnd; y >= YStart; y--)
            {
                int i = y * chunkResolution + XEnd;
                int voxelXOffset = XEnd * voxelResolution;
                for (int x = XEnd; x >= xStart; x--, i--)
                {
                    activeStencil.SetCenter(centerX - voxelXOffset, centerY - voxelYOffset);
                    chunks[i].Apply(activeStencil);
                    voxelXOffset -= voxelResolution;
                }

                voxelYOffset -= voxelResolution;
            }
        }

        private void CreateChunk(int i, int x, int y)
        {
            VoxelGrid chunk = Instantiate(voxelGridPrefab);
            chunk.Initialize(voxelResolution, chunkSize);
            chunk.transform.parent = transform;
            chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
            chunks[i] = chunk;

            if (x > 0)
            {
                chunks[i - 1].xNeighbor = chunk;
            }
            if (y > 0)
            {
                chunks[i - chunkResolution].yNeighbor = chunk;
                if (x > 0)
                    chunks[i - chunkResolution - 1].xyNeighbor = chunk;
            }
        }
    }

}