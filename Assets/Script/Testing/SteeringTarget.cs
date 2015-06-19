using UnityEngine;
using System.Collections;

public class SteeringTarget : MonoBehaviour {

    public float moveSpeed = 10;

    public Vector3 instantVelocity;


	// Use this for initialization
	void Start () {
        instantVelocity = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        var pos = transform.position;

        var horMovement = Input.GetAxis("Horizontal");
        var forwardMovement = Input.GetAxis("Vertical");

        if (horMovement != 0.0f)
            transform.Translate(transform.right * horMovement * Time.deltaTime * moveSpeed);
        if (forwardMovement != 0.0f)
            transform.Translate(transform.up * forwardMovement * Time.deltaTime * moveSpeed);

        instantVelocity = transform.position - pos;
	}
}
