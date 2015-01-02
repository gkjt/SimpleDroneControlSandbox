using UnityEngine;
using System.Collections;

public class Controller2 : MonoBehaviour {
	
	Vector3 DesiredPosition = new Vector3(10f, 0f, 10f);
	
	Vector3 Velocity = new Vector3(0f, 0f, 0f);
	Vector3 Acceleration = new Vector3(0f, 0f, 0f);
	Vector3 LastPosition = new Vector3(0f, 0f, 0f);
	Vector3 LastVelocity = new Vector3(0f, 0f, 0f);
	
	//Constants
	float Kdv = 1f; //Desired Speed
	

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Velocity = (transform.position - LastPosition) / Time.deltaTime;
		Acceleration = (Velocity - LastVelocity) / Time.deltaTime;
		
		Vector3 ErrorPosition = DesiredPosition - transform.position;
		ErrorPosition.Normalize();
		Vector3 DesiredVelocity = Kdv * ErrorPosition;
		
		Vector3 DesiredOrientation = DesiredVelocity - Velocity;
		Vector3 ErrorOrientation = DesiredOrientation - transform.up;
		var DesiredRotation = Quaternion.FromToRotation(Velocity, DesiredVelocity);
		var DesiredRotationEuler = DesiredRotation.eulerAngles;
		
		
		float DesiredRoll = DesiredRotationEuler.y;
		float ErrorRoll = DesiredRoll - transform.rotation.eulerAngles.y;
		
		if(ErrorRoll > 180)
			ErrorRoll -= 360;
		
		
		Debug.Log("Error Roll = " + ErrorRoll);
		Debug.DrawRay(transform.position, DesiredVelocity);
		
		
		
		
		
		LastPosition = transform.position;
		LastVelocity = Velocity;
	}
}
