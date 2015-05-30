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
		float dist = (player.transform.position - transform.position).sqrMagnitude;
		if (dist < VISION * VISION) {
			seek(player.transform.position);
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
