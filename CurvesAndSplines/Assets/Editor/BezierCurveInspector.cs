using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveInspector : Editor
{
	private BezierCurve curve;
	private Transform handleTransform;
	private Quaternion handleRotation;
	private const int lineSteps = 10;
	private const float directionScale = 0.5f;
	
	/* It is probably visually obvious by now that we draw our curve using straight line segments. 
	   We could increase the number of steps to improve the visual quality. We could also use an iterative approach to get accurate down to pixel level. 
	   But we can also use Unity's Handles.DrawBezier method, which takes care of drawing nice cubic Beziér curves for us.
	*/
	private void OnSceneGUI () 
	{
		curve = target as BezierCurve;
		handleTransform = curve.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
		Vector3 p0 = ShowPoint(0);
		Vector3 p1 = ShowPoint(1);
		Vector3 p2 = ShowPoint(2);
		Vector3 p3 = ShowPoint(3);
		Handles.color = Color.grey;
		Handles.DrawLine(p0, p1);
		Handles.DrawLine(p1, p2);
		Handles.DrawLine(p2, p3);
		ShowDirections();
		/* The method is a bit weird in that its parameter list begins with the end points, followed by the two intermediate points. 
		   The middle points are named tangents, but they are expected to be actual control points and not direction vectors.
		   The color argument is obvious, but it also expects a texture and a width. 
		   The width is in pixels and should be 2 if you want an anti-aliased look. The texture also needs to be of a specific form 
		   to allow anti-aliasing, though the default works fine and I always supply null.
		*/
		Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
	}

	/* We can clearly see how the velocity changes along the curve, but those long lines are cluttering the view.
	   Instead of showing the velocity, we can suffice with showing the direction of movement. Which requires that 
	   we add GetDirection to BezierCurve, which simply normalizes the velocity.
	   Let's also show the directions in their own method and scale them to take up less space.
	*/
	private void ShowDirections () 
	{
		Handles.color = Color.green;
		Vector3 point = curve.GetPoint(0f);
		Handles.DrawLine(point, point + curve.GetDirection(0f) * directionScale);
		for (int i = 1; i <= lineSteps; i++) 
		{
			point = curve.GetPoint(i / (float)lineSteps);
			Handles.DrawLine(point, point + curve.GetDirection(i / (float)lineSteps) * directionScale);
		}
    }

	private Vector3 ShowPoint (int index) 
	{
		Vector3 point = handleTransform.TransformPoint(curve.points[index]);
		EditorGUI.BeginChangeCheck();
		point = Handles.DoPositionHandle(point, handleRotation);
		if (EditorGUI.EndChangeCheck()) 
		{
			Undo.RecordObject(curve, "Move Point");
			EditorUtility.SetDirty(curve);
			curve.points[index] = handleTransform.InverseTransformPoint(point);
		}
		return point;
	}
}
