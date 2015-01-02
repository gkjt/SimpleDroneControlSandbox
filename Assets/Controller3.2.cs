using UnityEngine;
using System.Collections;

public class Controller32 : MonoBehaviour {
	
	Vector3 desiredLoc = new Vector3(10f,10f,0);
	Vector3 Error, LastError;
	Quaternion LastErrorRotation;
	
	// Use this for initialization
	void Start () {
		Error = desiredLoc - transform.position;
		LastError = Error;
		QuadDynamics.setThrottle(10,10,10,10);
		PController2D();
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
		Error = desiredLoc - transform.position;
		Vector3 ErrorVelocity = (Error - LastError) / Time.deltaTime;
		Vector3 DesiredAcc = Error + ErrorVelocity;
		//Debug.DrawLine(transform.position,DesiredAcc,Color.magenta);
		
		Quaternion ErrorRotation = Quaternion.FromToRotation(transform.up, DesiredAcc);
		
		
		
		if(LastErrorRotation == null){
			LastErrorRotation = ErrorRotation;
		}
		
		
		int RollThrottleShift, PitchThrottleShift;
		RollThrottleShift = (int) (4*ErrorRotation.eulerAngles.z - transform.rotation.eulerAngles.z + 
			(ErrorRotation.eulerAngles.z-LastErrorRotation.eulerAngles.z));
		PitchThrottleShift = (int) (1*ErrorRotation.eulerAngles.x - transform.rotation.eulerAngles.x + 
			(ErrorRotation.eulerAngles.x-LastErrorRotation.eulerAngles.x));
		
		
		
		//RELATIVE THROTTLE SHIFTS TO HAND OFF TO LOW LEVEL MOTOR CONTROLLER
		int[] RelMotorThrottles = {0,0,0,0};
		
			Debug.Log(Error);
		if(Error.y > 0){
			RelMotorThrottles[0] += (int) ErrorRotation.z;
			RelMotorThrottles[2] += (int) ErrorRotation.z;
			
			RelMotorThrottles[0] += (int) ErrorRotation.x;
			RelMotorThrottles[1] += (int) ErrorRotation.x;
			
		}
		
		
		if(Error.y <= 0){
			RelMotorThrottles[1] -= (int) ErrorRotation.z;
			RelMotorThrottles[3] -= (int) ErrorRotation.z;
			
			RelMotorThrottles[2] -= (int) ErrorRotation.x;
			RelMotorThrottles[3] -= (int) ErrorRotation.x;
		}
		
		//Scale the shifts by how much it has been shifted
		int sumMotorThrottleShifts = 0;
		foreach(int i in RelMotorThrottles){
			sumMotorThrottleShifts += i;
		}
		for(int i = 0; i<4; i++){
			RelMotorThrottles[i] *= (int) ErrorVelocity.magnitude/sumMotorThrottleShifts; //May need a coefficient
		}
		
		
		
		
		
		int ThrottleShift = (int) (Error.y + ((Error.y-LastError.y)/Time.deltaTime));
		QuadDynamics.shiftThrottle(ThrottleShift);
		
		Debug.Log(ErrorRotation.eulerAngles);
		Debug.Log(RollThrottleShift);
		Debug.Log(PitchThrottleShift);
		
		
		
		LastError = Error;
		LastErrorRotation = ErrorRotation;
		//Error = desiredLoc - transform.position;
	}
	
}
