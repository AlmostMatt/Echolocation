using UnityEngine;
using System.Collections;

public class EchoParticle : MonoBehaviour {
	public float minRad = 0f;
	public float maxRad = 35f;

	public bool dead = false;

	private float prevradius;
	public float radius;
	public float arc; // radians
	public float speed;

	private Transform quad;
	private Mesh mesh;
	private MeshRenderer meshRenderer;

	private float collisionRadius = -1f;

	private int frame = 0;
	public float fadeTime;

	public Color col;
	//private static Color normalCol = new Color(1f, 1f, 1f, 0.25f);
	//private static Color dirtCol = new Color(1f, 0.7f, 0.1f, 0.25f);
	//private static Color safeCol = new Color(0.4f, 1f, 0.4f, 0.25f);
	private static Color normalCol = new Color(0.275f, 0.129f, 0.082f, 0.25f);
	private static Color dirtCol = new Color(0.369f, 0.216f, 0.086f, 0.25f);
	private static Color safeCol = new Color(0.23f, 0.27f, 0.26f, 0.25f);

	public void init(float direction, float arcsize, float echospeed, float range=35f, float fade=0.7f) {
		radius = 0f;
		arc = arcsize;
		speed = echospeed;
		fadeTime = fade;

		Vector2 v = echospeed * new Vector2(Mathf.Cos(direction), Mathf.Sin(direction));
		transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Rad2Deg * direction);
		Rigidbody2D echoRB = transform.GetComponent<Rigidbody2D>();
		echoRB.velocity = v;
		maxRad = range;
		Color initCol = getTileColor(Scene.getTile(transform.position));
		col = initCol;
		meshRenderer.material.SetColor("_TintColor", initCol);
	}

	/* ALTERNATIVE TO INIT : RESUME */
	public void resume(EchoParticle previous, Color newCol) {
		// assume velocity/fadeTime/etc copied since they are public
		minRad = previous.radius;
		radius = previous.radius;
		maxRad = previous.maxRad;
		col = newCol;
		meshRenderer.material.SetColor("_TintColor", newCol);
		transform.GetComponent<Rigidbody2D>().velocity = previous.GetComponent<Rigidbody2D>().velocity;
	}

	// Use this for initialization
	void Awake () {
		quad = transform.FindChild("quad");
		mesh = quad.GetComponent<MeshFilter>().mesh;
		initMesh();
		meshRenderer = quad.GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		frame++;
		if (!dead) {
			float len = speed * fadeTime;
			float r1 = Mathf.Max(minRad, radius - len);
			float r2 = radius;//Mathf.Min(radius, maxRad);
			float a1 = Mathf.Max(0f, 1f - ((radius - minRad)/len));
			float a2 = 1f;
			if (collisionRadius != -1f) {
				r2 = collisionRadius;
				a2 = (collisionRadius - r1) / (radius - r1);
			}
			
			if (r2 <= r1) {
				removeThis();
			} else {
				Tile t = Scene.getTile(transform.position);
				Color newcol = getTileColor(t);
				if (radius < maxRad && newcol != col && collisionRadius == -1f) {
					split(newcol);
					maxRad = radius;
				}
				if (frame %2 == 0) {
					updateMesh((r2 - r1), r1 * arc, r2 * arc, a1, a2);
				} else {
					updateMesh((r2 - r1), r1 * arc, r2 * arc, a1, a2);
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

	public static Color getTileColor(Tile t) {
		switch (t) {
		case Tile.SAFE:
			return safeCol;
		case Tile.EMPTY2:
			return dirtCol;
		case Tile.EMPTY:
		default:
			return normalCol;
		}
	}

	private void split(Color newcol) {
		GameObject newEcho = Instantiate(gameObject);
		EchoParticle newEchoParticle = newEcho.GetComponent<EchoParticle>();
		newEchoParticle.resume(this, newcol);
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
		if (dead && gameObject != null) {
			Destroy(gameObject);
		}
		radius += Time.fixedDeltaTime * speed;
		if (radius >= maxRad && collisionRadius == -1f) {
			collisionRadius = radius;
			Rigidbody2D echoRB = transform.GetComponent<Rigidbody2D>();
			echoRB.velocity = new Vector3();
		}
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
