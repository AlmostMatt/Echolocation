using UnityEngine;
using System.Collections;

public class Enemy : Steering {

	public bool dead = false;

	void Awake() {
		MAXV = 3f;
		ACCEL = 10f;
	}
	
	// Update is called once per frame
	void Update () {
		Player player = Scene.getPlayer();
		float VISION = 10f;
		Vector2 offset =(player.transform.position - transform.position);
		float dist = offset.sqrMagnitude;
		if (dist < VISION * VISION) {
			int WALL_MASK = 1 << 10;
			RaycastHit2D hit = Physics2D.Raycast(transform.position, offset, offset.magnitude, WALL_MASK);
			if (hit != null) {
				Debug.Log("Hit : " + hit.collider);
			}
			if (hit == null  || hit.collider == null || hit.collider.transform == player.transform) {
				seek(player.transform.position);
			}
		} else {
			brake();
			//wander();
		}
	}

	public void damage(float amt) {
		dead = true;
		Destroy(gameObject);
	}
}
