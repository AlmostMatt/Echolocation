using UnityEngine;
using System.Collections;

public class Emitter : MonoBehaviour, Actor {
	private ActionMap actionMap;

	// Use this for initialization
	void Start () {
		actionMap = new ActionMap(this);
		actionMap.add(0, new Ability(3f));
	}
	
	// Update is called once per frame
	void Update () {
		actionMap.update(Time.deltaTime);
		if (actionMap.ready(0)) {
			actionMap.use(0, null);
			//Scene.echo(transform.position, 4f, 2f, 32);
		}
	}
}
