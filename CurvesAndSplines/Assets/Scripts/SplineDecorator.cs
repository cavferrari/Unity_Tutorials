using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineDecorator : MonoBehaviour
{

	public BezierSpline spline;
	public int frequency;
	public bool lookForward;
	public Transform[] items;

	private void Awake () 
	{
		if (frequency <= 0 || items == null || items.Length == 0) 
		{
			return;
		}
		/* This works well for loops, but it doesn't go all the way to the end of splines that aren't loops.
		   We can fix this by increasing our step size to cover the entire length of the spline, 
		   as long as it's not a loop and we have more than one item to place.
		*/
		float stepSize = frequency * items.Length;
		if (spline.Loop || stepSize == 1)
		{
			stepSize = 1f / stepSize;
		}
		else 
		{
			stepSize = 1f / (stepSize - 1);
		}
		for (int p = 0, f = 0; f < frequency; f++) 
		{
			for (int i = 0; i < items.Length; i++, p++) 
			{
				Transform item = Instantiate(items[i]) as Transform;
				Vector3 position = spline.GetPoint(p * stepSize);
				item.transform.localPosition = position;
				if (lookForward) 
				{
					item.transform.LookAt(position + spline.GetDirection(p * stepSize));
				}
				item.transform.parent = transform;
			}
		}
	}
}
