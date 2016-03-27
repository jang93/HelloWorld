using UnityEngine;
using System.Collections;

public class Lava : MonoBehaviour {
	private GameObject player;
	private int damage = 1;
	public bool inLava = true;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
	}
	
	// Update is called once per frame
	void Update () {
		if (inLava) {
			player.gameObject.GetComponent<ZombieScript> ().AdjustHealth (-1 * this.damage);
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == "Player") {
			inLava = true;
		}
	}

	void OnTriggerExit2D (Collider2D other) {
		if (other.tag == "Player") {
			inLava = false;
		}
	}
}
