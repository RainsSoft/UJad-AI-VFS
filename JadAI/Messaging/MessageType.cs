#region Using Directives

using System;

#endregion

namespace JadEngine.AI.Messaging
{
	/// <summary>
	/// The type of the message
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// Default message type
		/// </summary>
		DefaultMessage,

		#region Pathfinding Message Types

		/// <summary>
		/// Message indicating that a path was found
		/// </summary>
		PathReady,

		/// <summary>
		/// Message indicating that a path couldn´t be found
		/// </summary>
		PathNotAvailable,

		#endregion
	}
}
