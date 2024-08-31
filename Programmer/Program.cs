using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using IQLib;

namespace Programmer
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			if (IntPtr.Size != 4)
			{
				MessageBox.Show("QuizXpress Device Programmer cannot run in 64-bit mode.", "Device Programmer error", (MessageBoxButtons)0, (MessageBoxIcon)16);
				return;
			}
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			try
			{
				Utils.set_UICulture(new CultureInfo(QXEnvironment.GetUILanguage()));
				Thread.CurrentThread.CurrentUICulture = Utils.get_UICulture();
				Thread.CurrentThread.CurrentCulture = new CultureInfo(CultureInfo.CurrentUICulture.Name);
				CultureInfo.DefaultThreadCurrentCulture = Utils.get_UICulture();
				CultureInfo.DefaultThreadCurrentUICulture = Utils.get_UICulture();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			Application.Run((Form)(object)new ProgrammerForm());
		}
	}
}
