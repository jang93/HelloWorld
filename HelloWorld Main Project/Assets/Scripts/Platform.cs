using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour {
	private GameObject player;
	private int heal = 1;
	public bool onPlatform = false;
	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
	}
	
	// Update is called once per frame
	void Update () {
		if (onPlatform) {
			player.gameObject.GetComponent<ZombieScript> ().AdjustHealth (1 * this.heal);
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == "Player") {
			onPlatform = true;
		}
	}

	void OnTriggerExit2D (Collider2D other) {
		if (other.tag == "Player") {
			onPlatform = false;
		}
	}
}
