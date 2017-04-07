
using UnityEngine;

using System.Collections;


public class NewBehaviourScript : MonoBehaviour {



	//Inspector initiated variables. Defaults are set for ease of use.

	public bool on = true;
	public float runDistance = 25.0f; //If the enemy should keep its distance, or charge in, at what point should they begin to run?
	public float runBufferDistance = 50.0f; //Smooth AI buffer. How far apart does AI/Target need to be before the run reason is ended.
	public int speed = 1;
	public int runSpeed = 2; 

	public int patrolSpeed = 1; 
	public float rotationSpeed = 10.0f; 
	public float viewDistance = 4.0f; 

	public float distanceLimit = 200.0f; 
	public float attackRange = 1.0f; 
	public float attackInterval = 0.50f; 
	public bool useWaypoints = false; 
	public bool reversePatrol = true; 
	public Transform[] waypoints; 

	public bool pauseAtWaypoints = false;
	public float pauseMin = 1.0f; 
	public float pauseMax = 3.0f; 
    //Gives up chase after f seconds 
	public float secondsToGiveUp = 3.0f;
	public Transform target; 
	//Set angle that an AI can view at, for example if angle is 180 it can see 
	// the player standing in the right.
	[Range(0,360)]
	public float angleOfSight;



	//private script handled variables

	private bool start = false; 
	private bool react = true; 
	private Vector3 previousTargetLocation; 
	CharacterController charController; 
	private bool seen = false; 
	private bool canAttack = false;
	private bool attacking = false; 
	private bool executeBufferState = false;
	private float lastShotFired; 
	private float lostPlayerTimer;
	private bool targetIsOutOfSight;
	private bool wpCountdown = false; 
	private int wpPatrol = 0; 
	private bool pauseWpControl; 
	private bool smoothAttackRangeBuffer = false;

	private float angle; 

	void Start() {
		StartCoroutine(Init()); 
	}

	IEnumerator Init() {
		charController = gameObject.GetComponent<CharacterController>();
		start = true;
		yield return null;
	}
		
	//Show orientation of the enemy - working only in editor
	void OnDrawGizmos (){
		Gizmos.color = Color.red;
		Gizmos.DrawLine (transform.position, transform.position + transform.forward);
	}
	void Awake()
	{
        //In case this is instantiated attach target dynamically
		target = GameObject.Find("FPSController").transform;
	}
	void Update () {

		angle = Vector3.Angle(transform.forward,target.position - transform.position) ;

		if (!on || !start){
			return;
		} else {
			Decide();
		}
	}

    //Decision tree following
	void Decide() {
		
		if (!target) { 
			return; 
		}

		//Get player position and direction towards and reversed in case of running from the player
		previousTargetLocation = target.position;
		Vector3 directionToPlayer = previousTargetLocation - transform.position; 
		Vector3 runAwayDirection = transform.position - previousTargetLocation; 

		float dist = Vector3.Distance(transform.position, target.position);

		if (TargetIsInSight ()) {

			//if all the conditions are not satisfied to at least chase the player, return
			if (!react)
				return;

			// if we are in range,  but cannot attack, chase
			if (dist > attackRange) {
				canAttack = false; 
				SteerAndMove (directionToPlayer);
			} else if ((smoothAttackRangeBuffer) && (dist > attackRange + 5.0f)) {
				smoothAttackRangeBuffer = false;
			} 
			//start attacking if close enough, with the set cooldown

			if ((dist < attackRange) || (dist < runDistance)) {
				if (Time.time > lastShotFired + attackInterval) {
					StartCoroutine (Attack ());
				}
			}
		} else if ((seen) && (!targetIsOutOfSight) && (react)) {
			lostPlayerTimer = Time.time + secondsToGiveUp;
			StartCoroutine(Chase(previousTargetLocation));
		} else if (useWaypoints) {
			Patrol();
		} 
	}
		
	IEnumerator Attack() {
		canAttack = true;
		if (!attacking) {
			attacking = true;
			while (canAttack) {

				yield return new WaitForSeconds (attackInterval);
			}
		}
	}

	//verify enemy can see the target
	bool TargetIsInSight () {
        
		if (angle > angleOfSight/2) {
			react = false;
			return false;
		}
		if ((distanceLimit > 0) && (Vector3.Distance (transform.position, target.position) > distanceLimit)) {
			react = false;
		} else
			react = true;
		
		if ((viewDistance > 0) && (Vector3.Distance (transform.position, target.position) > viewDistance)) {
			return false;
		} 
		// check if hit wall instead of player
		RaycastHit sight;
		if (Physics.Linecast (transform.position, target.position, out sight)) {

			if (!seen && sight.transform == target) {
				if (angle < angleOfSight/2)
                    seen = true;
				else
					return false;
			}
			return sight.transform == target;
		} else
			return false;
		
	}
	IEnumerator Chase (Vector3 position) {
		targetIsOutOfSight = true;

		while (targetIsOutOfSight) {

			Vector3 moveToward = position - transform.position;
			SteerAndMove (moveToward);

			if (TargetIsInSight () ) {
				targetIsOutOfSight = false;
				break;
			}
			if (Time.time > lostPlayerTimer) {
				targetIsOutOfSight = false;
                seen = false;
				break;
			}
			yield return null;
		}
	}

	void Patrol () {
		if (pauseWpControl) {
			return;
		}
		Vector3 destination = waypoints[wpPatrol].position;
		Vector3 moveToward = destination - transform.position;
		float distance = Vector3.Distance(transform.position, destination);

		SteerAndMove (moveToward);
		if (distance <= 1.5f) {
			if (pauseAtWaypoints) {
				if (!pauseWpControl) {
					pauseWpControl = true;
					StartCoroutine (WaypointPause ());
				}
			} else
				NewPath ();
		}
	}
		
	IEnumerator WaypointPause () {
		yield return new WaitForSeconds(Random.Range(pauseMin, pauseMax));
		NewPath();
		pauseWpControl = false;
	}

	void NewPath () {
		if (!wpCountdown) {
			wpPatrol++;
			if (wpPatrol >= waypoints.GetLength(0)) {
				wpPatrol = 0;
			}
		}
	}

	//standard movement behaviour

	void SteerAndMove (Vector3 direction) {
		int spd = speed;

		if (executeBufferState) {
			spd = runSpeed;
		}
		//rotate toward or away from the target

		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
		//slow down when we are not facing the target

		Vector3 forward = transform.TransformDirection(Vector3.forward);
		float speedModifier = Vector3.Dot(forward, direction.normalized);
		speedModifier = Mathf.Clamp01(speedModifier);

		//actually move toward or away from the target

		direction = forward * spd * speedModifier;
		charController.Move(direction * Time.deltaTime);

	}



}
