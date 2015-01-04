using UnityEngine;
using System;
using System.Collections;

public class QuadDynamics : MonoBehaviour {
	
	
	//MOTORS 1 AND 4 CW
	public static int[] throttles = {10,10,10,10};
	public static int ThrottleAve;
	float[] rotationSpeeds = {0,0,0,0};
	float[] torques = {0,0,0,0};
	float[] thrusts = {0,0,0,0};
	
	float minMotorVolt = 7.4f;
	float maxMotorVolt = 11.1f;
	
	Vector3[] motorPositions = {
		new Vector3(0.225f,0,0.225f),
		new Vector3(0.225f,0,-0.225f),
		new Vector3(-0.225f,0,-0.225f),
		new Vector3(-0.225f,0,0.225f)
	};
	
	//Battery Stuff
	int dischargeRate;
	int capacity;
	int maxCurrent;
	float maxVoltage = 10f;
	
	//Physics Stuff - Masses in kg
	float loadMass;
	float frameMass = 0.300f;
	float electronicsMass;
	float freeStreamVelocity = 0;
	float totalMass;
	
	//Props Stuff
	float propPitch = 4.5f;
	float propDiameter = 10f;
	float torqueConstant;
	
	//Electronics Stuff:
	int motorKV = 850;
	int escMin, escMax;
	
	
	int frame = 0;
	
	
	//Lock
	private static object LOCK = new object();
	
	// Use this for initialization
	void Start () {
		totalMass = loadMass + frameMass;
		totalMass = 1.5f;
		rigidbody.mass = totalMass;
		maxCurrent = capacity * dischargeRate;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void FixedUpdate(){
		calcThrusts();
		applyThrusts();
		
		
		Debug.Log ("Throttles: 1:" + throttles[0] + "\t2:" + throttles[1] + "\t3:"
			+ throttles[2] + "\t4:" + throttles[3]);
		
	}
	
	float calcRPM(float voltage){
		return (float) (voltage * motorKV);
	}
	
	public static void setThrottle(int a, int b, int c, int d){
		throttles[0] = a;
		throttles[1] = b;
		throttles[2] = c;
		throttles[3] = d;
	}
	
	public static void setThrottle(int throttle){
		throttles[0] = throttle;
		throttles[1] = throttle;
		throttles[2] = throttle;
		throttles[3] = throttle;
	}
	
	public static void setThrottleAve(int throttle){
		if(throttle > 100)
			ThrottleAve = 100;
		else if(throttle < 0)
			ThrottleAve = 0;
		else
			ThrottleAve = throttle;
	}
	
	public static void shiftThrottleAve(int throttle){
		ThrottleAve += throttle;
		if(ThrottleAve > 100)
			ThrottleAve = 100;
		else if(ThrottleAve < 0)
			ThrottleAve = 0;
	}
	
	
	public static void shiftThrottle(int change){
		lock(LOCK){
			for(int i = 0; i < 4; i++){
				throttles[i] += change;
			}
		}
		
	}
	
	public static void shiftThrottles(int change1, int change2, int change3, int change4){
		lock(LOCK){
			throttles[0] += change1;
			throttles[1] += change2;
			throttles[2] += change3;
			throttles[3] += change4;
		}
	}
	
	public static void shiftThrottles(int[] shifts){
		if(shifts.Length == 4)
			shiftThrottles(shifts[0],shifts[1],shifts[2],shifts[3]);
		else Debug.LogError("shiftThrottles(int[] shifts) must be passed an array of size 4, passed size " 
			+ shifts.Length);
	}
	
	float calcThrust(float RPM){
		float thrust = (float) ( 4.392399 * Mathf.Pow(10, -8f) * RPM 
			* ( Mathf.Pow(propDiameter, 3.5f) / Mathf.Pow(propPitch, 0.5f) ) * 
			((4.23333 * Mathf.Pow(10, -4f) * RPM * propPitch) - freeStreamVelocity) );
		
		return thrust;
	}
	
	float calcThrustFromThrottle(int throttle){
		return calcThrust(calcRPM((float) (maxVoltage * throttle/100)));
	}
	
	void calcThrusts(){
		for(int i = 0; i < 4; i++){
			if(throttles[i] > 100) throttles[i] = 100;
			else if(throttles[i] < 0) throttles[i] = 0;
			thrusts[i] = calcThrustFromThrottle(throttles[i]);
		}
	}
	
	void applyThrusts(){
		for(int i = 0; i <4; i++){
			//Vector3 thrustVector = Vector3.up * thrusts[i];
			Vector3 thrustVector = transform.up * thrusts[i];
			rigidbody.AddForceAtPosition(thrustVector, (transform.rotation * motorPositions[i]) + transform.position);
			Debug.DrawRay(transform.rotation * motorPositions[i], thrustVector, Color.green);
		}
		
	}
	public static void normaliseThrottles(){
		lock(LOCK){
			int sum = 0;
			foreach(int i in throttles){
				sum += i;
			}
			setThrottle(sum/4,sum/4,sum/4,sum/4);
		}
	}
	
	public static void resetThrottlesToAve(){
		setThrottle(ThrottleAve);
	}
	
}
