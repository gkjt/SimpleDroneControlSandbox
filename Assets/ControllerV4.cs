using UnityEngine;
using System.Collections;

public class ControllerV4 : MonoBehaviour {
	
	Vector3 desiredLoc = new Vector3(10f,10f,10f);
	Vector3 Error, LastError;
	Vector3 Velocity, ErrorVelocity, VelocityError, LastPosition; //ErrorVelocity = Rate of Change in Error, VelocityError = DesiredVelocity - Velocity
	Quaternion LastErrorRotation;
	
	float theta, angle, ErrorTheta, ErrorAngle;
	
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
		Debug.DrawRay(transform.position, transform.forward, Color.red);
		Debug.DrawRay(transform.position, transform.up / 2, Color.red);
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
		//(rads)
		theta -= 45 * (Mathf.PI / 180);
		float angle = 10 * (Mathf.PI / 180) * HorizontalError.magnitude / VelocityError.y;
		Debug.Log ("Angle: " + angle + "\tTheta: " + theta);
		
		//Now convert to Quaternion/Euler Angles (degrees)
		//Corrosponds to a rotation z-x-y by 0-angle-(theta+45) for unit vector (0,0,1)
		//	or 0-(90-angle)-theta for (0,1,0)
		Quaternion DesiredRotation = Quaternion.Euler(angle * (180 / Mathf.PI) , theta * (180 / Mathf.PI)  + 45, 0f);
		
		Debug.DrawRay(transform.position, DesiredRotation * Vector3.up, Color.blue);
		//Debug.Log("DesiredRotation: " + DesiredRotation);
		
		//From http://answers.unity3d.com/questions/18438/rotation-relative-to-a-transform
		//Quaternion rotationDelta = Quaternion.FromToRotation(modelA.transform.forward, modelB.transform.forward);
		Quaternion ErrorRotation = Quaternion.FromToRotation(transform.rotation * Vector3.up,
			DesiredRotation * Vector3.up);
		Debug.DrawRay(transform.position, ErrorRotation * (0.5f * transform.up), Color.cyan);
		
		//Now convert back to Theta-Angle format (degrees)
		ThetaAngleFromQuaternion(ErrorRotation, out ErrorTheta, out ErrorAngle);
		ErrorTheta -= 45;
		Debug.Log ("ErrorAngle: " + ErrorAngle + "\tErrorTheta: " + ErrorTheta);
		
		//convert to rads
		ErrorTheta *= (Mathf.PI / 180);
		ErrorAngle *= (Mathf.PI / 180);
		
	}
	
	void ThetaAngleFromQuaternion(Quaternion quat, out float theta, out float angle){
		Vector3 vect = quat * Vector3.up;
		ThetaAngleFromVector3(vect, out theta, out angle);
		Debug.Log (quat.eulerAngles);
		
	}
	
	void ThetaAngleFromVector3(Vector3 vect, out float theta, out float angle){
		vect.Normalize();
		//Copy VelocityError into worker variable
		Vector3 horizVect = vect;
		//Scale y component to 0
		horizVect.Scale(new Vector3(1,0,1));
		
		angle = Vector3.Angle(Vector3.up, vect);
		
		if(horizVect == Vector3.zero){
			theta = 0;
		}else{
			theta = Mathf.Sign(horizVect.x) * Vector3.Angle(Vector3.forward, horizVect);
		}
		
		Debug.Log ("From V3: A: " + angle + "\tT: " + theta);
		Debug.DrawRay(transform.position, horizVect, Color.cyan);
	}
	
	void calcVertThrottle(){
		//PD:
		float kP = 1.0f, kD = 3.0f;
		int ThrottleShift = (int) (kP * Error.y + kD * ErrorVelocity.y);
		
		QuadDynamics.shiftThrottleAve(ThrottleShift);
		QuadDynamics.resetThrottlesToAve();
	}
	
	void applyRotation(){
		float kP = 0.01f;
		int[] throttleshifts = new int[4];
		throttleshifts[0] += (int) (-kP * QuadDynamics.throttles[0] * ErrorAngle / 4 * 20 * Mathf.Cos (ErrorTheta));
		throttleshifts[1] += (int) (-kP * QuadDynamics.throttles[1] * ErrorAngle / 4 * 20 * Mathf.Sin (ErrorTheta));
		throttleshifts[2] += (int) ( kP * QuadDynamics.throttles[2] * ErrorAngle / 4 * 20 * Mathf.Cos (ErrorTheta));
		throttleshifts[3] += (int) ( kP * QuadDynamics.throttles[3] * ErrorAngle / 4 * 20 * Mathf.Sin (ErrorTheta));
		QuadDynamics.shiftThrottles(throttleshifts);
		Debug.Log (string.Format("Throttle Shifts: {0}/{1}/{2}/{3}", throttleshifts[0], throttleshifts[1],
			throttleshifts[2], throttleshifts[3]));
	}
}
