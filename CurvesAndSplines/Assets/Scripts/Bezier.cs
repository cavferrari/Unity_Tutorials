using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier : MonoBehaviour
{
	/* This kind of curve is known as a quadratic Beziér curve, because of the polynomial math involved.
	  The linear curve can be written as B(t) = (1 - t) P0 + t P1. One step deeper you get B(t) = (1 - t) ((1 - t) P0 + t P1) + t ((1 - t) P1 + t P2). 
	  This is really just the linear curve with P0 and P1 replaced by two new linear curves. It can also be rewritten into the more compact form 
	  B(t) = (1 - t)2 P0 + 2 (1 - t) t P1 + t2 P2.
	  Let's go a step further and add new methods to Bezier for cubic curves as well! It works just like the quadratic version, 
	  except that it needs a fourth point and its formula goes another step deeper, resulting in a combination of six linear interpolations. 
	  The consolidated function of that becomes B(t) = (1 - t)3 P0 + 3 (1 - t)2 t P1 + 3 (1 - t) t2 P2 + t3 P3 
	  which has as its first derivative B'(t) = 3 (1 - t)2 (P1 - P0) + 6 (1 - t) t (P2 - P1) + 3 t2 (P3 - P2).
	  With that, we can upgrade BezierCurve from quadratic to cubic by taking an additional point into consideration. 
	  Be sure to add the fourth point to its array either manually or by resetting the component. 
	  BezierCurveInspector now needs to be updated so it shows the fourth point as well.
	*/
	public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) 
	{
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return oneMinusT * oneMinusT * oneMinusT * p0 +	3f * oneMinusT * oneMinusT * t * p1 + 3f * oneMinusT * t * t * p2 +	t * t * t * p3;
	}
	
	/* Now that we have a polynomial function, we can also describe its derivatives. 
	   The first derivative of our quadratic Beziér curve is B'(t) = 2 (1 - t) (P1 - P0) + 2 t (P2 - P1). Let's add it.
	   A derivative of a function measures its rate of change, and is a function itself as well.
	   For example, the function f(t) = 3 is constant, so its derivative is f'(t) = 0.
	   Another example, f(t) = t is linear, so its rate of change is constant f'(t) = 1. Compare this with f(t) = 2 t, which has derivative f'(t) = 2.
	   Jumping to a quadratic function, f(t) = t2 has a linear derivative, f'(t) = 2 t, which means it keeps growing faster.
	   Combinations work too. f(t) = t2 + 3 t + 4 has derivative f'(t) = 2 t + 3 + 0.
	   In general, tn becomes n t(n - 1) as long as n is larger than zero. 
	   There are more complex rules as well, but you don't need those to deal with derivatives of Beziér curves.
	   So how do we get the first derivative of B(t) = (1 - t)2 P0 + 2 (1 - t) t P1 + t2 P2?
	   Note that (1 - t)2 rewrites to t2 - 2 t + 1, which has derivative 2 t - 2. And 2 (1 - t) t rewrites to 2 t - 2 t2, which has derivative 2 - 4 t.
	   So we end up with B'(t) = (2 t - 2) P0 + (2 - 4 t) P1 + 2 t P2.
	   Then we rewrite it somewhat, turning the P1 part into -(2 t - 2) P1 - 2 t P1. 
	   This allows us to combine it with the P0 and P2 parts so we get B'(t) = (2 t - 2) (P0 - P1) + 2 t (P2 - P1).
	   As a last step we invert the first term and extract 2 so we get the nice B'(t) = 2 (1 - t) (P1 - P0) + 2 t (P2 - P1).
	   This function produces lines tangent to the curve, which can be interpreted as the speed with which we move along the curve. So now we can add a
	   GetVelocity method to BezierCurve.
	*/
	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) 
	{
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return 3f * oneMinusT * oneMinusT * (p1 - p0) +	6f * oneMinusT * t * (p2 - p1) + 3f * t * t * (p3 - p2);
	}
}
