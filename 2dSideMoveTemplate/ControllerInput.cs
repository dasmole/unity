using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerInput : MonoBehaviour {
    float directionInputX;
    public Controller2D controller2D;
    bool jumping;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        directionInputX = Input.GetAxisRaw("Horizontal");
        jumping = Input.GetKeyDown("space");
        if (jumping)
        {
            Debug.Log("Space is down");
        }
        controller2D.movement(directionInputX, jumping);
    }
}
