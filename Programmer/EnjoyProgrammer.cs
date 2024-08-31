using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Programmer.Properties;

namespace Programmer
{
	internal class EnjoyProgrammer : IProgrammer
	{
		private WndMsgReceiver _receiver = new WndMsgReceiver();

		public int Port { get; set; }

		public int MinKeypad { get; set; }

		public int MaxKeypad { get; set; }

		public bool Connected
		{
			get
			{
				return Port != -1;
			}
		}

		public event OnKeypadPressedHandler OnKeyPressed;

		public event OnQuizMasterRemotePressedHandler OnQuizMasterRemotePressed;

		public event SetIDSucceededHandler OnSetIDSucceeded;

		public event SetIDFailedHandler OnSetIDFailed;

		[DllImport("RF217_Com.dll")]
		public static extern bool Set_Hnd_MsgNo(IntPtr AppH, IntPtr WndH, int MsgNo);

		[DllImport("RF217_Com.dll")]
		public static extern bool CloseHnd();

		[DllImport("RF217_Com.dll")]
		public static extern int Activate_Receiver(byte CommPort, byte mMode, byte mType);

		[DllImport("RF217_Com.dll")]
		public static extern int Connect(byte CommPort, int MaxID, int MinID, byte PortType, int Sn1, int Sn2, int Sn3, int Sn4, int Sn5, int Sn6, int Sn7, int Sn8);

		[DllImport("RF217_Com.dll")]
		public static extern int DisConnect(int CommPort);

		[DllImport("RF217_Com.dll")]
		public static extern int Stop_Receiver(byte CommPort);

		[DllImport("RF217_Com.dll")]
		public static extern int Set_ID(byte CommPort, int RemoteID);

		public EnjoyProgrammer()
		{
			Port = -1;
			MinKeypad = 0;
			MaxKeypad = 0;
		}

		public void SetParameter(string method, object value)
		{
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

		public void CleanUp()
		{
			try
			{
				Thread.Sleep(1000);
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
			MinKeypad = min;
			MaxKeypad = max;
			if (!Set_Hnd_MsgNo(((Control)owningForm).get_Handle(), ((Control)_receiver).get_Handle(), 111))
			{
				return false;
			}
			if (Connect(Convert.ToByte(0), max, min, 0, 0, 0, 0, 0, 0, 0, 0, 0) != 0)
			{
				Port = 0;
			}
			else
			{
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
				foreach (string item in list)
				{
					try
					{
						Thread.Sleep(1);
						Application.DoEvents();
						char[] trimChars = new char[3] { 'C', 'O', 'M' };
						int num = int.Parse(item.TrimStart(trimChars));
						if (num != 1)
						{
							if (Connect(Convert.ToByte(num), max, min, 1, num, 0, 0, 0, 0, 0, 0, 0) != 0)
							{
								Port = num;
								break;
							}
							Port = -1;
						}
					}
					catch
					{
						Port = -1;
					}
				}
			}
			if (Port == -1)
			{
				CleanUp();
				return false;
			}
			Thread.Sleep(1000);
			if (Activate_Receiver(Convert.ToByte(Port), Convert.ToByte(0), Convert.ToByte(0)) == 0)
			{
				return false;
			}
			_receiver.OnKeyPressed += _receiver_OnKeyPressed;
			_receiver.OnQuizMasterRemotePressed += _receiver_OnQuizMasterRemotePressed;
			_receiver.OnSetIDSucceeded += _receiver_OnSetIDSucceeded;
			return true;
		}

		private void _receiver_OnSetIDSucceeded()
		{
			if (this.OnSetIDSucceeded != null)
			{
				this.OnSetIDSucceeded();
			}
		}

		private void _receiver_OnQuizMasterRemotePressed(WndMsgReceiver.QuizMasterRemoteCommands arg)
		{
			if (this.OnQuizMasterRemotePressed != null)
			{
				QuizMasterRemoteCommands arg2 = QuizMasterRemoteCommands.None;
				switch (arg)
				{
				case WndMsgReceiver.QuizMasterRemoteCommands.Power:
					arg2 = QuizMasterRemoteCommands.Power;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.Score:
					arg2 = QuizMasterRemoteCommands.Score;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.Chart:
					arg2 = QuizMasterRemoteCommands.Chart;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.Mode:
					arg2 = QuizMasterRemoteCommands.Mode;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.QuestionMark:
					arg2 = QuizMasterRemoteCommands.QuestionMark;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.Up:
					arg2 = QuizMasterRemoteCommands.Up;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.Down:
					arg2 = QuizMasterRemoteCommands.Down;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.Begin:
					arg2 = QuizMasterRemoteCommands.Begin;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.Stop:
					arg2 = QuizMasterRemoteCommands.Stop;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.Ok:
					arg2 = QuizMasterRemoteCommands.Ok;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.F1:
					arg2 = QuizMasterRemoteCommands.F1;
					break;
				case WndMsgReceiver.QuizMasterRemoteCommands.F2:
					arg2 = QuizMasterRemoteCommands.F2;
					break;
				}
				this.OnQuizMasterRemotePressed(arg2);
			}
		}

		private void _receiver_OnKeyPressed(int nKeyPad, int key)
		{
			if (this.OnKeyPressed != null)
			{
				switch (key)
				{
				case 4:
					key = 3;
					break;
				case 8:
					key = 4;
					break;
				case 16:
					key = 5;
					break;
				case 32:
					key = 6;
					break;
				case 251:
					key = 0;
					break;
				}
				this.OnKeyPressed(nKeyPad, key);
			}
		}

		public void Disconnect()
		{
			if (Connected)
			{
				Stop_Receiver(Convert.ToByte(Port));
				Port = -1;
			}
		}

		public bool SetID(int id)
		{
			if (Set_ID(Convert.ToByte(Port), id) == 0)
			{
				return false;
			}
			return true;
		}

		public void Deactivate()
		{
			if (Port >= 0)
			{
				Stop_Receiver(Convert.ToByte(Port));
			}
		}

		public void StartPolling()
		{
		}

		public Bitmap GetSetIdPicture()
		{
			return Resources.keypad_press;
		}

		public Bitmap GetNeutralPicture()
		{
			return Resources.keypad;
		}

		public Bitmap GetDisabledNeutralPicture()
		{
			return Resources.keypad_grey;
		}

		public Bitmap GetAutoProgramConfirmPicture()
		{
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

		public bool Activate()
		{
			return Activate_Receiver(Convert.ToByte(Port), Convert.ToByte(0), Convert.ToByte(0)) == 0;
		}

		public void AfterSetID()
		{
		}

		public int ProgramTimeoutMS()
		{
			return 4000;
		}

		public int MaxKeypads()
		{
			return 400;
		}

		public bool SetRemote()
		{
			return SetID(1);
		}
	}
}
