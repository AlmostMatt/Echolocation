using UnityEngine;
using System.Collections;

public class EchoParticle : MonoBehaviour {
	public float duration = 1f;
	public bool dead = false;

	private float radius;
	private float arc; // radians
	private float speed;

	private Transform imgTransform;
	private SpriteRenderer imgrenderer;
	private TrailRenderer trailrenderer;
	private Transform quad;
	private Mesh mesh;
	private MeshRenderer meshRenderer;

	private float collisionRadius = -1f;

	private int frame = 0;

	public void init(float direction, float arcsize, float echospeed) {
		radius = 0f;
		arc = arcsize;
		speed = echospeed;

		Vector2 v = echospeed * new Vector2(Mathf.Cos(direction), Mathf.Sin(direction));
		transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Rad2Deg * direction);
		Rigidbody2D echoRB = transform.GetComponent<Rigidbody2D>();
		echoRB.velocity = v;
		duration = 5f;
	}

	// Use this for initialization
	void Start () {
		imgTransform = transform.FindChild("img");
		imgrenderer = imgTransform.GetComponent<SpriteRenderer>();
		trailrenderer = transform.FindChild("trail").GetComponent<TrailRenderer>();
		imgrenderer.enabled = false;
		trailrenderer.enabled = false;
		quad = transform.FindChild("quad");
		mesh = quad.GetComponent<MeshFilter>().mesh;
		initMesh();
		meshRenderer = quad.GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		frame++;
		if (!dead) {
			float len = speed * 0.7f;
			float r1 = Mathf.Max(0f, radius - len);
			float r2 = radius;
			float a1 = Mathf.Max(0f, 1f - (radius/len));
			float a2 = 1f;
			if (collisionRadius != -1f) {
				r2 = collisionRadius;
				a2 = (collisionRadius - r1) / (radius - r1);
			}
			
			if (collisionRadius != -1f && collisionRadius <= r1) {
				removeThis();
			} else {
				if (frame %2 == 0) {
					updateMesh((r2 - r1), r1 * arc, r2 * arc, 0.5f * a1, a2);
				} else {
					updateMesh((r2 - r1), r1 * arc, r2 * arc, 0.5f * a1, a2);
				}
			}
			/*
			if (collisionRadius == -1f) {
				float segmentSize = radius * arc;
				imgTransform.localScale = new Vector3(5f, 100f * segmentSize, 1f);

				float prevRadius = Mathf.Max(0f, radius - (speed * trailrenderer.time));
				float prevWidth = prevRadius * arc;
				trailrenderer.startWidth = segmentSize;
				trailrenderer.endWidth = prevWidth;
			} else {
				if (Mathf.Max(0f, radius - speed * trailrenderer.time) > collisionRadius) {
					removeThis();
				}
			}
			float alpha = Mathf.Min(1f, duration / 4);
			imgrenderer.color = new Color(1f, 1f, 1f, alpha);
			*/
		}
	}

	void initMesh() {
		mesh.MarkDynamic();
		mesh.vertices = new Vector3[]
		{
			new Vector3(0, 0, 0),
			new Vector3(0, 0, 0),
			new Vector3(0, 0, 0),
			new Vector3(0, 0, 0),
		};
		mesh.uv = new Vector2[]
		{
			new Vector2(1, 1),
			new Vector2(1, 0),
			new Vector2(0, 1),
			new Vector2(0, 0),
		};
		mesh.triangles = new int[]
		{
			0, 1, 2,
			2, 1, 3
		};
	}

	void updateMesh(float len, float arc1, float arc2, float a1=0f, float a2=1f) {
		mesh.vertices = new Vector3[]
		{
			new Vector3(0, arc2/2, 0),
			new Vector3(0, -arc2/2, 0),
			new Vector3(-len, arc1/2, 0),
			new Vector3(-len, -arc1/2, 0),
		};
		mesh.uv = new Vector2[] // this can be approximate
		{
			new Vector2(a2, 1),
			new Vector2(a2, 0),
			new Vector2(a1, 1),
			new Vector2(a1, 0),
		};
		mesh.RecalculateBounds(); // this can be approximate
	}

	void FixedUpdate() {
		duration -= Time.fixedDeltaTime;
		if (duration <= 0f) {
			dead = true;
			Destroy(gameObject);
		} 
		radius += Time.fixedDeltaTime * speed;
	}

	private void removeThis() {
		dead = true;
		Destroy(gameObject);
	}

	void OnCollision2D(Collision2D coll) {
		foreach (ContactPoint2D contact in coll.contacts) {
			Debug.Log(contact.normal);
		}
		collisionRadius = radius;
		Rigidbody2D echoRB = transform.GetComponent<Rigidbody2D>();
		//echoRB.velocity = new Vector3();
	}

	void OnTriggerEnter2D(Collider2D other) {
		collisionRadius = radius;
		Rigidbody2D echoRB = transform.GetComponent<Rigidbody2D>();
		echoRB.velocity = new Vector3();
	}
}
