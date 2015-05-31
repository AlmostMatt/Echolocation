using UnityEngine;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public enum Tile {DIRT=0, SAND=8, STONE=2, WALL=1, SAFE=7};

public class Map : MonoBehaviour {
	public TextAsset tilemap;

	public GameObject wallObj;
	public GameObject playerObj;
	public GameObject enemyObj;
	public GameObject emitterObj;

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

	private void  ParseLayer (TileSet tileset, string[] data, int currentLayerID, int width, int height)
	{
		while (map.Count <= width) {
			map.Add(new List<Tile>());
		}
		// assuming all layers have the same size
		w = Mathf.Max (w,width);
		h = Mathf.Max (h,height);

		int dataIndex = 0;
		//_currentLayerID = _currentLayerID * 10;
		float z = currentLayerID * -10;
		for (int i = 1; i <= height; i++) {
			for (int j = 1; j <= width; j++) {
				while (map[j].Count <= height) {
					map[j].Add(Tile.DIRT);
				}
				Vector2 tilePos = new Vector2(j, i);
				int dataValue = int.Parse(data [dataIndex].ToString ().Trim ());
				if (dataValue == 7) {
					setTile(tilePos, Tile.SAFE);
				} else if (dataValue == 2) {
					setTile(tilePos, Tile.STONE);
				} else if (dataValue == 8) {
					setTile(tilePos, Tile.SAND);
				} else if (   dataValue == 1
				           || dataValue == 5
				           || dataValue == 4
				           ) {
					GameObject objectType = wallObj;
					switch (dataValue) {
					case 1:
						objectType = wallObj;
						setTile(tilePos, Tile.WALL);
						break;
					case 4:
						objectType = enemyObj;
						break;
					case 5:
						objectType = playerObj;
						break;
					}
					GameObject obj = Instantiate(objectType);
					obj.transform.position = mapToGame(tilePos);
					if (dataValue == 5) {
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
	}

	public void generateMap() {
		// instantiate level object,
		// get player/enemy/wall objects in the scene

		level = levels[0];
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

}
