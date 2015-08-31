// Author: Mihai Cozma

#region Using Directives

using System;

using JadEngine.Collections;

#endregion

namespace JadEngine.Core
{
	/// <summary>
	/// A simple state machine to controlate states in the main application
	/// </summary>
	public class JStateMachine
	{
		#region Fields

		/// <summary>
		/// The current state (current running code)
		/// </summary>
		private JState currentState;

		/// <summary>
		/// All states
		/// </summary>
		private JList<JState> statesList;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public JStateMachine()
		{
			currentState = null;
			statesList = new JList<JState>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Registers a new state
		/// </summary>
		/// <param name="state">State to register</param>
		public void RegisterState(JState state)
		{
			statesList.Add(state);

			//We set a reference to this machine to the state so it will be able
			//to change the current state from within any state
			state.SetStateMachine(this);
		}

		/// <summary>
		/// Changes the current state with a registered state
		/// </summary>
		/// <param name="state">Name of the state</param>
		public bool SetCurrentState(string state)
		{
			//Run exiting code for the current state
			if (currentState != null)
				currentState.OnExit();

			//Set the new current state
			currentState = statesList.FindFirst(state);

			//Run the entering code for the new current state
			if (currentState != null)
				return currentState.OnEnter();

			return false;
		}

		/// <summary>
		/// Runs the current state OnRun method
		/// </summary>
		public void OnRun()
		{
			if (currentState != null)
				currentState.OnRun();
		}

		/// <summary>
		/// Runs the current state OnExit method
		/// </summary>
		public void OnEnd()
		{
			if (currentState != null)
				currentState.OnExit();
		}

		#endregion
	}
}
