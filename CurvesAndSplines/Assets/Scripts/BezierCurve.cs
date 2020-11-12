using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
	public Vector3[] points;

	public void Reset () 
	{
		points = new Vector3[] 
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
	}
   
   	public Vector3 GetPoint (float t) 
   	{
   		return transform.TransformPoint(Bezier.GetPoint(points[0], points[1], points[2], points[3], t));
	}

	/* Because it produces a velocity vector and not a point, it should not be affected by the position of the curve, so we subtract that after transforming.
	   Now we can visualize the speed along the curve in BezierCurveInspector's OnSceneGUI method.
	*/
	public Vector3 GetVelocity (float t) 
	{
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[0], points[1], points[2], points[3], t)) - transform.position;
	}

	public Vector3 GetDirection (float t) 
	{
		return GetVelocity(t).normalized;
	}	
}
