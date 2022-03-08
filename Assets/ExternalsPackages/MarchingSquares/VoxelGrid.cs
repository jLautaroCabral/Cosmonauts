using UnityEngine;
using System.Collections.Generic;

[SelectionBase]
//[RequireComponent(typeof(PolygonCollider2D))]
public class VoxelGrid : MonoBehaviour {

	public int resolution;

	public GameObject voxelPrefab;

	public VoxelGrid xNeighbor, yNeighbor, xyNeighbor;

	private Voxel[] voxels;

	private float voxelSize, gridSize;

	private Material[] voxelMaterials;

	private Mesh mesh;

	private List<Vector3> vertices;
	private List<int> triangles;

	private Voxel dummyX, dummyY, dummyT;

	private List<PolygonCollider2D> polygonColliderList;

	PolygonCollider2D helperPolygonCollider2D;

	private void Awake()
    {
		polygonColliderList = new List<PolygonCollider2D>();
	}

	private void ClearColliders()
    {
        for (int i = 0; i < polygonColliderList.Count; i++)
        {
			Destroy(polygonColliderList[i]);
        }
		polygonColliderList.Clear();

	}

    public void Initialize (int resolution, float size) {
		this.resolution = resolution;
		gridSize = size;
		voxelSize = size / resolution;
		voxels = new Voxel[resolution * resolution];
		voxelMaterials = new Material[voxels.Length];

		dummyX = new Voxel();
		dummyY = new Voxel();
		dummyT = new Voxel();

		for (int i = 0, y = 0; y < resolution; y++) {
			for (int x = 0; x < resolution; x++, i++) {
				CreateVoxel(i, x, y);
			}
		}

		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "VoxelGrid Mesh";
		vertices = new List<Vector3>();
		triangles = new List<int>();
		Refresh();
	}

	private void CreateVoxel (int i, int x, int y) {
		GameObject o = Instantiate(voxelPrefab) as GameObject;
		o.transform.parent = transform;
		o.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize, -0.01f);
		o.transform.localScale = Vector3.one * voxelSize * 0.1f;
		voxelMaterials[i] = o.GetComponent<MeshRenderer>().material;
		voxels[i] = new Voxel(x, y, voxelSize);
	}

	private void Refresh () {
		SetVoxelColors();
		Triangulate();
	}
	
	private void Triangulate () {
		vertices.Clear();
		triangles.Clear();
		mesh.Clear();

		ClearColliders();

		if (xNeighbor != null) {
			dummyX.BecomeXDummyOf(xNeighbor.voxels[0], gridSize);
		}
		TriangulateCellRows();
		if (yNeighbor != null) {
			TriangulateGapRow();
		}

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
	}

	private void TriangulateCellRows () {
		int cells = resolution - 1;
		for (int i = 0, y = 0; y < cells; y++, i++) {
			for (int x = 0; x < cells; x++, i++) {
				TriangulateCell(
					voxels[i],
					voxels[i + 1],
					voxels[i + resolution],
					voxels[i + resolution + 1]);
			}
			if (xNeighbor != null) {
				TriangulateGapCell(i);
			}
		}
	}

	private void TriangulateGapCell (int i) {
		Voxel dummySwap = dummyT;
		dummySwap.BecomeXDummyOf(xNeighbor.voxels[i + 1], gridSize);
		dummyT = dummyX;
		dummyX = dummySwap;
		TriangulateCell(voxels[i], dummyT, voxels[i + resolution], dummyX);
	}

	private void TriangulateGapRow () {
		dummyY.BecomeYDummyOf(yNeighbor.voxels[0], gridSize);
		int cells = resolution - 1;
		int offset = cells * resolution;

		for (int x = 0; x < cells; x++) {
			Voxel dummySwap = dummyT;
			dummySwap.BecomeYDummyOf(yNeighbor.voxels[x + 1], gridSize);
			dummyT = dummyY;
			dummyY = dummySwap;
			TriangulateCell(voxels[x + offset], voxels[x + offset + 1], dummyT, dummyY);
		}

		if (xNeighbor != null) {
			dummyT.BecomeXYDummyOf(xyNeighbor.voxels[0], gridSize);
			TriangulateCell(voxels[voxels.Length - 1], dummyX, dummyY, dummyT);
		}
	}

	private void TriangulateCell (Voxel a, Voxel b, Voxel c, Voxel d) {
		int cellType = 0;
		if (a.state) {
			cellType |= 1;
		}
		if (b.state) {
			cellType |= 2;
		}
		if (c.state) {
			cellType |= 4;
		}
		if (d.state) {
			cellType |= 8;
		}

		switch (cellType) {
		case 0:
			return;

		case 1:
			CreatePolygon(new Vector2[] { a.position, a.yEdgePosition, a.xEdgePosition }, PolygonType.Triangle);
			break;

		case 2:
			CreatePolygon(new Vector2[] { b.position, a.xEdgePosition, b.yEdgePosition }, PolygonType.Triangle);
			break;

		case 3:
			CreatePolygon(new Vector2[] { a.position, a.yEdgePosition, b.yEdgePosition, b.position }, PolygonType.Quad);
			break;

		case 4:
			CreatePolygon(new Vector2[] { c.position, c.xEdgePosition, a.yEdgePosition }, PolygonType.Triangle);
			break;

		case 5:
			CreatePolygon(new Vector2[] { a.position, c.position, c.xEdgePosition, a.xEdgePosition }, PolygonType.Quad);
			break;

		case 6:
			CreatePolygon(new Vector2[] { b.position, a.xEdgePosition, b.yEdgePosition }, PolygonType.Triangle);
			CreatePolygon(new Vector2[] { c.position, c.xEdgePosition, a.yEdgePosition }, PolygonType.Triangle);
			break;

		case 7:
			CreatePolygon(new Vector2[] { a.position, c.position, c.xEdgePosition, b.yEdgePosition, b.position }, PolygonType.Pentagon);
			break;

		case 8:
			CreatePolygon(new Vector2[] { d.position, b.yEdgePosition, c.xEdgePosition }, PolygonType.Triangle);
			break;

		case 9:
			CreatePolygon(new Vector2[] { a.position, a.yEdgePosition, a.xEdgePosition }, PolygonType.Triangle);
			CreatePolygon(new Vector2[] { d.position, b.yEdgePosition, c.xEdgePosition }, PolygonType.Triangle);
			break;

		case 10:
			CreatePolygon(new Vector2[] { a.xEdgePosition, c.xEdgePosition, d.position, b.position }, PolygonType.Quad);
			break;

		case 11:
			CreatePolygon(new Vector2[] { b.position, a.position, a.yEdgePosition, c.xEdgePosition, d.position }, PolygonType.Pentagon);
			break;

		case 12:
			CreatePolygon(new Vector2[] { a.yEdgePosition, c.position, d.position, b.yEdgePosition }, PolygonType.Quad);
			break;

		case 13:
			CreatePolygon(new Vector2[] { c.position, d.position, b.yEdgePosition, a.xEdgePosition, a.position }, PolygonType.Pentagon);
			break;

		case 14:
			CreatePolygon(new Vector2[] { d.position, b.position, a.xEdgePosition, a.yEdgePosition, c.position }, PolygonType.Pentagon);
			break;

		case 15:
			CreatePolygon(new Vector2[] { a.position, c.position, d.position, b.position }, PolygonType.Quad);
			break;
		}

		List<Vector2> vertices2D = new List<Vector2>();
		foreach(Vector3 point in vertices)
        {
			vertices2D.Add(point);
		}

		//if (polygonCollider != null)
			//Destroy(polygonCollider);

		//polygonCollider = gameObject.AddComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
		//polygonCollider = Component
		//polygonCollider.points = vertices2D.ToArray();
	}

	private enum PolygonType { Triangle, Quad, Pentagon }
	private void CreatePolygon(Vector2[] polygonPoints, PolygonType polygonType)
    {
		switch(polygonType)
        {
			case PolygonType.Triangle:
				AddTriangle(polygonPoints[0], polygonPoints[1], polygonPoints[2]);
				helperPolygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
				helperPolygonCollider2D.usedByComposite = true;
				helperPolygonCollider2D.points = new Vector2[] { polygonPoints[0], polygonPoints[1], polygonPoints[2]};
				polygonColliderList.Add(helperPolygonCollider2D);
				break;

			case PolygonType.Quad:
				AddQuad(polygonPoints[0], polygonPoints[1], polygonPoints[2], polygonPoints[3]);
				helperPolygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
				helperPolygonCollider2D.usedByComposite = true;
				helperPolygonCollider2D.points = new Vector2[] { polygonPoints[0], polygonPoints[1], polygonPoints[2], polygonPoints[3] };
				polygonColliderList.Add(helperPolygonCollider2D);
				break;

			case PolygonType.Pentagon:
				AddPentagon(polygonPoints[0], polygonPoints[1], polygonPoints[2], polygonPoints[3], polygonPoints[4]);
				helperPolygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
				helperPolygonCollider2D.usedByComposite = true;
				helperPolygonCollider2D.points = new Vector2[] { polygonPoints[0], polygonPoints[1], polygonPoints[2], polygonPoints[3], polygonPoints[4] };
				polygonColliderList.Add(helperPolygonCollider2D);
				break;
		}
	}

	private void AddTriangle (Vector3 a, Vector3 b, Vector3 c) {
		int vertexIndex = vertices.Count;
		vertices.Add(a);
		vertices.Add(b);
		vertices.Add(c);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	private void AddQuad (Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
		int vertexIndex = vertices.Count;
		vertices.Add(a);
		vertices.Add(b);
		vertices.Add(c);
		vertices.Add(d);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
	}

	private void AddPentagon (Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e) {
		int vertexIndex = vertices.Count;
		vertices.Add(a);
		vertices.Add(b);
		vertices.Add(c);
		vertices.Add(d);
		vertices.Add(e);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 3);
		triangles.Add(vertexIndex + 4);
	}
	
	private void SetVoxelColors () {
		for (int i = 0; i < voxels.Length; i++) {
			voxelMaterials[i].color = voxels[i].state ? Color.black : Color.white;
		}
	}

	public void Apply (VoxelStencil stencil) {
		int xStart = stencil.XStart;
		if (xStart < 0) {
			xStart = 0;
		}
		int xEnd = stencil.XEnd;
		if (xEnd >= resolution) {
			xEnd = resolution - 1;
		}
		int yStart = stencil.YStart;
		if (yStart < 0) {
			yStart = 0;
		}
		int yEnd = stencil.YEnd;
		if (yEnd >= resolution) {
			yEnd = resolution - 1;
		}

		for (int y = yStart; y <= yEnd; y++) {
			int i = y * resolution + xStart;
			for (int x = xStart; x <= xEnd; x++, i++) {
				voxels[i].state = stencil.Apply(x, y, voxels[i].state);
			}
		}
		Refresh();
	}
}