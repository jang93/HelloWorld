using UnityEngine;
using System.Collections;

public class GameOverScript : MonoBehaviour {
	private ZombieScript character;

	void Awake() 
	{
		this.character = this.GetComponent<ZombieScript>();
		character.deathFunction = () =>
			{
				Application.LoadLevel(Application.loadedLevelName); 
			};
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
