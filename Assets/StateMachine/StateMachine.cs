using System.Collections.Generic;
using UnityEngine;

namespace Game.Tools {

/* Delegates */
public delegate bool Condition();
public delegate void EnterState();
public delegate void UpdateState();
public delegate void FixedUpdateState();
public delegate void ExitState();

public class StateOutput {
	
	public Condition condition;
	public State toState;
	
}

public class State {
	
	string name;
	StateMachine stateMachine;
	
	protected EnterState enter;
	protected UpdateState update;
	protected FixedUpdateState fixedUpdate;
	protected ExitState exit;
	
	protected List<StateOutput> outputStates = new List<StateOutput>();
	
	bool firstUpdate = true;
	
	// -- //
	
	protected void Enter() {
		if(firstUpdate) {
			firstUpdate = false;
			
			if(enter != null) {
				enter();
			}
		}
	}
	
	protected void Exit() {
		firstUpdate = true;
		
		if(exit != null) {
			exit();
		}
	}
	
	// -- //
	
	public State(StateMachine stateMachine, string name, EnterState enter, UpdateState update, FixedUpdateState fixedUpdate, ExitState exit) {
		this.stateMachine = stateMachine;
		this.name        = name;
		this.enter       = enter;
		this.update      = update;
		this.fixedUpdate = fixedUpdate;
		this.exit        = exit;
	}
	
	public State To(string name, Condition switchCondition,
		EnterState enter = null, UpdateState update = null,
		FixedUpdateState fixedUpdate = null, ExitState exit = null
	) {
		State stateToAdd = stateMachine.GetState(name);
		
		if(stateToAdd == null) {
			stateToAdd = stateMachine.AddState(name, enter, update, fixedUpdate, exit);
		}
		
		StateOutput output = new StateOutput();
		output.condition   = switchCondition;
		output.toState     = stateToAdd;
		
		outputStates.Add(output);
		
		return output.toState;
	}
	
	public AnyState AnyState(string name, Condition switchCondition,
		EnterState enter = null, UpdateState update = null,
		FixedUpdateState fixedUpdate = null, ExitState exit = null
	) {
		return stateMachine.AnyState(name, switchCondition, enter, update, fixedUpdate, exit);
	}
	
	public State GoTo(string name) {
		return stateMachine.GetState(name);
	}
	
	public State Update(State overrideExit = null) {
		Enter();
		
		if(update != null) {
			update();
		}
		
		if(overrideExit != null) {
			Exit();
			return overrideExit;
		}
		
		for(int i = 0; i < outputStates.Count; ++ i) {
			if(outputStates[i].condition()) {
				Exit();
				return outputStates[i].toState;
			}
		}
		
		return null;
	}
	
	public void FixedUpdate() {
		Enter();
		
		if(fixedUpdate != null) {
			fixedUpdate();
		}
	}
	
}

public class AnyState : State {
	
	public State PreviousState {
		set {
			outputStates[0].toState = value;
		}
	}
	
	// -- //
	
	public AnyState(StateMachine stateMachine, string name, EnterState enter, UpdateState update, FixedUpdateState fixedUpdate, ExitState exit)
		: base(stateMachine, name, enter, update, fixedUpdate, exit)
	{  }
	
	public AnyState Exit(Condition exitCondition) {
		// TODO si Exit est appelé plus d'une fois, il faudrait remplacer la condition déjà présente
		
		// In Update, the first condition to test will be the exit condition
		outputStates.Insert(0, new StateOutput() { condition = exitCondition });
		return this;
	}
	
}

public class StateMachine {
	
	State currentState;
	Dictionary<string, State> states = new Dictionary<string, State>();
	List<StateOutput> anyStates      = new List<StateOutput>();
	
	// -- //
	
	public State AddState(string name, EnterState enter = null,
			UpdateState update = null, FixedUpdateState fixedUpdate = null,
			ExitState exit = null
	) {
		State newState = new State(this, name, enter, update, fixedUpdate, exit);
		
		if(currentState == null) {
			currentState = newState;
		}
		
		states.Add(name, newState);
		
		return newState;
	}
	
	public AnyState AnyState(string name, Condition switchCondition,
			EnterState enter = null, UpdateState update = null,
			FixedUpdateState fixedUpdate = null, ExitState exit = null
	) {
		AnyState stateToAdd = GetState(name) as AnyState;
		
		if(stateToAdd == null) {
			stateToAdd = new AnyState(this, name, enter, update, fixedUpdate, exit);
		}
		
		StateOutput output = new StateOutput();
		output.condition   = switchCondition;
		output.toState     = stateToAdd;
		
		anyStates.Add(output);
		states.Add(name, stateToAdd);
		
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
		State maybeNewState;
		AnyState maybeAnyState = null; 
		
		for(int i = 0; i < anyStates.Count; ++ i) {
			if(anyStates[i].condition()) {
				maybeAnyState = anyStates[i].toState as AnyState; 
				maybeAnyState.PreviousState = currentState;
			}
		}
		
		maybeNewState = currentState.Update(maybeAnyState);
		
		if(maybeNewState != null) {
			currentState = maybeNewState;
		}
	}
	
	public void FixedUpdate() {
		currentState.FixedUpdate();
	}
	
}

}