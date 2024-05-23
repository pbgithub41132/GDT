using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitBehaviour : MonoBehaviour
{
	//
	// Constants
	//
	const float TARGET_X      = -10;
	const float TARGET_Y      = 0;
	const float TARGET_Z      = 0;
	const float DEFAULT_SPEED = 0.025F;
    //
    // Private variables
    //
	private float   fSpeed;
    private Vector3 V3Target;
    private Vector3 V3Actor;
    //
    // Private functions
    //
	private void Start() {
        //
        // Init
        //	
		V3Target = new Vector3(TARGET_X, TARGET_Y, TARGET_Z);	
	}
    
    private void FixedUpdate() {
        //
        // Move fruit towards target
        //
        transform.position = Vector3.MoveTowards(transform.position, V3Target, DEFAULT_SPEED);
    }
}