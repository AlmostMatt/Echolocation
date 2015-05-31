using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour, Actor {
	private int ECHO = 0;
	private int ATTACK = 1;
	
	public GameObject explosionObj;
	public GameObject bulletObj;

	public AudioClip shootSound;
	public AudioClip painSound;

	private ActionMap actionMap;
	private StatusMap statusMap;
	private Rigidbody2D rb;
	float facingAngle = 0f;

	public bool safe;
	private Vector3 respawnPoint;
	private Transform gun;
	private AudioSource playerAudio;
	private AudioSource gunAudio;

	private float health;
	private float maxHealth = 3f;

	// for help text
	private bool hasMovedVertically;
	private bool hasMovedHorizontally;
	private int shotsFired = 0;
	private bool dangerMessaged = false;
	private bool safetyMessaged = false;
	
	void Awake() {
		statusMap = new StatusMap(this);
		actionMap = new ActionMap(this);
		actionMap.add(ECHO, new Ability(0.5f));
		actionMap.add(ATTACK, new Ability(0.3f));
		rb = GetComponent<Rigidbody2D>();
		respawnPoint = transform.position;
		gun = transform.FindChild("gun");
		playerAudio = GetComponent<AudioSource>();
		gunAudio = gun.GetComponent<AudioSource>();
	}

	// Use this for initialization
	void Start () {
		respawnPoint = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		float ACCEL = 20f;
		float MAXV = 5f;
		float ROTV = 4 * Mathf.PI;

		if (!statusMap.has(State.DEAD)) {
			Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			if (input.x != 0f) {
				hasMovedHorizontally = true;
			}
			if (input.y != 0f) {
				hasMovedVertically = true;
			}
			Vector2 desiredV = MAXV * input;
			rb.AddForce(ACCEL * (desiredV - rb.velocity));
			if (rb.velocity.sqrMagnitude > MAXV * MAXV) {
				rb.velocity = MAXV * rb.velocity.normalized;
			} else if (rb.velocity.sqrMagnitude < 1f) {
			}
			echolocate();
			// look at mouse
			Vector2 mouse = Scene.get().getWorldMousePos();
			turnToward(mouse, ROTV * Time.deltaTime);
			if (Input.GetMouseButton(0)) {
				attack(mouse);
			}
		} else {
			HUD.takeDamage();
		}

		actionMap.update(Time.deltaTime);
		statusMap.update(Time.deltaTime);

		safe = (Scene.getTile(transform.position) == Tile.SAFE);
		if (safe) {
			health = maxHealth;
			// respawn at center of tile
			respawnPoint = Scene.get ().map.mapToGame(Scene.get ().map.gameToMap(transform.position));
		}
		/* DIALOG / HELP */
		
		if (statusMap.has(State.DEAD)) {
			HUD.setText("", 0f);
		} else if (!hasMovedHorizontally || !hasMovedVertically) {
			HUD.setText("Move with WASD", 1f);
		} else if (shotsFired < 2) {
			if (!HUD.hasText()) {
				HUD.setText("Shoot with the mouse", 1f);
			}
		} else if (!dangerMessaged || !safetyMessaged) {
			if (!dangerMessaged && !HUD.hasText() && !safe) {
				HUD.setText("Watch out for moving shapes!", 3f);
				dangerMessaged = true;
			}
			if (!safetyMessaged && !HUD.hasText() && safe) {
				HUD.setText("These gray areas are safe.", 3f);
				safetyMessaged = true;
			}
		}
	}

	private void attack(Vector2 mouse) {
		if (actionMap.ready(ATTACK)) {
			gunAudio.PlayOneShot(shootSound, 0.5f);
			++shotsFired;
			actionMap.use(ATTACK, null);
			GameObject shot = Instantiate(bulletObj);
			shot.transform.position = gun.position;
			shot.transform.rotation = gun.rotation;
			int WALL_MASK = 1 << 10;
			float shotRange = 15f;
			RaycastHit2D hit = Physics2D.Raycast(gun.position, gun.right, shotRange, WALL_MASK);
			if (hit.collider != null) {
				shotRange = hit.distance;
			}
			shot.transform.localScale = new Vector3(shotRange, 1f, 1f);

			/*
			GameObject expl = Instantiate(explosionObj);
			expl.transform.position = mouse;
			float ERAD = 0.8f;
			foreach (Enemy e in Scene.getEnemies()) {
				if ((mouse - (Vector2) e.transform.position).sqrMagnitude < ERAD * ERAD) {
					e.damage(1f);
				}
			}
			*/
		}
	}

	public void damage(float amt) {
		if (statusMap.has(State.DEAD)) { return;}
		// sound?
		if (!statusMap.has(State.INVULNERABLE)) {
			playerAudio.PlayOneShot(painSound, 1f);
			statusMap.add (new Status(State.INVULNERABLE), 0.8f);
			HUD.takeDamage();
			health -= amt;
		}
		if (health <= 0f) {
			statusMap.add(new Status(State.DEAD), 1000f);
			GetComponent<Rigidbody2D>().velocity = new Vector3();
			// delayed respawn coroutine
			Invoke("respawn", 1f);
		}
	}

	private void respawn() {
		health = maxHealth;
		transform.position = respawnPoint;
		foreach (Enemy e in Scene.getEnemies()) {
			e.reset();
		}
		statusMap.remove(State.DEAD);
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
