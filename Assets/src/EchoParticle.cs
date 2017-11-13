using UnityEngine;
using System.Collections;

/*
 * Trapezoid particles that grow outwards.
 * 
 * Update and FixedUpdate seem to be on separate threads.
 * So some echo particles move after the mesh is updated, which would result in incorrect mesh sizes.
 * To address this, I have split the physics enabled object and the mesh into separate instances
 * 
 */
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

	public float collisionRadius = -1f;

	private int frame = 0;
	public float fadeTime;
	public EchoParticle successor = null;
	public EchoParticle precedent = null;
	public GameObject meshObject = null;
	private bool hasSuccessor = false;

	public Color currentColor;

	public void init(
			Vector3 echoPos,
			GameObject echoMesh,
			float direction,
			float arcsize,
			float echospeed,
			float range=35f,
			float fade=0.7f) {
		transform.position = echoPos;
		meshObject = echoMesh;
		meshObject.transform.position = echoPos;
		meshObject.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Rad2Deg * direction);
		transform.GetComponent<Rigidbody2D>().velocity = echospeed * new Vector2(Mathf.Cos(direction), Mathf.Sin(direction));

		radius = 0f;
		arc = arcsize;
		speed = echospeed;
		fadeTime = fade;
		maxRad = range;
		currentColor = Map.getTileColor(Scene.getTile(transform.position));
		// Generic initialization to the mesh object
		mesh = meshObject.GetComponent<MeshFilter>().mesh;
		meshRenderer = meshObject.GetComponent<MeshRenderer>();
		meshRenderer.material.SetColor("_TintColor", currentColor);
		initMesh();
	}

	private void split(Color newColor) {
		hasSuccessor = true;
		GameObject newEchoCollider = Instantiate(Scene.get().echoColliderObject);
		GameObject newEchoMesh = Instantiate(Scene.get().echoMeshObject);
		EchoParticle newEchoParticle = newEchoCollider.GetComponent<EchoParticle>();
		newEchoParticle.resume(this, newEchoMesh, newColor);
	}

	/* VARIANT OF INIT
	 * When an echo particle reaches a new color, it stops growing,
	 * creates a new particle, and calls resume on the new particle.
	 */
	public void resume(EchoParticle previous, GameObject echoMesh, Color newColor) {
		transform.position = previous.transform.position;
		meshObject = echoMesh;
		meshObject.transform.position = previous.meshObject.transform.position;
		meshObject.transform.rotation = previous.meshObject.transform.rotation;
		transform.GetComponent<Rigidbody2D>().velocity = previous.GetComponent<Rigidbody2D>().velocity;

		minRad = previous.collisionRadius;
		radius = previous.radius;
		maxRad = previous.maxRad;
		arc = previous.arc;
		speed = previous.speed;
		fadeTime = previous.fadeTime;
		currentColor = newColor;
		// Give echoes parent/child references (for debugging)
		previous.successor = this;
		precedent = previous;
		// Generic initialization to the mesh object
		mesh = meshObject.GetComponent<MeshFilter>().mesh;
		//Debug.Log(mesh);
		meshRenderer = meshObject.GetComponent<MeshRenderer>();
		meshRenderer.material.SetColor("_TintColor", currentColor);
		initMesh();
	}

	// Use this to perform initialization code
	void Awake () {
		// (Note: Awake is called during GameObject instantiation, and the mesh object isn't associated until init())
	}

	void computeNewRadiusAndUpdateMesh() {
		if (dead) {
			return;
		}
		float len = speed * fadeTime;
		float r1 = Mathf.Max(minRad, radius - len);
		float r2 = radius;
		if (collisionRadius != -1f) {
			r2 = collisionRadius;
		}
		// Alpha is 1 at radius and 0 at radius-len. r1 and r2 are somewhere in between.
		float a1 = Mathf.Max(0.0f, (r1-(radius-len))/len);
		float a2 = (r2-(radius-len))/len;
		// Remove echo particles when they have finished fading out
		if (a2 <= 0) {
			removeThis();
		} else {
			//if (frame %2 == 0) {
			updateMesh(r1, r2, r1 * arc, r2 * arc, a1, a2);
			//}
		}
	}

	void onPreRender() {
	}

	void LateUpdate() {
		//Debug.Log("late update!");
	}

	// Update is called once per frame
	void Update () {
		frame++;
		computeNewRadiusAndUpdateMesh();
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

	void updateMesh(float radius1, float radius2, float arc1, float arc2, float alpha1=0f, float alpha2=1f) {
		mesh.vertices = new Vector3[]
		{
			new Vector3(radius2, arc2/2, 0),
			new Vector3(radius2, -arc2/2, 0),
			new Vector3(radius1, arc1/2, 0),
			new Vector3(radius1, -arc1/2, 0),
		};
		mesh.uv = new Vector2[] // The texture coordinates for each vertex
		{
			new Vector2(alpha2, 1),
			new Vector2(alpha2, 0),
			new Vector2(alpha1, 1),
			new Vector2(alpha1, 0),
		};
		mesh.RecalculateBounds(); // this can be approximate
	}

	void FixedUpdate() {
		if (dead && gameObject != null) {
			Debug.Log("removing echo during update");
			Destroy(meshObject);
			Destroy(gameObject);
		}
		// Radius always increases. If an object has stopped, collisionRadius will be used for r2, but radius will still influence r1.
		radius += Time.fixedDeltaTime * speed;
		
		GameObject minimapTile = Scene.getMinimapTile(transform.position);
		if (minimapTile != null && collisionRadius == -1) {
			minimapTile.active = true;
		}
		Tile t = Scene.getTile(transform.position);
		Color newColor = Map.getTileColor(t);
		if (t != Tile.WALL && radius < maxRad && newColor != currentColor && collisionRadius == -1f) {
			collisionRadius = radius;
			split(newColor);
			maxRad = radius;
			collide();
		}

		if (radius >= maxRad) {
		    collide();
		}
	}

	private void removeThis() {
		dead = true;
		Destroy(meshObject);
		Destroy(gameObject);
	}

	void collide() {
		// Only count the first collision
		if (collisionRadius == -1f) {
			collisionRadius = radius;
		}
		// Disable the collider so that no other collision checking will happen for this particle.
		GetComponent<Collider2D>().enabled = false;
	}

	void OnTriggerEnter2D(Collider2D other) {
		collide();
	}
}
