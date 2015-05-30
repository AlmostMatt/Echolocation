using UnityEngine;
using System.Collections.Generic;

public enum Tile {EMPTY=0, WALL=1};

public class Map : MonoBehaviour {
	public GameObject wallObj;
	public GameObject playerObj;
	public GameObject enemyObj;

	public GameObject[] levels;

	private GameObject level;

    // grid size
	private int w = 20;
    private int h = 20;
    private float tileSize = 1f;
	
	private List<List<Tile>> map;
	private Player player;
	private List<Enemy> enemies = new List<Enemy>();

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void generateMap() {
		// instantiate level object,
		// get player/enemy/wall objects in the scene

		level = levels[0];
		enemies.Clear();
		Enemy[] enemyList = level.transform.GetComponentsInChildren<Enemy>();
		for (int i=0; i<enemyList.Length; ++i) {
			enemies.Add(enemyList[i]);
		}

		GameObject pO = Instantiate(playerObj);
		pO.transform.parent = transform;
		pO.transform.localPosition = new Vector2();
		player = pO.GetComponent<Player>();

		map = new List<List<Tile>>();
		GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
		foreach (GameObject wall in walls) {
			Transform t = wall.transform;
			for (int x=0; x<t.localScale.x; ++x) {
				for (int y =0; y<t.localScale.y; ++y) {

				}
			}
		}

		/*
		map = new List<List<Tile>>();

		int NUMWALLS = 2;
		int NUMENEMIES = 2;

		RandomSet<Vector2> emptyTiles = new RandomSet<Vector2>();
		// default - water
		for (int x = 0; x < w; ++x) {
			map.Add(new List<Tile>());
			for (int y = 0; y < h; ++y) {
				map[x].Add(Tile.EMPTY);
				emptyTiles.Add (new Vector2(x,y));
			}
		}
        System.Random rnd = new System.Random();
		for (int i =0; i<NUMWALLS; i++) {
			Vector2 wallPos = emptyTiles.popRandom();
			int x = (int) wallPos.x;
			int y = (int) wallPos.y;

            setTile(wallPos, Tile.WALL);
            GameObject wall = Instantiate(wallObj);
            wall.transform.parent = transform;
			wall.transform.localPosition = mapToGame(wallPos);
        }

		Vector2 pos = emptyTiles.popRandom();
		GameObject pO = Instantiate(playerObj);
		pO.transform.parent = transform;
		pO.transform.localPosition = mapToGame(pos);
		player = pO.GetComponent<Player>();

		// remove empty tiles near the player
		for (int i =0; i<NUMENEMIES; i++) {
			pos = emptyTiles.popRandom();
			int x = (int) pos.x;
			int y = (int) pos.y;
			
			setTile(pos, Tile.WALL);
			GameObject enemy = Instantiate(enemyObj);
			enemy.transform.parent = transform;
			enemy.transform.localPosition = mapToGame(pos);
			enemies.Add(enemy.GetComponent<Enemy>());
		}
		*/
	}
	
	public Vector3 mapToGame(int x, int y) {
        return mapToGame(new Vector2(x,y));
    }
    
    public Vector3 mapToGame(float x, float y) {
        return mapToGame(new Vector2(x,y));
    }
    
    public Vector3 mapToGame(Vector2 mapPos) {
        return tileSize * (Vector3) (mapPos - new Vector2(w/2f, h/2f));
    }

	public Vector2 gameToMap(Vector3 gamePos) {
        Vector2 mapPos = ((1/tileSize) * ((Vector2) gamePos) + new Vector2(w/2f, h/2f));
		mapPos.x = Mathf.RoundToInt(mapPos.x);
		mapPos.y = Mathf.RoundToInt(mapPos.y);
		return mapPos;
	}
	
	// assumes map coordinates, not game
	public Tile getTile(Vector2 coord) {
		return getTile((int) coord.x, (int) coord.y);
	}
	
	// assumes map coordinates, not game
	public Tile getTile(int x, int y) {
		if (x >= 0 && x < map.Count && y >= 0 && y < map[x].Count) {
			return map[x][y];
		} else {
			return Tile.WALL; // ideally this doesn't come up much. neighbours don't handle it either.
		}
	}
    
    private void setTile(Vector2 coord, Tile value) {
        map[(int) coord.x][(int) coord.y] = value;
	}
	
	public bool isWalkable(Tile tile) {
		// consider buildings water for now since they are where boats spawn
		return tile == Tile.EMPTY;
	}
	
	public bool isWall(Tile tile) {
		// consider buildings water for now since they are where boats spawn
		return tile == Tile.WALL;
	}
	
	public HashSet<Vector2> getNeighbours4(int x, int y) {
        return getNeighbours4(new Vector2(x,y));
    }

    public HashSet<Vector2> getNeighbours4(Vector2 coord) {
        HashSet<Vector2> result = new HashSet<Vector2>();
        if (coord.x > 0) result.Add(new Vector2(coord.x - 1, coord.y));
        if (coord.x < w - 1) result.Add(new Vector2(coord.x + 1, coord.y));
        if (coord.y > 0) result.Add(new Vector2(coord.x, coord.y - 1));
        if (coord.y < h -1) result.Add(new Vector2(coord.x, coord.y + 1));
        return result;
    }

    // include diagonals
    public HashSet<Vector2> getNeighbours8(int x, int y) {
        return getNeighbours8(new Vector2(x,y));
    }

    public HashSet<Vector2> getNeighbours8(Vector2 coord) {
        HashSet<Vector2> result = new HashSet<Vector2>();
        for (int x = Mathf.Max ((int) coord.x - 1, 0); x <= Mathf.Min(w - 1, (int) coord.x + 1); x++) {
            for (int y = Mathf.Max ((int) coord.y - 1, 0); y <= Mathf.Min(h - 1, (int) coord.y + 1); y++) {
                result.Add(new Vector2(x,y));
            }
        }
        result.Remove (coord);
        return result;
    }

	public Player getPlayer() {
		return player;
	}

	public List<Enemy> getEnemies() {
		return enemies;
	}
}
