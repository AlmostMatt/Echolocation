using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour {
	public Text helpText;
	public CanvasGroup helpGroup;
	public CanvasGroup damagedImage;

	private static HUD singleton;
	private float damageAlpha = 0f;
	private float textDuration = 0f;

	// Use this for initialization
	void Start () {
		singleton = this;
	}
	
	// Update is called once per frame
	void Update () {
		damageAlpha = Mathf.Max(0f, damageAlpha - Time.deltaTime);
		damagedImage.alpha = damageAlpha;

		textDuration -= Time.deltaTime;
		if (textDuration <= 0f) {
			helpGroup.alpha = Mathf.Max(0f, helpGroup.alpha - Time.deltaTime);
		}
	}
	
	public static void takeDamage() {
		singleton.damageAlpha = 1f;
	}
	
	public static void setText(string message, float duration) {
		singleton.helpGroup.alpha = 1f;
		if (singleton.helpText.text == message) {
			singleton.textDuration = duration;
		} else {
			singleton.helpText.text = message;
			singleton.textDuration = duration;
		}
	}
}
