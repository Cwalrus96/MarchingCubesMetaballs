using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metaball : MonoBehaviour
{
	

	public float maxSquaredRadius; // This represents the maximum distance that the metaball can effect
	Vector3 prevPosition;
	float prevRadius; 
	public bool hasChanged {
		get
		{
			return (prevPosition != transform.position || prevRadius != maxSquaredRadius);  
		} }

	// Start is called before the first frame update
	void Start()
    {
		prevPosition = transform.position;
		prevRadius = maxSquaredRadius; 
	
	}

	public void updateValues()
	{
		prevPosition = transform.position;
		prevRadius = maxSquaredRadius; 
	}

	public float getValueAtPoint(Vector3 point)
	{
		//Value is maxSquaredRadius - squaredDistance. When distance == maxSquaredRadius value will be 0 (only need to check inside the cube)
		Vector3 position = transform.position;
		float xDist = position.x - point.x;
		xDist = xDist * xDist;
		float yDist = position.y - point.y;
		yDist = yDist * yDist;
		float zDist = position.z - point.z;
		zDist = zDist * zDist;
		return maxSquaredRadius / (xDist + yDist + zDist); 
	}
}
