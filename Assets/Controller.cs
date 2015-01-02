using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	
	Vector3 desiredAcceleration = new Vector3(0,0,0);
	float desiredAltitude = 10f; 
	Vector3 desiredPos = new Vector3(1,1,1);
	
	float Kp = 5;
	float Ki;
	float Kd = 2;
	
	float lastError;
	Vector3 lastErrorVect;
	
	float output;
	Vector3 outputVect;
	// Use this for initialization
	
	void Start () {
		lastError = desiredAltitude;
		lastErrorVect = desiredPos;
	
	}
	
	// Update is called once per frame
	void Update () {
		altitudePDController();
		
		//positionPDController();
		ThreeDimensionalController();
		
	}
	
	void altitudePDController(){
		float error = desiredAltitude - transform.position.y;
		
		
		float acceleration = (error - lastError) / (Time.deltaTime);
		
		output = (error * Kp) + (acceleration * Kd);
		
		QuadDynamics.shiftThrottle((int)output);
		lastError = error;
		Debug.Log("Altitude PD Output = " + output);
	}
	
	void positionPDController(){
		Vector3 error = desiredPos - transform.position;
		
		
	}
	
	void ThreeDimensionalController(){
		Vector3 error = desiredPos - transform.position;
		
		//1,2 - z error
		//1,3/2,4 - x error
		//all - y error
		
		
		Vector3 velocity = (error - lastErrorVect) / (Time.deltaTime);
		
		int outputPitch = (int) (Mathf.Ceil(error.z * 0.005f) + Mathf.Ceil(velocity.z * 0.1f));
		int outputRoll = (int) (Mathf.Ceil(error.y * 0.005f) + Mathf.Ceil(velocity.y * 0.1f));
		
		if(transform.rotation.eulerAngles.y > 2){
			outputPitch *= -1;
			outputRoll *= -1;
		}
		
		QuadDynamics.shiftThrottles(outputPitch - outputRoll,outputPitch + outputRoll, -1*outputRoll, outputRoll);
		
		Vector3 acceleration = (error - lastErrorVect) / (Time.deltaTime);
		
		outputVect = (error * Kp) + (acceleration * Kd);
		
		
		
		lastErrorVect = error;
		Debug.Log("Altitude PD Output = " + output);
		
	}
	
	
}
