using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MetaballMeshBuilder : MonoBehaviour
{
	public List<Metaball> metaballs = new List<Metaball>();
	public float cubeLength;
	float prevCubeLength;
	public float minValue = 1;
	float prevMinValue; 
	public Mesh mesh;
	List<Vector3> vertices;
	List<int> triangles;
	public MARCHING_MODE mode = MARCHING_MODE.CUBES;
	Marching marching;

	/** Step 1. Have a single mesh attached to the MeshBuilder, rather than the individual metaballs
	 *	Step 2. When generating mesh, determine the furthest extent occupied by any metaball in the list
	 *	Step 3. Loop once through the entire box that contains all metaballs in the list
	 *	**/

	private void Awake()
	{
		mesh = GetComponent<MeshFilter>().mesh;
		vertices = new List<Vector3>();
		triangles = new List<int>(); 
	}

	// Start is called before the first frame update
	void Start()
    {
		if (mode == MARCHING_MODE.TETRAHEDRON)
		{
			marching = new MarchingTertrahedron();
		}
		else
		{
			marching = new MarchingCubes();
		}
		marching.Surface = minValue;
		prevCubeLength = cubeLength;
		prevMinValue = minValue; 
		
		generateMesh(); 
    }

    // Update is called once per frame
    void Update()
    {
		bool hasChanged = false;
		foreach (Metaball m in metaballs)
		{
			if(m.hasChanged)
			{
				m.updateValues();
				hasChanged = true; 
			}
		}
		if (prevCubeLength != cubeLength || prevMinValue != minValue) hasChanged = true; 
		if(hasChanged)
		{
			marching.Surface = minValue; 
			generateMesh(); 
		}
		prevCubeLength = cubeLength;
		prevMinValue = minValue; 
    }


	/**This function should generate the mesh that surrounds all of the metaballs. 
	 * Although there may be many metaballs, it will technically only generate a single mesh. **/
	 
	void generateMesh()
	{
		if (metaballs.Count <= 0) return;
		//STEP 1. Iterate through the cubic region surrounding each metaball.
		//Determine the largest size of the region to explore. Need to locate the largest and smallest X, Y and Z values. 
		vertices.Clear();
		triangles.Clear();
		float minX, maxX, minY, maxY, minZ, maxZ;
		minX = maxX = minY = maxY = minZ = maxZ = 0;
		
		for (int i = 0; i < metaballs.Count; i++)
		{
			Metaball m = metaballs[i];
			Vector3 position = m.transform.position;
			float r = m.maxSquaredRadius; 
			if((i == 0) || (position.x - r < minX))
			{
				minX = position.x - r; 
			}
			if ((i == 0) || (position.x + r > maxX))
			{
				maxX = position.x + r; 
			}
			if ((i == 0) || (position.y - r < minY))
			{
				minY = position.y - r;
			}
			if ((i == 0) || (position.y + r > maxY))
			{
				maxY = position.y + r;
			}
			if ((i == 0) || (position.z - r < minZ))
			{
				minZ = position.z - r;
			}
			if ((i == 0) || (position.z + r > maxZ))
			{
				maxZ = position.z + r;
			}
		}
		int xResolution = Mathf.CeilToInt((maxX - minX) / cubeLength);
		int yResolution = Mathf.CeilToInt((maxY - minY) / cubeLength);
		int zResolution = Mathf.CeilToInt((maxZ - minZ) / cubeLength);
		Vector3 corner = new Vector3(minX, minY, minZ); 
		for (int x = 0; x < xResolution; x++)
		{
			for(int y = 0; y < yResolution; y++)
			{
				for(int z = 0; z < zResolution; z++)
				{
					//Start in left, bottom, back corner, slowly work your way to the right, top, front corner. 
					//For each cube you will examine 8 points, and determine the value at that point
					//Each cube is going to need 8 vertices - Order is strange because it needs to fit with the Marching Cubes library
					//1. Left Bottom Back
					int i = (x * xResolution * xResolution) + (y * yResolution) + z;  
					float[] cube = new float[8];
					cube[0] = getValueAtPoint(new Vector3(corner.x + (cubeLength * x), corner.y + (cubeLength * y), corner.z + (cubeLength * z)));
					//2. Left Bottom Front
					cube[4] = getValueAtPoint(new Vector3(corner.x + (cubeLength * x), corner.y + (cubeLength * y), corner.z + (cubeLength * (z + 1))));
					//3. Left Top Back
					cube[3] = getValueAtPoint(new Vector3(corner.x + (cubeLength * x), corner.y + (cubeLength * (y + 1)), corner.z + (cubeLength * z)));
					//4. Left Top Front
					cube[7] = getValueAtPoint(new Vector3(corner.x + (cubeLength * x), corner.y + (cubeLength * (y + 1)), corner.z + (cubeLength * (z + 1))));
					//5. Right Bottom Back
					cube[1] = getValueAtPoint(new Vector3(corner.x + (cubeLength * (x + 1)), corner.y + (cubeLength * y), corner.z + (cubeLength * z)));
					//6. Right Bottom Front
					cube[5] = getValueAtPoint(new Vector3(corner.x + (cubeLength * (x + 1)), corner.y + (cubeLength * y), corner.z + (cubeLength * (z + 1))));
					//7. Right Top Back
					cube[2] = getValueAtPoint(new Vector3(corner.x + (cubeLength * (x + 1)), corner.y + (cubeLength * (y + 1)), corner.z + (cubeLength * z)));
					//8. Right Top Front
					cube[6] = getValueAtPoint(new Vector3(corner.x + (cubeLength * (x + 1)), corner.y + (cubeLength * (y + 1)), corner.z + (cubeLength * (z + 1))));
					marching.March(corner.x + (cubeLength * x), corner.y + (cubeLength * y), corner.z + (cubeLength * z), cube, cubeLength, vertices, triangles);
				}
			}
		}
		mesh.Clear();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals(); 
	}

	private float getValueAtPoint(Vector3 point)
	{
		float value = 0.0f;
		float temp = 0.0f; 
		foreach(Metaball m in metaballs)
		{
			temp = m.getValueAtPoint(point);
			if (temp < 0.0f) temp = 0.0f;
			value += temp; 
		}
		return value; 
	}
}
