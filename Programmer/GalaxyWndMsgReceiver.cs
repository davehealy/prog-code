using System;
using System.Windows.Forms;

namespace Programmer
{
	internal class GalaxyWndMsgReceiver : Form
	{
		public event EventHandler OnSetIdFailed;

		public event EventHandler OnSetIdOk;

		protected unsafe override void WndProc(ref Message msg)
		{
			if (((Message)(ref msg)).get_Msg() == 111)
			{
				byte[] array = new byte[20];
				byte* ptr = (byte*)((Message)(ref msg)).get_LParam().ToPointer();
				for (int i = 0; i < 20; i++)
				{
					array[i] = Convert.ToByte(ptr[i]);
				}
				if (array[0] == 3)
				{
					if (array[4] == 65)
					{
						EventHandler onSetIdOk = this.OnSetIdOk;
						if (onSetIdOk != null)
						{
							onSetIdOk(this, null);
						}
					}
					else
					{
						EventHandler onSetIdFailed = this.OnSetIdFailed;
						if (onSetIdFailed != null)
						{
							onSetIdFailed(this, null);
						}
					}
				}
			}
			((Form)this).WndProc(ref msg);
		}
	}
}
