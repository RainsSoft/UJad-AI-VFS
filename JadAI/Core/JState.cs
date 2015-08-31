// Author: Mihai Cozma

#region Using Directives

using System;
using JadEngine.Collections;

#endregion

namespace JadEngine.Core
{
	/// <summary>
	/// State to use with <see cref="JadEngine.Core.JStateMachine"/>
	/// </summary>
	public class JState : IJListable
	{
		#region Fields

		/// <summary>
		/// Name of the state
		/// </summary>
		/// <remarks>
		/// Used for state transitions
		/// </remarks>
		private string name;

		/// <summary>
		/// Reference to the state machine for easly changing states from
		/// another states
		/// </summary>
		private JStateMachine machine;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public JState()
		{
			name = "";
			machine = null;
		}

		/// <summary>
		/// Creates a new state with a name
		/// </summary>
		/// <param name="name">Name of the state</param>
		public JState(string name)
		{
			this.name = name;
			machine = null;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the state ID
		/// </summary>
		/// <remarks>
		/// Useless, always returns 0
		/// </remarks>
		public int ID
		{
			get { return 0; }
		}

		/// <summary>
		/// Gets or sets the name of the state
		/// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sets a reference to the parent state machine
		/// </summary>
		/// <param name="machine">Parents state machine</param>
		public void SetStateMachine(JStateMachine machine)
		{
			this.machine = machine;
		}

		/// <summary>
		/// Called when entering a new state
		/// </summary>
		/// <returns>Auxiliar return value</returns>
		public virtual bool OnEnter()
		{
			return true;
		}

		/// <summary>
		/// Called when running the state
		/// </summary>
		/// <returns>Auxiliar return value</returns>
		public virtual bool OnRun()
		{
			return true;
		}

		/// <summary>
		/// Called when exiting the state
		/// </summary>
		/// <returns>Auxiliar return value</returns>
		public virtual bool OnExit()
		{
			return true;
		}

		#endregion
	}
}
