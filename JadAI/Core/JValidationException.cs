#region Using Directives

using System;

#endregion

namespace JadEngine.Core
{
	/// <summary>
	/// This exception is raised when a field recives an invalid value (invalid related to the 
	/// program logic). For example: a field that only allows positive integers and recieves a
	/// negative number
	/// </summary>
	public sealed class JValidationException : Exception
	{
		#region Fields

		/// <summary>
		/// The field that recieved the invalid value
		/// </summary>
		private String fieldName;

		/// <summary>
		/// The class where the field is located
		/// </summary>
		private String className;

		/// <summary>
		/// The validation error message
		/// </summary>
		private String errorMessage;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="fieldName">The field that recieved the invalid value</param>
		/// <param name="className">The class where the field is located</param>
		/// <param name="errorMessage">The validation error message</param>
		public JValidationException(String fieldName, String className, String errorMessage)
		{
			this.fieldName = fieldName;
			this.className = className;
			this.errorMessage = errorMessage;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the validation error message
		/// </summary>
		public override string Message
		{
			get
			{
				return "Invalid value assigned to \"" + className + "." + fieldName + "\". The validation error was: " + errorMessage;
			}
		}

		#endregion
	}
}
