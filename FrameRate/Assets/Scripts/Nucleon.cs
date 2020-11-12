using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Nucleon : MonoBehaviour
{
	public float attractionForce;
	
	Rigidbody body;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void FixedUpdate () 
	{
		body.AddForce(transform.localPosition * -attractionForce);
	}        
	
	void Awake () 
	{
		body = GetComponent<Rigidbody>();
	}
	
}
