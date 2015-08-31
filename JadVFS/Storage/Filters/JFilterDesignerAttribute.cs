#region Using Directives

using System;

#endregion

namespace JadEngine.VFS.Storage.Filters
{
	/// <summary>
	/// Allows you to specify the visual designer of a filter
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class JFilterDesignerAttribute : Attribute
	{
		#region Fields

		private Type designerType;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the type of the designer
		/// </summary>
		public Type DesignerType
		{
			get { return designerType; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes the <see cref="JFilterDesignerAttribute"/>
        /// </summary>
        /// <param name="designerType">Type of the designer to work with the filter</param>
        public JFilterDesignerAttribute(Type designerType)
            : base()
        {
            this.designerType = designerType;
		}

		#endregion
	}
}
