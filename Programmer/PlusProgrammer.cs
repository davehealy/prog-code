using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Programmer.Properties;

namespace Programmer
{
	internal class PlusProgrammer : IProgrammer
	{
		private PlusWndMsgReceiver _receiver;

		private int _port = -1;

		private int _minId;

		private int _maxId;

		public bool Is237 { get; set; }

		public event OnKeypadPressedHandler OnKeyPressed;

		public event OnQuizMasterRemotePressedHandler OnQuizMasterRemotePressed;

		public event SetIDSucceededHandler OnSetIDSucceeded;

		public event SetIDFailedHandler OnSetIDFailed;

		[DllImport("HidRf317Com.dll")]
		public static extern bool Set_Hnd_MsgNo(IntPtr AppH, IntPtr WndH, int MsgNo);

		[DllImport("HidRf317Com.dll")]
		public static extern bool CloseHnd();

		[DllImport("HidRf317Com.dll")]
		public static extern int Activate_Receiver(byte CommPort, byte mQuType, byte mQuPara, byte IsItemNumber);

		[DllImport("HidRf317Com.dll")]
		public static extern int Connect(byte CommPort, int MaxID, int MinID, byte PortType, int Sn1, int Sn2, int Sn3, int Sn4, int Sn5, int Sn6, int Sn7, int Sn8, byte ReceiveId);

		[DllImport("HidRf317Com.dll")]
		public static extern int DisConnect(byte CommPort);

		[DllImport("HidRf317Com.dll")]
		public static extern int Open_Receiver(byte CommPort);

		[DllImport("HidRf317Com.dll")]
		public static extern int Stop_Receiver(byte CommPort);

		[DllImport("HidRf317Com.dll")]
		public static extern int Set_ID(byte CommPort, int RemoteID);

		[DllImport("HidRf317Com.dll")]
		public static extern int Set_Teacher(byte CommPort);

		public void SetParameter(string method, object value)
		{
		}

		public void Cleanup()
		{
			try
			{
				Stop_Receiver(Convert.ToByte(0));
				Thread.Sleep(200);
				DisConnect(0);
			}
			catch
			{
			}
		}

		public bool Connect(Form owningForm, int max, int min)
		{
			if (_receiver == null)
			{
				_receiver = new PlusWndMsgReceiver();
			}
			if (!Set_Hnd_MsgNo(((Control)owningForm).get_Handle(), ((Control)_receiver).get_Handle(), 111))
			{
				return false;
			}
			_maxId = max;
			_minId = min;
			if (Connect(0, _maxId, _minId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) != 0)
			{
				_port = 0;
			}
			else
			{
				string[] array = COMPorts();
				foreach (string text in array)
				{
					try
					{
						int num = int.Parse(text.TrimStart("COM".ToCharArray()));
						if (num != 1)
						{
							if (Connect(Convert.ToByte(num), _maxId, _minId, 1, num, 0, 0, 0, 0, 0, 0, 0, 0) != 0)
							{
								_port = num;
								break;
							}
							_port = -1;
						}
					}
					catch
					{
						_port = -1;
					}
				}
			}
			if (_port == -1)
			{
				Cleanup();
				return false;
			}
			Thread.Sleep(1000);
			if (Activate_Receiver(Convert.ToByte(_port), Convert.ToByte(0), Convert.ToByte(8), Convert.ToByte(0)) == 0)
			{
				return false;
			}
			_receiver.OnKeyPressed += OnKeypadDataReceived;
			_receiver.OnQuizMasterRemotePressed += OnQuizMasterRemoteDataReceived;
			_receiver.OnSetIDSucceeded += _receiver_OnSetIDSucceeded;
			_receiver.OnSetIDFailed += _receiver_OnSetIDFailed;
			return true;
		}

		private void _receiver_OnSetIDFailed()
		{
			try
			{
				SetIDFailedHandler onSetIDFailed = this.OnSetIDFailed;
				if (onSetIDFailed != null)
				{
					onSetIDFailed();
				}
			}
			catch
			{
			}
		}

		private void _receiver_OnSetIDSucceeded()
		{
			try
			{
				SetIDSucceededHandler onSetIDSucceeded = this.OnSetIDSucceeded;
				if (onSetIDSucceeded != null)
				{
					onSetIDSucceeded();
				}
			}
			catch
			{
			}
		}

		public void Disconnect()
		{
			if (_port >= 0)
			{
				Stop_Receiver(Convert.ToByte(_port));
				DisConnect(Convert.ToByte(_port));
			}
		}

		public bool SetID(int id)
		{
			if (Set_ID(Convert.ToByte(_port), id) != 1)
			{
				return false;
			}
			return true;
		}

		public void Deactivate()
		{
			if (_port >= 0)
			{
				Stop_Receiver(Convert.ToByte(_port));
			}
		}

		public void AfterSetID()
		{
			Open_Receiver(Convert.ToByte(_port));
		}

		public bool Activate()
		{
			if (Activate_Receiver(Convert.ToByte(_port), Convert.ToByte(0), Convert.ToByte(8), 0) == 0)
			{
				return false;
			}
			return true;
		}

		public int ProgramTimeoutMS()
		{
			return 5000;
		}

		public int MaxKeypads()
		{
			return 2000;
		}

		public bool SetRemote()
		{
			return Set_Teacher(Convert.ToByte(_port)) == 1;
		}

		public void StartPolling()
		{
		}

		public Bitmap GetSetIdPicture()
		{
			if (!Is237)
			{
				return Resources.keypad_press_plus;
			}
			return Resources.keypad_press;
		}

		public Bitmap GetNeutralPicture()
		{
			if (!Is237)
			{
				return Resources.keypad_plus;
			}
			return Resources.keypad;
		}

		public Bitmap GetDisabledNeutralPicture()
		{
			return Resources.keypad_grey;
		}

		public Bitmap GetAutoProgramConfirmPicture()
		{
			if (!Is237)
			{
				return Resources.keypad_single_plus;
			}
			return Resources.keypad_single;
		}

		public void GetDisplayStrings(out string indicatorGroupBox, out string manualGroupBox, out string manualLabel, out string manualButton, out string autoGroupBox, out string autoLabel)
		{
			indicatorGroupBox = Resources.IndicatorGroupBoxKeypad;
			manualGroupBox = Resources.ManualGroupBoxKeypad;
			manualLabel = Resources.ManualLabelKeypad;
			manualButton = Resources.ManualButtonKeypad;
			autoGroupBox = Resources.AutoGroupBoxKeypad;
			autoLabel = Resources.AutoLabelKeypad;
		}

		public bool CanAutoProgram()
		{
			return true;
		}

		public bool CanProgramRemote()
		{
			return true;
		}

		public bool CanReceive()
		{
			return true;
		}

		private void OnKeypadDataReceived(int keypadId, string key, string timeMs)
		{
			try
			{
				if (this.OnKeyPressed != null)
				{
					int key2 = 0;
					switch (key)
					{
					case "G":
						key2 = 0;
						break;
					case "A":
						key2 = 1;
						break;
					case "B":
						key2 = 2;
						break;
					case "C":
						key2 = 3;
						break;
					case "D":
						key2 = 4;
						break;
					case "E":
						key2 = 5;
						break;
					case "F":
						key2 = 6;
						break;
					}
					this.OnKeyPressed(keypadId, key2);
				}
			}
			catch
			{
			}
		}

		private void OnQuizMasterRemoteDataReceived(QuizMasterRemoteCommands arg)
		{
			try
			{
				if (this.OnQuizMasterRemotePressed != null)
				{
					this.OnQuizMasterRemotePressed(arg);
				}
			}
			catch
			{
			}
		}

		public string[] COMPorts()
		{
			string name = "SOFTWARE\\Game Show Crew\\QuizXpress\\";
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(name, true);
			if (registryKey != null && registryKey.GetValue("HasSerialCable") == null)
			{
				return new string[0];
			}
			string[] portNames = SerialPort.GetPortNames();
			List<string> list = new List<string>();
			Array.Sort(portNames);
			string text = null;
			for (int i = 0; i < portNames.Length; i++)
			{
				if (!(text == portNames[i]))
				{
					text = portNames[i];
					list.Add(portNames[i]);
				}
			}
			return list.ToArray();
		}
	}
}
