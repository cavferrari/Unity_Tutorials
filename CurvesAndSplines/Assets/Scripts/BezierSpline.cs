using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BezierSpline : MonoBehaviour
{
	/* Although our spline is continuous, it sharply changes direction in between curve sections. 
	   These sudden changes in direction and speed are possible because the shared control point between two curves 
	   has two different velocities associated with it, one for each curve. If we want the velocities to be equal, 
	   we must ensure that the two control points that define them the third of the previous curve and the second of the next curve
	   mirror each other around the shared point. This ensures that the combined first and second derivatives are continuous.
	   Alternatively, we could align them but let their distance from the shared point differ. 
	   That will result in an abrubt change in velocity, while still keeping the direction continuous. 
	   In this case the combined first derivative is continuous, but the second is not.
	   The most flexible approach is to decide per curve boundary which contraints should apply, so we'll do that. 
	   Of course, once we have these constraints we can't just let anyone directly edit BezierSpline's points. 
	   So let's make our array private and provide indirect access to it. 
	   Make sure to let Unity know that we still want to serialize our points, otherwise they won't be saved.
	*/
	[SerializeField]
	private Vector3[] points;
	
	/* We only need to store the mode in between curves, so let's put them in an array with a length equal to the number of curves plus one. 
	   You'll need to reset your spline or create a new one to make sure you have an array of the right size.
	*/
	[SerializeField]
	private BezierControlPointMode[] modes;
	
	/* There is yet another constraint that we could add. By enforcing that the first and last control points share the same position, 
	   we can turn our spline into a loop. Of course, we also have to take modes into consideration as well. So let's add a loop property 
	   to BezierSpline. Whenever it is set to true, we make sure the modes of the end points match and we call SetPosition, 
	   trusting that it will take care of the position and mode constraints.
	*/
	[SerializeField]
	private bool loop;
	
	/* Let's add a method to BezierSpline to add another curve to the spline.
	   Because we want the spline to be continuous, the last point of the previous curve is the same as the first point of the next curve. 
	   So each extra curve adds three more points.
	   After that we can reduce t to just the fractional part to get the interpolation value for our curve.
	   To get to the actual points, we have to multiply the curve index by three.
	   However, this would fail when then original t equals one. In this case we can just set it to the last curve.
	   We now see direction lines across the entire spline, but we can improve the visualization by making sure that 
	   each curve segment gets the same amount of lines. Fortunately, it is easy to change BezierSplineInspector.ShowDirections 
	   so it uses BezierSpline.CurveCount to determine how many lines to draw.	   
	*/
	public Vector3 GetPoint (float t) 
	{
		int i;
		if (t >= 1f) 
		{
			t = 1f;
			i = points.Length - 4;
		}
		else 
		{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
	}
	
	public Vector3 GetVelocity (float t) 
	{
		int i;
		if (t >= 1f) 
		{
			t = 1f;
			i = points.Length - 4;
		}
		else 
		{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetFirstDerivative(	points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}
	
	public Vector3 GetDirection (float t)
	{
		return GetVelocity(t).normalized;
	}
	
	public void Reset ()
	{
		points = new Vector3[]
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
		modes = new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		};		
	}

	/* Let's add a method to BezierSpline to add another curve to the spline.
	   Because we want the spline to be continuous, the last point of the previous curve is the same as the first point of the next curve. 
	   So each extra curve adds three more points.
	*/
	public void AddCurve () 
	{
		Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		point.x += 1f;
		points[points.Length - 3] = point;
		point.x += 1f;
		points[points.Length - 2] = point;
		point.x += 1f;
		points[points.Length - 1] = point;
		
		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		/* To wrap things up, we should also make sure that the constraints are enforced when we add a curve. 
		   We can do this by simply calling EnforceMode at the point where the new curve was added. 
		*/
		EnforceMode(points.Length - 4);
		
		/* And finally, we also have to take looping into account when adding a curve to the spline. 
		   The result might be a tangle, but it will remain a proper loop. 
		*/
		if (loop) 
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}		
	}
	
	/* To cover the entire spline with a t going from zero to one, we first need to figure out which curve we're on. 
	   We can get the curve's index by multiplying t by the number of curves and then discarding the fraction. 
	   Let's add a CurveCount property to make that easy.
	*/
	public int CurveCount
	{
		get 
		{
			return (points.Length - 1) / 3;
		}
	}

	/* Now BezierSplineInspector must use the new methods and property instead of directly accessing the points array.
	*/
	public int ControlPointCount 
	{
		get 
		{
			return points.Length;
		}
	}

	public Vector3 GetControlPoint (int index) 
	{
		return points[index];
	}

	public void SetControlPoint (int index, Vector3 point) 
	{
		/* From now on, whenever you move a point or change a point's mode, the constraints will be enforced. 
		   But when moving a middle point, the previous point always stays fixed and the next point is always enforced. 
		   This might be fine, but it's intuitive if both other points move along with the middle one. 
		   So let's adjust SetControlPoint so it moves them together. 
		*/
		if (index % 3 == 0) 
		{
			Vector3 delta = point - points[index];
			// Next, SetControlPoint needs different edge cases when dealing with a loop, because it needs to wrap around the points array.
			if (loop) 
			{
				if (index == 0) 
				{
					points[1] += delta;
					points[points.Length - 2] += delta;
					points[points.Length - 1] = point;
				}
				else if (index == points.Length - 1) 
				{
					points[0] = point;
					points[1] += delta;
					points[index - 1] += delta;
				}
				else 
				{
					points[index - 1] += delta;
					points[index + 1] += delta;
				}
			}
			else 
			{
				if (index > 0) 
				{
					points[index - 1] += delta;
				}
				if (index + 1 < points.Length) 
				{
					points[index + 1] += delta;
				}
			}
		}
		points[index] = point;
		EnforceMode(index);
	}
	
	/* While we store the modes in between curves, it is convenient if we could get and set modes per control point. 
	   So we need to convert a point index into a mode index because in reality points share modes. 
	   As an example, the point index sequence 0, 1, 2, 3, 4, 5, 6 corresponds to the mode index sequence 0, 0, 1, 1, 1, 2, 2. 
	   So we need to add one and then divide by three. 
	   Now BezierSplineInspector can allow us to change the mode of the selected point. 
	   You will notice that changing the mode of one point also appears to change the mode of the points that are linked to it.
	*/
	public BezierControlPointMode GetControlPointMode (int index) 
	{
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode) 
	{
		/* To correctly enforce the loop, we need to make a few more changes to BezierSpline.
		   First, SetControlPointMode needs to make sure that the first and last mode remain equal in case of a loop.
		*/
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop) 
		{
			if (modeIndex == 0) 
			{
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1) 
			{
				modes[0] = mode;
			}
		}
		EnforceMode(index);
	}
	
	/* So far we're just coloring points. It's time to enforce the constraints. 
	   We add a new method to BezierSpline to do so and call it when a point is moved or a mode is changed. 
	   It takes a point index and begins by retrieving the relevant mode.
	*/
	private void EnforceMode (int index) 
	{
		int modeIndex = (index + 1) / 3;
		/* We should check if we actually don't have to enforce anything. This is the case when the mode is set to free, 
	   	   or when we're at the end points of the curve. In these cases, we can return without doing anything.
	    */
		BezierControlPointMode mode = modes[modeIndex];
		/* Next, EnforceMode can now only bail at the end points when not looping. 
		   It also has to check whether the fixed or enforced point wraps around the array. 
		*/
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1)) 
		{
			return;
		}
		/* Now which point should we adjust? When we change a point's mode, it is either a point in between curves or one of its neighbors. 
		   When we have the middle point selected, we can just keep the previous point fixed and enforce the constraints on the point on the opposite side.
		   If we have one of the other points selected, we should keep that one fixed and adjust its opposite. 
		   That way our selected point always stays where it is. So let's define the indices for these points.
		*/
		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex)
		{
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0) 
			{
				fixedIndex = points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Length)
			{
				enforcedIndex = 1;
			}
		}
		else 
		{
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Length) 
			{
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0)
			{
				enforcedIndex = points.Length - 2;
			}
		}
		/* Let's consider the mirrored case first. To mirror around the middle point, we have to take the vector from the middle to the fixed point
		   which is (fixed - middle) and invert it. This is the enforced tangent, and adding it to the middle gives us our enforced point.
		*/
		Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        /* For the aligned mode, we also have to make sure that the new tangent has the same length as the old one. 
           So we normalize it and then multiply by the distance between the middle and the old enforced point.
        */
        if (mode == BezierControlPointMode.Aligned) 
        {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}
		points[enforcedIndex] = middle + enforcedTangent;
	}
	
	public bool Loop 
	{
		get 
		{
			return loop;
		}
		set 
		{
			loop = value;
			if (value == true) 
			{
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0]);
			}
		}
	}	
}
