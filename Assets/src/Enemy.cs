using UnityEngine;
using System.Collections;

public class Enemy : Steering {

	public bool dead = false;
	private float health = 2f;

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
		if (!player.safe && dist < VISION * VISION) {
			int WALL_MASK = 1 << 10;
			RaycastHit2D hit = Physics2D.Raycast(transform.position, offset, offset.magnitude, WALL_MASK);
			if (hit == null  || hit.collider == null || hit.collider.transform == player.transform) {
				// check for intermediary safe tiles)
				bool playerSafe = Scene.tileRaycast(transform.position, player.transform.position, Tile.SAFE);
				if (!playerSafe) {
					seek(player.transform.position);
					// they both have radius 0.6
					if (dist < 1.3f * 1.3f) {
						player.damage(1f);
					}
				}
			}
		} else {
			brake();
			//wander();
		}
	}

	public void damage(float amt) {
		health -= amt;
		if (health <= 0f) {
			dead = true;
			Destroy(gameObject);
		}
	}
}
