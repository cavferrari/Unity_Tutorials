using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/* The inspector needs to extend UnityEditor.Editor. We also have to give it the UnityEditor.CustomEditor attribute. 
   This lets Unity know that it should use our class instead of the default editor for Line components.
*/
[CustomEditor(typeof(Line))]
public class LineInspector : Editor
{
	/*We need to add an OnSceneGUI method, which is a special Unity event method. We can use it to draw stuff in the scene view for our component.
	  The Editor class has a target variable, which is set to the object to be drawn when OnSceneGUI is called. We can cast this variable to a 
	  line and then use the Handles utility class to draw a line between our points.
	*/
	private void OnSceneGUI () 
	{
		Line line = target as Line;
		/* We now see the line, but it doesn't take its transform's settings into account. 
		   Moving, rotating, and scaling does not affect them at all. This is because 
		   Handles operates in world space while the points are in the local space of the line.
		   We have to explicitly convert the points into world space points.
		*/
		Transform handleTransform = line.transform;
		Vector3 p0 = handleTransform.TransformPoint(line.p0);
		Vector3 p1 = handleTransform.TransformPoint(line.p1);
		
		Handles.color = Color.white;
		Handles.DrawLine(p0, p1);
		
		/* Although we now get handles, they do not honor Unity's pivot rotation mode. 
		   Fortunately, we can use Tools.pivotRotation to determine the current mode and set our rotation accordingly.
		*/
		Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
		EditorGUI.BeginChangeCheck();
		/* Besides showing the line, we can also show position handles for our two points. 
		   To do this, we also need our transform's rotation so we can align them correctly.
		*/
		p0 = Handles.DoPositionHandle(p0, handleRotation);
		if (EditorGUI.EndChangeCheck()) 
		{
			/* There are two additional issues that need attention. First, we cannot undo the drag operations. 
			   This is fixed by adding a call to Undo.RecordObject before we make any changes. 
			   Second, Unity does not know that a change was made, so for example won't ask the user to save when quitting. 
			   This is remedied with a call to EditorUtility.SetDirty.
			 */
			Undo.RecordObject(line, "Move Point");
			EditorUtility.SetDirty(line);
			/* To make the handles actually work, we need to assign their results back to the line. 
			   However, as the handle values are in world space we need to convert them back into the 
			   line's local space with the InverseTransformPoint method. 
			   Also, we only need to do this when a point has changed. 
			   We can use EditorGUI.BeginChangeCheck and EditorGUI.EndChangeCheck for this. 
			   The second method tells us whether a change happened after calling the first method.
			*/
			line.p0 = handleTransform.InverseTransformPoint(p0);
		}
		EditorGUI.BeginChangeCheck();
		p1 = Handles.DoPositionHandle(p1, handleRotation);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(line, "Move Point");
			EditorUtility.SetDirty(line);
			line.p1 = handleTransform.InverseTransformPoint(p1);
		}
	}
}
