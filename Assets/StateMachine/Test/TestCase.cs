﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Tools;

namespace Game {

public class TestCase : MonoBehaviour {
	
	StateMachine stateMachine = new StateMachine();
	
	// -- //
	
	void Awake() {
		stateMachine.AddState("Idle")
			.To("Jumping", () => Input.GetKeyDown(KeyCode.Space), Jumping_Enter, Jumping_Update, Jumping_Exit)
		.To("Idle", () => Input.GetKeyUp(KeyCode.Space))
			.To("Crouching", () => Input.GetKeyDown(KeyCode.DownArrow), Crouching_Enter, Crouching_Update, Crouching_Exit)
		.To("Idle", () => Input.GetKeyUp(KeyCode.DownArrow));
		;
	}
	
	void Update() {
		stateMachine.Update();
	}
	
	// -- //
	
	void Idle_Enter() {
		Debug.Log("Enter to Idle");
	}
	
	void Idle_Update() {
		Debug.Log("I'm idling :)");
	}
	
	void Idle_Exit() {
		Debug.Log("Exit from Idle");
	}
	
	// -- //
	
	void Jumping_Enter() {
		Debug.Log("JUMP !");
	}
	
	void Jumping_Update() {
		Debug.Log("Lol I'm in the air :]");
	}
	
	void Jumping_Exit() {
		Debug.Log("On the ground");
	}
	
	// -- //
	
	void Crouching_Enter() {
		Debug.Log("Chilling on the ground");
	}
	
	void Crouching_Update() {
		Debug.Log("Still chilling");
	}
	
	void Crouching_Exit() {
		Debug.Log("Enough chill");
	}
	
}

}