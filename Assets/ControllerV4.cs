using UnityEngine;
using System.Collections;

public class ControllerV4 : MonoBehaviour {
	
	Vector3 desiredLoc = new Vector3(10f,10f,10f);
	Vector3 Error, LastError;
	Vector3 Velocity, ErrorVelocity, VelocityError, LastPosition; //ErrorVelocity = Change in Error, VelocityError = DesiredVelocity - Velocity
	Quaternion LastErrorRotation;
	
	float theta;
	
	// Use this for initialization
	void Start () {
		Error = desiredLoc - transform.position;
		LastError = Error;
		Velocity = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		SetVariables();
		DrawDebugRays();
		calcV();
		calcRotation();
		calcVertThrottle();
		applyRotation();
	}
	
	void FixedUpdate(){
		
		
	}
	
	void SetVariables(){
		LastError = Error;
		Error = desiredLoc - transform.position;
		ErrorVelocity = (Error - LastError) / Time.deltaTime;
		Velocity = transform.position - LastPosition;
		LastPosition = transform.position;
	}
	
	void DrawDebugRays(){
		Debug.DrawRay(transform.position, Error, Color.magenta);
		Debug.DrawRay(transform.position, Velocity, Color.blue);
	}
	
	void calcV(){
		//Calculate the multiplier for the magnitude of the desired velocity
		Vector3 DesiredVelocity = Error.normalized;
		VelocityError = DesiredVelocity - Velocity.normalized;
		//The multiplier is inversely proportional to the magnitude of the 
		//of the error ofthe velocity and proportional to the error in location
		float MagMultiplier = Error.magnitude/(VelocityError.magnitude < 1f ? 1 : VelocityError.magnitude);
		DesiredVelocity *= MagMultiplier;
		
		//VelocityError is the DesiredAcceleration
		VelocityError = DesiredVelocity - Velocity;
		Debug.Log("Velocity Error: " + VelocityError);
	}
	
	
	void calcRotation() {
		//Calculate rotation based horizontal plane error, scaled by ratio with vertical error
		
		//Copy VelocityError into worker variable
		Vector3 HorizontalError = VelocityError;
		//Scale y component to 0
		HorizontalError.Scale(new Vector3(1,0,1));
		Debug.DrawRay(transform.position, HorizontalError);
		//Calculate theta
		if(HorizontalError == Vector3.zero){
			theta = 0;
		}
		else if (HorizontalError.z == 0){
			theta = Mathf.Sign(HorizontalError.x) * 90;
		}
		else if(HorizontalError.x == 0){
			theta = (HorizontalError.z > 0 ? 0 : 180);
		}
		else{
			theta = Mathf.Atan(HorizontalError.x / HorizontalError.z);
		}
		//Calculations are nicer when taking the forward line as through two diagonally opposite motors
		theta -= 45 * (Mathf.PI / 180);
		float angle = 20 * (Mathf.PI / 180) * HorizontalError.magnitude / VelocityError.y;
		Debug.Log ("Angle: " + angle + "\tTheta: " + theta);
		
		//Now convert to Quaternion/Euler Angles
		//Corrosponds to a rotation z-x-y by 0-angle-(theta+45) for vector (0,0,1)
		//	or 0-(90-angle)-theta for (0,1,0)
		Quaternion DesiredRotation = new Quaternion();
		
		DesiredRotation.eulerAngles.Set(angle, theta + 45 * (Mathf.PI / 180), 0);
		Debug.DrawRay (transform.position, DesiredRotation * Vector3.up, Color.blue);
		Debug.Log("DesiredRotation: " + DesiredRotation);
	}
	
	void calcVertThrottle(){
		//P:
		float kP = 1.0f, kD = 1.0f;
		int ThrottleShift = (int) (kP * Error.y + kD * ErrorVelocity.y);
		
		QuadDynamics.shiftThrottleAve(ThrottleShift);
		QuadDynamics.resetThrottlesToAve();
	}
	
	void applyRotation(){
		float kP = 0.5f;
		int[] throttleshifts = new int[4];
		throttleshifts[0] += (int) (-kP * QuadDynamics.throttles[0] / 4 * Mathf.Sin (theta));
		throttleshifts[1] += (int) (-kP * QuadDynamics.throttles[0] / 4 * Mathf.Cos (theta));
		throttleshifts[2] += (int) ( kP * QuadDynamics.throttles[0] / 4 * Mathf.Cos (theta));
		throttleshifts[3] += (int) ( kP * QuadDynamics.throttles[0] / 4 * Mathf.Sin (theta));
		QuadDynamics.shiftThrottles(throttleshifts);
		Debug.Log (string.Format("Throttle Shifts: {0}/{1}/{2}/{3}", throttleshifts[0], throttleshifts[1],
			throttleshifts[2], throttleshifts[3]));
	}
}
