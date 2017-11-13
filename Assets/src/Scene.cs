using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scene : MonoBehaviour {

	public GameObject echoColliderObject;
	public GameObject echoMeshObject;

	public Map map;
	public List<EchoParticle> echos;
	private static Scene singleton;

	public Player player;
	public List<Enemy> enemies;

	private Texture2D shadowMask;
	private Transform cam;

	void Awake() {
		map = GetComponent<Map>();
		map.generateMap();
		echos = new List<EchoParticle>();
		singleton = this;

		player = map.getPlayer();
		enemies = map.getEnemies();
		/*
		int texW = 100;
		int texH = 100;
		shadowMask = new Texture2D(texW, texH, TextureFormat.Alpha8, false);
		// set pixels
		for (int x=0; x<texW; ++x) {
			for (int y=0; y<texH; ++y) {
				shadowMask.SetPixel(x, y, Color.white);
			}
		}
		shadowMask.Apply();

		RenderTexture
			*/
	}

	public void drawQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {

	}

	// Use this for initialization
	void Start () {
		cam = Camera.main.transform;
		cam.position = new Vector3(player.transform.position.x, player.transform.position.y, cam.position.z);
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = echos.Count - 1; i >= 0; --i) {
			// TODO: fix this code (echoes clean themselves up, and split echoes aren't added to this)
			if (echos[i].dead) {
				echos.Remove(echos[i]);
			}
		}
		for (int i = enemies.Count - 1; i >= 0; --i) {
			if (enemies[i].dead) {
				enemies.Remove(enemies[i]);
			}
		}
		// camera to follow the player
		Vector2 offset = player.transform.position - cam.position;
		float camspd = 20f * Time.deltaTime;
		if (offset.sqrMagnitude > camspd * camspd) {
			cam.position += camspd * (Vector3) offset.normalized;
		} else {
			cam.position = new Vector3(player.transform.position.x, player.transform.position.y, cam.position.z);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
		if (Input.GetKeyDown(KeyCode.R)) {
			Application.LoadLevel(Application.loadedLevel);
		}
	}

	public static void echo(Vector3 echoPos, float echoSpeed=8f, float range=15f, float fadeTime=0.7f, int numP=64) {
		// Force z to -6 so that echos appear in front of other objects
		echoPos.z = -6;
		// TODO: fix bug where particles are sometimes not created or rendered on lower quality settings.
		// This is 100% related to quality settings - particle raycast budget
		// Each echo creates 64 particles that have colliders. if budget is 64 I get 1 echo. if 256 I get 4 echos.
		// Note: this seems to have gone away when I separated echo collider and mesh
		//   This probably means that it no longer considers the collider to be a particle.
		// TODO: fix the phsyics bug that occurs on low quality settings (player collides with something)
		for (int n =0; n<numP; ++n) {
		//int n = 0;
			float rotation = n * 2 * Mathf.PI / numP;
			float arc = 2 * Mathf.PI / numP;
			GameObject echoCollider = Instantiate(singleton.echoColliderObject);
			GameObject echoMesh = Instantiate(singleton.echoMeshObject);
			EchoParticle echo = echoCollider.GetComponent<EchoParticle>();
			echo.init(echoPos, echoMesh, rotation, arc, echoSpeed, range, fadeTime);
			get().echos.Add(echo);
		}
	}

	public static Scene get() {
		return singleton;
	}
	
	public static Player getPlayer() {
		return singleton.player;
	}

	public static List<Enemy> getEnemies() {
		return singleton.enemies;
	}

	public Vector2 getWorldMousePos() {
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		return ray.origin;
	}

	public static Tile getTile(Vector2 gamePos) {
		Map map = get().map;
		return map.getTile(map.gameToMap(gamePos));
	}

	public static GameObject getMinimapTile(Vector2 gamePos) {
		Map map = get().map;
		return map.getMinimapTile(map.gameToMap(gamePos));
	}

	public static bool tileRaycast(Vector2 gamePos1, Vector2 gamePos2, Tile hitTile) {
		Map map = get().map;
		return map.raycast(map.gameToMap(gamePos1), map.gameToMap(gamePos2), hitTile);
	}
}
