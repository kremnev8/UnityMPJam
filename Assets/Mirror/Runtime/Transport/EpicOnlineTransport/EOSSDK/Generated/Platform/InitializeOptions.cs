// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Platform
{
	/// <summary>
	/// Options for initializing the Epic Online Services SDK.
	/// </summary>
	public class InitializeOptions
	{
		/// <summary>
		/// A custom memory allocator, if desired.
		/// </summary>
		public System.IntPtr AllocateMemoryFunction { get; set; }

		/// <summary>
		/// A corresponding memory reallocator. If the AllocateMemoryFunction is nulled, then this field must also be nulled.
		/// </summary>
		public System.IntPtr ReallocateMemoryFunction { get; set; }

		/// <summary>
		/// A corresponding memory releaser. If the AllocateMemoryFunction is nulled, then this field must also be nulled.
		/// </summary>
		public System.IntPtr ReleaseMemoryFunction { get; set; }

		/// <summary>
		/// The name of the product using the Epic Online Services SDK.
		/// 
		/// The name string is required to be non-empty and at maximum of 64 characters long.
		/// The string buffer can consist of the following characters:
		/// A-Z, a-z, 0-9, dot, underscore, space, exclamation mark, question mark, and sign, hyphen, parenthesis, plus, minus, colon.
		/// </summary>
		public string ProductName { get; set; }

		/// <summary>
		/// Product version of the running application.
		/// 
		/// The name string has same requirements as the ProductName string.
		/// </summary>
		public string ProductVersion { get; set; }

		/// <summary>
		/// This field is for system specific initialization if any.
		/// 
		/// If provided then the structure will be located in <System>/eos_<system>.h.
		/// The structure will be named EOS_<System>_InitializeOptions.
		/// </summary>
		public System.IntPtr SystemInitializeOptions { get; set; }

		/// <summary>
		/// The thread affinity override values for each category of thread.
		/// </summary>
		public InitializeThreadAffinity OverrideThreadAffinity { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct InitializeOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_AllocateMemoryFunction;
		private System.IntPtr m_ReallocateMemoryFunction;
		private System.IntPtr m_ReleaseMemoryFunction;
		private System.IntPtr m_ProductName;
		private System.IntPtr m_ProductVersion;
		private System.IntPtr m_Reserved;
		private System.IntPtr m_SystemInitializeOptions;
		private System.IntPtr m_OverrideThreadAffinity;

		public System.IntPtr AllocateMemoryFunction
		{
			set
			{
				m_AllocateMemoryFunction = value;
			}
		}

		public System.IntPtr ReallocateMemoryFunction
		{
			set
			{
				m_ReallocateMemoryFunction = value;
			}
		}

		public System.IntPtr ReleaseMemoryFunction
		{
			set
			{
				m_ReleaseMemoryFunction = value;
			}
		}

		public string ProductName
		{
			set
			{
				Helper.TryMarshalSet(ref m_ProductName, value);
			}
		}

		public string ProductVersion
		{
			set
			{
				Helper.TryMarshalSet(ref m_ProductVersion, value);
			}
		}

		public System.IntPtr SystemInitializeOptions
		{
			set
			{
				m_SystemInitializeOptions = value;
			}
		}

		public InitializeThreadAffinity OverrideThreadAffinity
		{
			set
			{
				Helper.TryMarshalSet<InitializeThreadAffinityInternal, InitializeThreadAffinity>(ref m_OverrideThreadAffinity, value);
			}
		}

		public void Set(InitializeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = PlatformInterface.InitializeApiLatest;
				AllocateMemoryFunction = other.AllocateMemoryFunction;
				ReallocateMemoryFunction = other.ReallocateMemoryFunction;
				ReleaseMemoryFunction = other.ReleaseMemoryFunction;
				ProductName = other.ProductName;
				ProductVersion = other.ProductVersion;
				int[] reservedData = new int[] { 1, 1 };
				System.IntPtr reservedDataAddress = System.IntPtr.Zero;
				Helper.TryMarshalSet(ref reservedDataAddress, reservedData);
				m_Reserved = reservedDataAddress;
				SystemInitializeOptions = other.SystemInitializeOptions;
				OverrideThreadAffinity = other.OverrideThreadAffinity;
			}
		}

		public void Set(object other)
		{
			Set(other as InitializeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AllocateMemoryFunction);
			Helper.TryMarshalDispose(ref m_ReallocateMemoryFunction);
			Helper.TryMarshalDispose(ref m_ReleaseMemoryFunction);
			Helper.TryMarshalDispose(ref m_ProductName);
			Helper.TryMarshalDispose(ref m_ProductVersion);
			Helper.TryMarshalDispose(ref m_Reserved);
			Helper.TryMarshalDispose(ref m_SystemInitializeOptions);
			Helper.TryMarshalDispose(ref m_OverrideThreadAffinity);
		}
	}
}