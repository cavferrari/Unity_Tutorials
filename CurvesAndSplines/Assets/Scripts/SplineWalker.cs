using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineWalker : MonoBehaviour
{
	public BezierSpline spline;
	public float duration;
	/* The walker now walks, but it's not looking in the direction that it's going. We can add an option for that.
	*/
	public bool lookForward;
	/* Another option is to keep looping the splines, instead of walking it just once.
	   While we're at it, we could also make the walker move back and forth, ping-ponging across the spline. 
	   Let's create an enumeration to select between these modes.
	*/
	public SplineWalkerMode mode;
	
	/* Now SplineWalker has to remember whether it's going forward or backward. 
	   It also needs to adjust the progress when passing the spline ends depending on its mode.
	*/
	private bool goingForward = true;
	private float progress;

	private void Update () 
	{
		if (goingForward) 
		{
			progress += Time.deltaTime / duration;
			if (progress > 1f) {
				if (mode == SplineWalkerMode.Once) 
				{
					progress = 1f;
				}
				else if (mode == SplineWalkerMode.Loop) 
				{
					progress -= 1f;
				}
				else 
				{
					progress = 2f - progress;
					goingForward = false;
				}
			}
		}
		else 
		{
			progress -= Time.deltaTime / duration;
			if (progress < 0f) 
			{
				progress = -progress;
				goingForward = true;
			}
		}

		Vector3 position = spline.GetPoint(progress);
		transform.localPosition = position;
		if (lookForward) 
		{
			transform.LookAt(position + spline.GetDirection(progress));
		}
	}
}
