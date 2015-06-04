using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour {
	public Text helpText;
	public CanvasGroup helpGroup;
	public CanvasGroup damagedImage;
	
	public Image heart1;
	public Image heart2;
	public Image heart3;

	private static HUD singleton;
	private float damageAlpha = 0f;
	private float textDuration = 0f;
	private static Color darkHeart = new Color(0.4f, 0.4f, 0.4f);

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

	public static void setHealth(float hp) {
		singleton.heart1.color = hp > 0f ? Color.white : darkHeart;
		singleton.heart2.color = hp > 1f ? Color.white : darkHeart;
		singleton.heart3.color = hp > 2f ? Color.white : darkHeart;
	}
	
	public static void takeDamage() {
		singleton.damageAlpha = 1f;
	}
	public static bool hasText() {
		return singleton.textDuration > 0f;
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
