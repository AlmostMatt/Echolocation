using UnityEngine;
using System.Collections;

public class Shot : MonoBehaviour {
	private float alpha = 1f;
	private SpriteRenderer img;

	// Use this for initialization
	void Start () {
		img = transform.FindChild("img").GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		alpha -= 3 * Time.deltaTime;
		if (alpha <= 0f) {
			Destroy(gameObject);
		} else {
			img.color = new Color(1f, 1f, 1f, alpha);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		// shots only collide with enemies
		Enemy e = other.GetComponent<Enemy>();
		e.damage(1f);
	}
}
