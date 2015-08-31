#region Using Directives

using System;

#endregion

namespace JadEngine.VFS.Storage.Filters
{
	/// <summary>
	/// Interface that the user controls to configure filters must implement.
    /// �ļ�ϵͳ�洢���� ����(����)���� �������ýӿ�
	/// </summary>
	public interface IJFilterConfigurationUI
	{
		#region Properties

		/// <summary>
		/// Gets or sets the filter configured by this interface
		/// </summary>
		IJFilter Filter { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Configures the filter
		/// </summary>
		void Configure();

		#endregion
	}
}
