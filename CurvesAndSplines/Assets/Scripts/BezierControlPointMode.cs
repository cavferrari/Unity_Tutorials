using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Let's define an enumeration type to describe our three modes.
   Now we can add these modes to BezierSpline. We only need to store the mode in between curves, 
   so let's put them in an array with a length equal to the number of curves plus one.
*/
public enum BezierControlPointMode 
{
	Free,
	Aligned,
	Mirrored
}