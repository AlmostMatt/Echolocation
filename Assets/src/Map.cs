using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public enum Tile {DIRT=0, SAND=8, STONE=2, WALL=1, SAFE=7};

public class Map : MonoBehaviour {
	private static Dictionary<Tile, Color> TILE_COLORS = new Dictionary<Tile, Color>{
		{Tile.DIRT, new Color(0.275f, 0.129f, 0.082f, 0.25f)},  //reddish
		{Tile.SAND, new Color(0.369f, 0.216f, 0.086f, 0.25f)},  // yellowish
		{Tile.STONE,new Color(0.369f, 0.356f, 0.126f, 0.25f)}, // orangish
		{Tile.SAFE, new Color(0.23f, 0.27f, 0.26f, 0.25f)},     // blueish
		{Tile.WALL, new Color(0f, 0f, 0f, 0.25f)},     // blueish
	};

	public TextAsset tilemap;

	public GameObject wallObj;
	public GameObject playerObj;
	public GameObject enemyObj;
	public GameObject minimapTileObj;

	public Camera minimapCamera;

    // grid size
	private int w = 20;
    private int h = 20;
    private float tileSize = 1f;

	private List<List<Tile>> map;
	private List<List<GameObject>> minimapTiles;
	private Player player;
	private List<Enemy> enemies = new List<Enemy>();

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void  ParseLayer (TileSet tileset, string[] data, int currentLayerID, int width, int height)
	{
		Dictionary<int, GameObject> objectMap = new Dictionary<int, GameObject>() {
			{1, wallObj},
			{4, enemyObj},
			{5, playerObj},
		};
		// Make the map and minimapTile arrays larger if necessary.
		while (map.Count < width) {
			map.Add(new List<Tile>());
			minimapTiles.Add(new List<GameObject>());
		}
		for (int x = 0; x < width; x++) {
			while (map[x].Count < height) {
				map[x].Add(Tile.DIRT);
				minimapTiles[x].Add(null);
			}
		}
		// the world width/height is the largest of the layer widths/heights
		w = Mathf.Max (w,width);
		h = Mathf.Max (h,height);

		int dataIndex = 0;
		//_currentLayerID = _currentLayerID * 10;
		float z = currentLayerID * -10;
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				int dataValue = int.Parse(data [dataIndex].ToString ().Trim ());
				if (Enum.IsDefined(typeof(Tile), dataValue)) {
					map[x][y] = (Tile)dataValue;
				}
				if (objectMap .ContainsKey(dataValue)) {
					GameObject obj = Instantiate(objectMap [dataValue]);
					obj.transform.position = mapToGame(new Vector2(x, y));
					if (obj.GetComponent<Player>() != null) {
						player = obj.GetComponent<Player>();
					}
				}
				dataIndex++;
			}
		}
	}

	private void ParseTilemap()
	{
		/* MODIFIED FROM UNITMX, NOT ORIGINALLY WRITTEN BY ME (some guy names PolCPP)*/
		map = new List<List<Tile>>();
		minimapTiles = new List<List<GameObject>>();

		// We use the currentLayer ID to order them on the Z axis.
		int currentLayerID = 0;

		TileSet tileset = null;		

		// What we're doing here is simple: Load the xml file from the attributes
		// And start parsing. Once it finds a tileset element it creates the tileset object
		// (it will be the first thing it encounters)
		XmlDocument xmldoc = new XmlDocument ();
		xmldoc.Load (new StringReader (tilemap.text)); 
		XmlNodeList nodelist = xmldoc.DocumentElement.ChildNodes;		
		foreach (XmlNode outerNode in nodelist) {
			switch (outerNode.Name) {
			case "tileset":
				// Basically we just grab the data from the xml and build a Tileset object
				// To avoid problems with the collision tileset since we only store one tileset 
				// we ignore anything with Collision inside.  
				if (outerNode.Attributes ["name"].InnerText.IndexOf ("Collision") == -1) {
					XmlNode imageNode = outerNode.SelectSingleNode ("image");
					int firstGID = int.Parse (outerNode.Attributes ["firstgid"].InnerText);
					int width = int.Parse (outerNode.Attributes ["tilewidth"].InnerText);
					int height = int.Parse (outerNode.Attributes ["tileheight"].InnerText);
					int imageWidth = int.Parse (imageNode.Attributes ["width"].InnerText);
					int imageheight = int.Parse (imageNode.Attributes ["height"].InnerText);
					int tileBorder = 0;
					if (outerNode.Attributes ["spacing"] != null)
						tileBorder = int.Parse (outerNode.Attributes ["spacing"].InnerText);						
					tileset = new TileSet (firstGID, width, height, imageWidth, imageheight, tileBorder);	
				}
				break;
			case "layer":
				// First we build the layer object and then just call the renderVertices 
				// renderUV and renderTriangles to build our textured mesh. 
				// Finally we store the vertexcount and +1 to the currentLayerID. 
				// like in the tileset, we avoid using the "collision_" layers since 
				// they only contain collision info
				if (outerNode.Attributes ["name"].InnerText.IndexOf ("collision_") == -1) {
					XmlNode dataNode = outerNode.SelectSingleNode ("data");
					int layerWidth = int.Parse (outerNode.Attributes ["width"].InnerText);
					int layerHeight = int.Parse (outerNode.Attributes ["height"].InnerText);
					string csvData = dataNode.InnerText;
					ParseLayer(tileset, csvData.Split(','), currentLayerID, layerWidth, layerHeight);
				}
				currentLayerID += 1;
				break;
			}
		}
		// Only populate minimap tiles after all of the layers of the map have been parsed.
		for (int x = -1; x <= w; x++) {
			for (int y = -1; y <= h; y++) {
				GameObject minimapTile = Instantiate(minimapTileObj);
				minimapTile.transform.position = new Vector2(x, y+1);
				// Put a gray border around the minimap
				Color col = new Color(0.5f, 0.5f, 0.5f, 1f);
				if (x > -1 && y > -1 && x < w && y < h) {
					minimapTile.active = false;
					col = getTileColor(getTile(x, y));
					col.a = 1f;
					minimapTiles[x][y] = minimapTile;
				}
				// the minimap tile will only be activated once an echo particle reaches it.
				minimapTile.GetComponent<SpriteRenderer>().color = col;
			}
		}
	}

	public void generateMap() {
		// instantiate level object,
		// get player/enemy/wall objects in the scene

		enemies.Clear();
		ParseTilemap();

		
		GameObject[] enemyList = GameObject.FindGameObjectsWithTag("Enemy");
		for (int i=0; i<enemyList.Length; ++i) {
			enemies.Add(enemyList[i].GetComponent<Enemy>());
		}

		GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
		foreach (GameObject wall in walls) {
			Transform t = wall.transform;
			for (int x=0; x<t.localScale.x; ++x) {
				for (int y =0; y<t.localScale.y; ++y) {
					// ??? Why do I have this loop
				}
			}
		}

		//minimapCamera.aspect = 1f;
		minimapCamera.orthographicSize = (Mathf.Max(w,h)+2)/2;
		minimapCamera.rect = new Rect(0.70f, 0.55f, 0.25f, 0.4f);
		minimapCamera.transform.position = new Vector3(w/2, h/2, -20);

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
		if (x >= 0 && x < w && y >= 0 && y < h) {
			return map[x][y];
		} else {
			return Tile.WALL; // ideally this doesn't come up much. neighbours don't handle it either.
		}
	}

	// assumes map coordinates, not game
	public GameObject getMinimapTile(Vector2 coord) {
		int x = (int) coord.x;
		int y = (int) coord.y;
		if (x >= 0 && x < w && y >= 0 && y < h) {
			return minimapTiles[x][y];
		}
		return null;
	}

	public bool isWalkable(Tile tile) {
		// consider buildings water for now since they are where boats spawn
		return tile != Tile.WALL;
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

	// grid coordinates, uses bresenham
	public bool raycast(Vector2 t1, Vector2 t2, Tile hitTile) {
		if (t1.x > t2.x) {
			return raycast (t2, t1, hitTile);
		}
		int x1 = (int) t1.x, x2 = (int) t2.x;
		int y1 = (int) t1.y, y2 = (int) t2.y;
		int y;
		if (x1 == x2) {
			for (y = Mathf.Min (y1, y2); y<=Mathf.Max (y1,y2);++y) {
				if (getTile(x1,y) == hitTile) {
					return true;
				}
			}
			return false;
		}
		float dx = x2-x1, dy=y2-y1;
		float yoffset = 0f;
		float deltaoffset = Mathf.Abs (dy / dx);
		y = y1;
		for (int x = x1; x <= x2; ++x) {
			if (getTile(x,y) == hitTile) {
				return true;
			}
			yoffset += deltaoffset;
			while (yoffset >= 0.5f) {
				if (getTile(x,y) == hitTile) {
					return true;
				}
				y += (int) Mathf.Sign(y2 - y1);
				yoffset -= 1f;
			}
		}
		return false;
	}
	
	public static Color getTileColor(Tile t) {
		return TILE_COLORS[t];
	}
}
