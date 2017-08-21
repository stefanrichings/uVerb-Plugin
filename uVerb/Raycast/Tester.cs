using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour {
	 void Update () {
        CustRayHit hit;
        Debug.DrawRay(transform.position, -transform.up * 100);
        if (CustRaycast.Raycast(new Ray(transform.position, -transform.up), out hit))
        {
            Debug.Log("We have a hit!");
        }
        else
        {
            Debug.Log("No hit :(");
        }
    }
}
