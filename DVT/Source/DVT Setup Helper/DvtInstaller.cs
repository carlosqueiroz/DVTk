// Part of DvtkHighLevelInterface.dll - .NET class library providing High Level DICOM test capabilities
// Copyright � 2001-2006
// Philips Medical Systems NL B.V., Agfa-Gevaert N.V.



using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;



namespace SetupHelper
{
	/// <summary>
	/// This dll contains functionality needed during the install and uninstall of DVT.
	/// </summary>
	[RunInstaller(true)]
	public class DvtInstaller : System.Configuration.Install.Installer
	{
		//
		// - Generated by Visual Studio -
		//

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion



		//
		// - Constants -
		//

		private const String environmentKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

		private const int HWND_BROADCAST = 0xffff;

		private const int WM_SETTINGCHANGE = 0x001A;

		private const int SMTO_NORMAL = 0x0000;

		private const int SMTO_BLOCK = 0x0001;

		private const int SMTO_ABORTIFHUNG = 0x0002;

		private const int SMTO_NOTIMEOUTIFNOTHUNG = 0x0008;



		//
		// - Imports -
		//

		[DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		[return:MarshalAs(UnmanagedType.Bool)]
		public static extern bool SendMessageTimeout(IntPtr hWnd, int Msg, int wParam, string lParam, int fuFlags, int uTimeout, out int lpdwResult);



		//
		// - Constructors -
		//

		public DvtInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}



		//
		// - Properties -
		//

		/// <summary>
		/// Get the bin directory of DVT.
		/// </summary>
		private String DvtBinDirectory
		{
			get
			{
				String dvtBinDirectory = "";

				String targetDirectory = this.Context.Parameters["targetdir"];

				dvtBinDirectory = Path.Combine(targetDirectory, "bin");

				return(dvtBinDirectory);
			}
		}

		/// <summary>
		/// Get or Set the path enviroonment variable.
		/// </summary>
		private String PathEnvironmentVariable
		{
			get
			{
				String path = "";

				Microsoft.Win32.RegistryKey localMachine = Microsoft.Win32.Registry.LocalMachine;
				Microsoft.Win32.RegistryKey environmentRegistryKey = localMachine.OpenSubKey(environmentKey);
				path = environmentRegistryKey.GetValue("Path").ToString();

				return(path);
			}

			set
			{
				// Set the registry path value.
				Microsoft.Win32.RegistryKey localMachine = Microsoft.Win32.Registry.LocalMachine;
				Microsoft.Win32.RegistryKey environmentRegistryKey = localMachine.OpenSubKey(environmentKey, true);

				environmentRegistryKey.SetValue("Path", value);

				// Make sure the rest of the windows (applications) is notified of this change in the
				// path environment variable. If this method below is not explicitly called, changes
				// will only take effect after rebooting.
				int result;

				SendMessageTimeout(
					(System.IntPtr)HWND_BROADCAST,
					WM_SETTINGCHANGE,
					0,
					"Environment",
					SMTO_BLOCK | SMTO_ABORTIFHUNG | SMTO_NOTIMEOUTIFNOTHUNG,
					5000, 
					out result);
			}
		}



		//
		// - Methods -
		//	

		/// <summary>
		/// This method is called during installation of DVT.
		/// 
		/// This method will make sure that the path environment variable will be extended with
		/// the bin directory of DVT.
		/// </summary>
		/// <param name="stateSaver"></param>
		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);

			// If needed, extend the path environment variable with the bin directory.
			String path = PathEnvironmentVariable;

			if (path.IndexOf(DvtBinDirectory + ";") == -1)
				 // Path to bin directory is not yet included.
			{
				 // Add the bin directory to the path enviroonment variable.
				PathEnvironmentVariable = DvtBinDirectory + ";" + path;
			}
			else
				 // Path to bin directory is already included.
			{
				 // Do nothing.
			}
		}

		/// <summary>
		/// This method is called during uninstallation of DVT.
		/// 
		/// This method will make sure that the the bin directory of DVT will be removed from the 
		/// path environment variable.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			String path = PathEnvironmentVariable;

			if (path.IndexOf(DvtBinDirectory + ";") == -1)
				// Path to bin directory is not included.
			{
				// Do nothing.
			}
			else
				// Path to bin directory is included.
			{
				// Remove the bin directory from the path.
				PathEnvironmentVariable = path.Replace(DvtBinDirectory + ";", "");
			}
		}
	}
}
