using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour, Actor {
	private int ECHO = 0;
	private int ATTACK = 1;

	public GameObject explosionObj;

	private ActionMap actionMap;
	private StatusMap statusMap;
	private Rigidbody2D rb;
	float facingAngle = 0f;
	
	void Awake() {
		statusMap = new StatusMap(this);
		actionMap = new ActionMap(this);
		actionMap.add(ECHO, new Ability(0.5f));
		actionMap.add(ATTACK, new Ability(0.3f));
		rb = GetComponent<Rigidbody2D>();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float ACCEL = 20f;
		float MAXV = 5f;
		float ROTV = 4 * Mathf.PI;

		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		Vector2 desiredV = MAXV * input;
		rb.AddForce(ACCEL * (desiredV - rb.velocity));
		if (rb.velocity.sqrMagnitude > MAXV * MAXV) {
			rb.velocity = MAXV * rb.velocity.normalized;
		} else if (rb.velocity.sqrMagnitude < 1f) {
		}
		echolocate();

		actionMap.update(Time.deltaTime);
		statusMap.update(Time.deltaTime);

		// look at mouse
		Vector2 mouse = Scene.get().getWorldMousePos();
		turnToward(mouse, ROTV * Time.deltaTime);

		if (Input.GetMouseButton(0)) {
			attack(mouse);
		}
	}

	private void attack(Vector2 mouse) {
		if (actionMap.ready(ATTACK)) {
			actionMap.use(ATTACK, null);
			GameObject expl = Instantiate(explosionObj);
			expl.transform.position = mouse;
			float ERAD = 0.8f;
			foreach (Enemy e in Scene.getEnemies()) {
				if ((mouse - (Vector2) e.transform.position).sqrMagnitude < ERAD * ERAD) {
					e.damage(1f);
				}
			}
		}
	}

	private void echolocate() {
		if (actionMap.ready(ECHO)) {
			actionMap.use(ECHO, null);
			Scene.echo(transform.position);
		}
	}

	private void turnToward(Vector2 pt, float turnAmount) {
		Vector2 offset = pt - (Vector2) transform.position;
		float desiredAngle = Mathf.Atan2(offset.y, offset.x);
		float angleDiff = desiredAngle - facingAngle;
		while (angleDiff < - Mathf.PI) {
			angleDiff += 2 * Mathf.PI;
		}
		while (angleDiff > Mathf.PI) {
			angleDiff -= 2 * Mathf.PI;
		}
		if (Mathf.Abs(angleDiff) >= turnAmount) {
			facingAngle += turnAmount;
		} else {
			facingAngle = desiredAngle;
		}
		transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Rad2Deg * facingAngle);
	}

	void OnCollisionEnter2D(Collision2D other) {

	}
}
