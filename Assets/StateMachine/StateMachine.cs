using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tools {

/* Delegates */
public delegate bool Condition();
public delegate void EnterState();
public delegate void UpdateState();
public delegate void ExitState();

public class StateOutput {
	
	public Condition condition;
	public State toState;
	
}

public class State {
	
	string name;
	StateMachine stateMachine;
	
	EnterState enter;
	UpdateState update;
	ExitState exit;
	
	List<StateOutput> outputStates = new List<StateOutput>();
	
	bool firstUpdate = true;
	
	// -- //
	
	public State(StateMachine stateMachine, string name, EnterState enter, UpdateState update, ExitState exit) {
		this.stateMachine = stateMachine;
		this.name   = name;
		this.enter  = enter;
		this.update = update;
		this.exit   = exit;
	}
	
	public State To(string name, Condition switchCondition, EnterState enter = null, UpdateState update = null, ExitState exit = null) {
		State stateToAdd = stateMachine.GetState(name);
		
		if(stateToAdd == null) {
			stateToAdd = stateMachine.AddState(name, enter, update, exit);
		}
		
		StateOutput output = new StateOutput();
		output.condition   = switchCondition;
		output.toState     = stateToAdd;
		
		outputStates.Add(output);
		
		return output.toState;
	}
	
	public State AnyState(string name, Condition switchCondition, EnterState enter = null, UpdateState update = null, ExitState exit = null) {
		return stateMachine.AnyState(name, switchCondition, enter, update, exit);
	}
	
	public State GoTo(string name) {
		return stateMachine.GetState(name);
	}
	
	public State Update() {
		if(firstUpdate) {
			if(enter != null) {
				enter();
			}
			
			firstUpdate = false;
		}
		
		if(update != null) {
			update();
		}
		
		for(int i = 0; i < outputStates.Count; ++ i) {
			if(outputStates[i].condition()) {
				firstUpdate = true;
				
				if(exit != null) {
					exit();
				}
				
				return outputStates[i].toState;
			}
		}
		
		return null;
	}
	
}

public class StateMachine {
	
	State currentState;
	Dictionary<string, State> states = new Dictionary<string, State>();
	List<StateOutput> anyStates      = new List<StateOutput>();
	
	// -- //
	
	public State AddState(string name, EnterState enter = null, UpdateState update = null, ExitState exit = null) {
		State newState = new State(this, name, enter, update, exit);
		
		if(currentState == null) {
			currentState = newState;
		}
		
		states.Add(name, newState);
		
		return newState;
	}
	
	public State AnyState(string name, Condition switchCondition, EnterState enter = null, UpdateState update = null, ExitState exit = null) {
		State stateToAdd = GetState(name);
		
		if(stateToAdd == null) {
			stateToAdd = AddState(name, enter, update, exit);
		}
		
		StateOutput output = new StateOutput();
		output.condition   = switchCondition;
		output.toState     = stateToAdd;
		
		anyStates.Add(output);
		
		return stateToAdd;
	}
	
	public State GetState(string name) {
		State maybeTheState;
		
		if(states.TryGetValue(name, out maybeTheState)) {
			return maybeTheState;
		}
		
		return null;
	}
	
	public void Update() {
		// TODO check les Any States
		
		State maybeNewState = currentState.Update();
		
		if(maybeNewState != null) {
			currentState = maybeNewState;
		}
	}
	
}

}