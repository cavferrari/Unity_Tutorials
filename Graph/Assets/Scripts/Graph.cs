using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public Transform pointPrefab;
    [Range(10, 100)]
	public int resolution = 10;
    Transform[] points;
    
    // Start is called before the first frame update
    void Start()
    {
		float step = 2f / resolution;
		points = new Transform[resolution];
		for (int i = 0; i < points.Length; i++) {
			Transform point = Instantiate(pointPrefab);
			point.localPosition = new Vector3((i + 0.5f) * step - 1f,
			                                  0f,
			                                  0f);
			point.localScale = Vector3.one * step;
			point.SetParent(transform, false);
			points[i] = point;
		}
		
    }

    // Update is called once per frame
    void Update()
    {
		for (int i = 0; i < points.Length; i++) {
			points[i].localPosition = new Vector3(points[i].localPosition.x, 
			                                      Mathf.Sin(Mathf.PI * (points[i].localPosition.x + Time.time)), 
			                                      points[i].localPosition.z);
		}
    }
}
