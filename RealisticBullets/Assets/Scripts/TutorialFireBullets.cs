using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFireBullets : MonoBehaviour
{
	public GameObject bulletObj;
    public Transform bulletParent;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FireBullet());
    }

	public IEnumerator FireBullet() 
    {        
        while (true)
        {
            //Create a new bullet
            GameObject newBullet = Instantiate(bulletObj, transform.position, transform.rotation) as GameObject;

            //Parent it to get a less messy workspace
            newBullet.transform.parent = bulletParent;

            //Add velocity to the bullet with a rigidbody
            newBullet.GetComponent<Rigidbody>().velocity = TutorialBallistics.bulletSpeed * transform.forward;

            yield return new WaitForSeconds(2f);
        }
    }
}
