using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Programmer.Properties;

namespace Programmer
{
	internal class GalaxyWristbandsProgrammer : IProgrammer
	{
		private GalaxyWndMsgReceiver _receiver;

		public event OnKeypadPressedHandler OnKeyPressed
		{
			add
			{
			}
			remove
			{
			}
		}

		public event OnQuizMasterRemotePressedHandler OnQuizMasterRemotePressed
		{
			add
			{
			}
			remove
			{
			}
		}

		public event SetIDSucceededHandler OnSetIDSucceeded;

		public event SetIDFailedHandler OnSetIDFailed;

		[DllImport("Rf301_Com.dll")]
		public static extern bool Set_Hnd_MsgNo(IntPtr AppH, IntPtr WndH, int MsgNo);

		[DllImport("Rf301_Com.dll")]
		public static extern bool CloseHnd();

		[DllImport("RF301_com.dll")]
		public static extern int Connect(byte CommPort, int MaxID, byte PortType, int Sn1, int Sn2, int Sn3, int Sn4, int Sn5, int Sn6, int Sn7, int Sn8, byte ReceiveId);

		[DllImport("RF301_com.dll")]
		public static extern int DisConnect(byte CNo);

		[DllImport("RF301_com.dll")]
		public static extern int Open_Base(byte mBase);

		[DllImport("RF301_com.dll")]
		public static extern int Close_Base(byte mBase);

		[DllImport("RF301_com.dll")]
		public static extern int Stop_Base(byte mBase);

		[DllImport("RF301_com.dll")]
		public static extern int Set_Id_301(byte CommPort, int LedId, byte Group, byte Row, byte Col);

		public bool Activate()
		{
			return true;
		}

		public void AfterSetID()
		{
		}

		public bool CanAutoProgram()
		{
			return false;
		}

		public bool CanProgramRemote()
		{
			return false;
		}

		public bool CanReceive()
		{
			return false;
		}

		public bool Connect(Form owningForm, int max, int min)
		{
			if (_receiver == null)
			{
				_receiver = new GalaxyWndMsgReceiver();
				_receiver.OnSetIdFailed += _receiver_OnSetIdFailed;
				_receiver.OnSetIdOk += _receiver_OnSetIdOk;
			}
			if (!Set_Hnd_MsgNo(((Control)owningForm).get_Handle(), ((Control)_receiver).get_Handle(), 111))
			{
				return false;
			}
			if (Connect(0, max, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) == 1)
			{
				Thread.Sleep(1000);
				Close_Base(0);
				Open_Base(0);
				return true;
			}
			return false;
		}

		private void _receiver_OnSetIdOk(object sender, EventArgs e)
		{
			SetIDSucceededHandler onSetIDSucceeded = this.OnSetIDSucceeded;
			if (onSetIDSucceeded != null)
			{
				onSetIDSucceeded();
			}
		}

		private void _receiver_OnSetIdFailed(object sender, EventArgs e)
		{
			SetIDFailedHandler onSetIDFailed = this.OnSetIDFailed;
			if (onSetIDFailed != null)
			{
				onSetIDFailed();
			}
		}

		public void Deactivate()
		{
		}

		public void Disconnect()
		{
			Stop_Base(0);
			Close_Base(0);
			DisConnect(0);
		}

		public Bitmap GetAutoProgramConfirmPicture()
		{
			return null;
		}

		public Bitmap GetDisabledNeutralPicture()
		{
			return null;
		}

		public void GetDisplayStrings(out string indicatorGroupBox, out string manualGroupBox, out string manualLabel, out string manualButton, out string autoGroupBox, out string autoLabel)
		{
			indicatorGroupBox = Resources.IndicatorGroupBoxKeypad;
			manualGroupBox = Resources.ManualGroupBoxKeypad;
			manualLabel = Resources.ManualLabelWristband;
			manualButton = Resources.ManualButtonWristband;
			autoGroupBox = Resources.AutoGroupBoxKeypad;
			autoLabel = Resources.AutoLabelKeypad;
		}

		public Bitmap GetNeutralPicture()
		{
			return Resources.wristbands_neutral;
		}

		public Bitmap GetSetIdPicture()
		{
			return Resources.wristbands_programming;
		}

		public int MaxKeypads()
		{
			return 3840;
		}

		public int ProgramTimeoutMS()
		{
			return 16000;
		}

		public bool SetID(int id)
		{
			return Set_Id_301(0, id - 1, 0, 0, 0) == 1;
		}

		public void SetParameter(string method, object value)
		{
		}

		public bool SetRemote()
		{
			return false;
		}

		public void StartPolling()
		{
		}
	}
}
