using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public Transform hoursTransform;
    public Transform minutesTransform;
    public Transform secondsTransform;
    public bool continuous;
    
    public const float degreesPerHour = 30f;
    public const float degreesPerMinute = 6f;
    public const float degreesPerSecond = 6f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update () 
    {
        if (continuous) 
        {
	    UpdateContinuous();
	}
	else 
	{
	    UpdateDiscrete();
	}
    }    

    // Update is called once per frame
    void UpdateContinuous()
    {
    	TimeSpan time = DateTime.Now.TimeOfDay;
        hoursTransform.localRotation = Quaternion.Euler(0f, (float)time.TotalHours * degreesPerHour, 0f);
	minutesTransform.localRotation = Quaternion.Euler(0f, (float)time.TotalMinutes * degreesPerMinute, 0f);
	secondsTransform.localRotation = Quaternion.Euler(0f, (float)time.TotalSeconds * degreesPerSecond, 0f);
    }
    
    void UpdateDiscrete()
    {
    	DateTime time = DateTime.Now;
        hoursTransform.localRotation = Quaternion.Euler(0f, time.Hour * degreesPerHour, 0f);
	minutesTransform.localRotation = Quaternion.Euler(0f, time.Minute * degreesPerMinute, 0f);
	secondsTransform.localRotation = Quaternion.Euler(0f, time.Second * degreesPerSecond, 0f);
    }    
}
