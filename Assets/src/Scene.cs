using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scene : MonoBehaviour {

	public Map map;
	private List<EchoParticle> echos;
	private static Scene singleton;

	public Player player;
	public List<Enemy> enemies;

	private Texture2D shadowMask;

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
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = echos.Count - 1; i >= 0; --i) {
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
		Transform cam = Camera.main.transform;
		cam.position = new Vector3(player.transform.position.x, player.transform.position.y, cam.position.z);
	}

	public void addEcho(EchoParticle p) {
		echos.Add(p);
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


}
