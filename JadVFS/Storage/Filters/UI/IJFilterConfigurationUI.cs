#region Using Directives

using System;

#endregion

namespace JadEngine.VFS.Storage.Filters
{
	/// <summary>
	/// Interface that the user controls to configure filters must implement.
    /// 文件系统存储类型 过滤(加密)类型 界面配置接口
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
