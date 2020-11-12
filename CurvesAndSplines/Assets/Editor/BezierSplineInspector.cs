using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor
{
	private const int stepsPerCurve = 10;
	private const float directionScale = 0.5f;
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;
	private int selectedIndex = -1;
	private BezierSpline spline;
	private Transform handleTransform;
	private Quaternion handleRotation;
	private static Color[] modeColors = {Color.white,
			                             Color.yellow,
			                             Color.cyan};
	
	private void OnSceneGUI () 
	{
		spline = target as BezierSpline;
		handleTransform = spline.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
		
		Vector3 p0 = ShowPoint(0);
		/* Of course we still only see the first curve. So we adjust BezierSplineInspector so it loops over all the curves.
		   Now we can see all the curves, but the direction lines are only added to the first one. 
		   This is because BezierSpline's method also still only work with the first curve. It's time to change that.
		*/
		for (int i = 1; i < spline.ControlPointCount; i += 3) 
		{
			Vector3 p1 = ShowPoint(i);
			Vector3 p2 = ShowPoint(i + 1);
			Vector3 p3 = ShowPoint(i + 2);
			
			Handles.color = Color.gray;
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p2, p3);
			
			Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
			p0 = p3;
		}
		ShowDirections();
	}

	private void DrawSelectedPointInspector() 
	{
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck()) 
		{
			Undo.RecordObject(spline, "Move Point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPoint(selectedIndex, point);
		}
		/* Now BezierSplineInspector can allow us to change the mode of the selected point. 
		   You will notice that changing the mode of one point also appears to change the mode of the points that are linked to it.
		*/
		EditorGUI.BeginChangeCheck();
		BezierControlPointMode mode = (BezierControlPointMode)
		EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
		if (EditorGUI.EndChangeCheck()) 
		{
			Undo.RecordObject(spline, "Change Point Mode");
			spline.SetControlPointMode(selectedIndex, mode);
			EditorUtility.SetDirty(spline);
		}		
	}

	/* To actually be able to add a curve, we have to add a button to our spline's inspector. 
	   We can customize the inspector that Unity uses for our component by overriding the OnInspectorGUI method of BezierSplineInspector. 
	   Note that this is not a special Unity method, it relies on inheritance.
	   To keep drawing the default inspector, we call the DrawDefaultInspector method. 
	   Then we use GUILayout to draw a button, which when clicked adds a curve.
	*/
	public override void OnInspectorGUI () 
	{
		spline = target as BezierSpline;
		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
		if (EditorGUI.EndChangeCheck()) 
		{
			Undo.RecordObject(spline, "Toggle Loop");
			EditorUtility.SetDirty(spline);
			spline.Loop = loop;
		}		
		/* While we're at it, we also no longer want to allow direct access to the array in the inspector, 
		   so remove the call to DrawDefaultInspector. To still allow changes via typing, let's show a vector field for the selected point.
		*/
		if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount) 
		{
			DrawSelectedPointInspector();
		}
		if (GUILayout.Button("Add Curve")) 
		{
			Undo.RecordObject(spline, "Add Curve");
			spline.AddCurve();
			EditorUtility.SetDirty(spline);
		}
	}

	private void ShowDirections () 
	{
		Handles.color = Color.green;
		Vector3 point = spline.GetPoint(0f);
		Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);
		int steps = stepsPerCurve * spline.CurveCount;
		for (int i = 1; i <= steps; i++) 
		{
			point = spline.GetPoint(i / (float)steps);
			Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
		}
	}

	/* It's rather crowded with all those transform handles. We could only show a handle for the active point. 
	   Then then other points can suffice with dots. Let's update ShowPoint so it shows a button instead of a position handle. 
	   This button will look like a white dot, which when clicked will turn into the active point. 
	   Then we only show the position handle if the point's index matches the selected index, which we initialize at -1 so nothing is selected by default.
	*/
	private Vector3 ShowPoint (int index) 
	{
		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
		/* This works, but it is tough to get a good size for the dots. Depending on the scale you're working at, 
		   they could end up either too large or too small. It would be nice if we could keep the screen size of the dots fixed, 
		   just like the position handles always have the same screen size. We can do this by factoring in HandleUtility.GetHandleSize. 
		   This method gives us a fixed screen size for any point in world space.
		*/
		float size = HandleUtility.GetHandleSize(point);
		
		/* It is great that we have loops, but it is inconvenient that we can no longer see where the spline begins. 
		   We can make this obvious by letting BezierSplineInspector always double the size of the dot for the first point.
		   Note that in case of a loop the last point will be drawn on top of it, 
		   so if you clicked the middle of the big dot you'd select the last point, while if you clicked further from the center you'd get the first point.
		*/
		if (index == 0) 
		{
			size *= 2f;
		}
		Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) 
		{
			selectedIndex = index;
			/* Unfortunately, it turns out that the inspector doesn't refresh itself when we select a point in the scene view. 
			   We could fix this by calling SetDirty for the spline, but that's not right because the spline didn't change. 
			   Fortunately, we can issue a repaint request instead.
			*/
			Repaint();
		}
		if (selectedIndex == index)
		{
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) 
			{
				Undo.RecordObject(spline, "Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}
}
