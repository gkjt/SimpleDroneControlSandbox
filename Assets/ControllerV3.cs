using UnityEngine;
using System.Collections;

public class ControllerV3 : MonoBehaviour {
	
	Vector3 desiredLoc = new Vector3(2f,10f,0);
	Vector3 Error, LastError;
	Quaternion LastErrorRotation;
	
	// Use this for initialization
	void Start () {
		Error = desiredLoc - transform.position;
		LastError = Error;
		QuadDynamics.setThrottle(10,10,10,10);
		//PController2D();
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine(transform.position, desiredLoc);
		PController2D();
	}
	
	void PController1D(){
		QuadDynamics.shiftThrottle((int)(4*Error.y));
		Error = desiredLoc - transform.position;
	}
	
	void PDController1D(){
		Vector3 ErrorVelocity = (Error - LastError) / Time.deltaTime;
		
		QuadDynamics.shiftThrottle((int)(Error.y + ErrorVelocity.y));
		
		LastError = Error;
		Error = desiredLoc - transform.position;
		
	}
	
	void PController2D(){
		
		if(LastError == null){
			LastError = Error;
		}
		
		Vector3 ErrorVelocity = (Error - LastError) / Time.deltaTime;
		Vector3 DesiredAcc = Error + ErrorVelocity;
		
		Quaternion ErrorRotation = Quaternion.FromToRotation(transform.up, DesiredAcc);
		Debug.Log (ErrorRotation.eulerAngles);
		
		if(LastErrorRotation == null){
			LastErrorRotation = ErrorRotation;
		}
		
		
		float[] RelMotorThrottles = {0,0,0,0};
		float k = Time.deltaTime;
		
		float Proll, Ppitch, Droll, Dpitch;
		
		float RollError = (Mathf.Pow(ErrorRotation.eulerAngles.z, 2f) > Mathf.Pow(ErrorRotation.eulerAngles.z - 360, 2f) ? ErrorRotation.eulerAngles.z - 360 : ErrorRotation.eulerAngles.z);
		float PitchError = (Mathf.Pow(ErrorRotation.eulerAngles.x, 2f) > Mathf.Pow(ErrorRotation.eulerAngles.x - 360, 2f) ? ErrorRotation.eulerAngles.x - 360 : ErrorRotation.eulerAngles.x);
		
		Proll =  RollError;
		Ppitch =  PitchError;
		Droll = 1f * (ErrorRotation.eulerAngles.z - LastErrorRotation.eulerAngles.z) / k;
		Dpitch = 1f * (ErrorRotation.eulerAngles.x - LastErrorRotation.eulerAngles.x) / k;
		
		Debug.Log("Proportional Terms: Roll: " + Proll + " | Pitch: " + Ppitch); 
		
		//Rel Motor Throttles to provide rotation
		RelMotorThrottles[1] = Proll + Droll;
		RelMotorThrottles[3] = Proll+ Droll;
		RelMotorThrottles[0] = -1 * (Proll + Droll);
		RelMotorThrottles[2] = -1 * (Proll + Droll);
		/*
		RelMotorThrottles[0] += Ppitch;
		RelMotorThrottles[1] += Ppitch;
		RelMotorThrottles[2] += ( -1 * Ppitch);
		RelMotorThrottles[3] += ( -1 * Ppitch);
		*/
		
		float sum = 0;
		float highest = 0;
		foreach(float i in RelMotorThrottles){
			if(i > highest) highest = i;
			//PROBLEM: SUM MAY ALWAYS EQUAL ZERO
			sum += i;
		}
		
		//Set throttle x based on y error
		int throttleAveShift = (int) (0.5*Error.y + (5*(Error.y-LastError.y)/Time.deltaTime));
		QuadDynamics.shiftThrottleAve(throttleAveShift);
		//set motor throttles so average is throttle x and the motors are set to throttle * rel[i]/(sum/4)
		
		int[] motorThrottles = {0,0,0,0};
		
		for(int i = 0; i < 4; i++){
			if(highest*highest > 25)
				RelMotorThrottles[i] = 5 * (RelMotorThrottles[i] / highest);
			
			int motorThrottleConvert = (int)(QuadDynamics.ThrottleAve + RelMotorThrottles[i]);
			
			if(motorThrottleConvert > 100){
				motorThrottles[i] = 100;
			}
			else if(motorThrottleConvert < 0){
				motorThrottles[i] = 0;
			}
			else {
				motorThrottles[i] = motorThrottleConvert;
			}
			//Debug.Log ("Motor Throttle: " + motorThrottles[i] + " : Rel: " + RelMotorThrottles[i]);
		}
		
		QuadDynamics.throttles = motorThrottles;
		
		Debug.Log("Throttle Ave: " + QuadDynamics.ThrottleAve);
		
		LastError = Error;
		LastErrorRotation = ErrorRotation;
		Error = desiredLoc - transform.position;
	}
	
}
