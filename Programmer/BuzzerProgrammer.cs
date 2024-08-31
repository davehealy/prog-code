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
	internal class BuzzerProgrammer : IProgrammer
	{
		public enum Mode
		{
			Buzzer,
			Keypad
		}

		private readonly BuzzerWndMsgReceiver _receiver = new BuzzerWndMsgReceiver();

		private Mode _mode;

		public int Port { get; set; } = -1;


		public int MinKeypad { get; set; }

		public int MaxKeypad { get; set; }

		public event OnKeypadPressedHandler OnKeyPressed;

		public event OnQuizMasterRemotePressedHandler OnQuizMasterRemotePressed;

		public event SetIDSucceededHandler OnSetIDSucceeded;

		public event SetIDFailedHandler OnSetIDFailed;

		[DllImport("RF215_Com.dll")]
		public static extern bool Set_Hnd_MsgNo(IntPtr AppH, IntPtr WndH, int MsgNo);

		[DllImport("RF215_Com.dll")]
		public static extern bool CloseHnd();

		[DllImport("RF215_Com.dll")]
		public static extern int Activate_Receiver(byte CommPort, byte mMode, byte mType);

		[DllImport("RF215_Com.dll")]
		public static extern int Connect(byte CommPort, int MaxID, int MinID, byte PortType, int Sn1, int Sn2, int Sn3, int Sn4, int Sn5, int Sn6, int Sn7, int Sn8);

		[DllImport("RF215_Com.dll")]
		public static extern int DisConnect(int CommPort);

		[DllImport("RF215_Com.dll")]
		public static extern int Stop_Receiver(byte CommPort);

		[DllImport("RF215_Com.dll")]
		public static extern int Set_ID(byte CommPort, int RemoteID);

		[DllImport("RF215_Com.dll")]
		public static extern int BuzzConnect(byte CommPort, int MaxID, byte PortType, int Sn1, int Sn2, int Sn3, int Sn4, int Sn5, int Sn6, int Sn7, int Sn8);

		[DllImport("RF215_Com.dll")]
		public static extern int BStart_Buzz(byte CommPort, byte WithAnswer);

		[DllImport("RF215_Com.dll")]
		public static extern int BContinue_Buzz(byte CommPort);

		[DllImport("RF215_Com.dll")]
		public static extern int BStart_Answer(byte CommPort);

		[DllImport("RF215_Com.dll")]
		public static extern int BGet_Battery(byte CommPort);

		[DllImport("RF215_Com.dll")]
		public static extern int BStop_Receiver(byte CommPort);

		[DllImport("RF215_Com.dll")]
		public static extern int BSet_Winner(byte CommPort, int RemoteID, byte WithAnswer);

		[DllImport("RF215_Com.dll")]
		public static extern int BSet_Right(byte CommPort, int RemoteID);

		[DllImport("RF215_Com.dll")]
		public static extern int BSet_Wrong(byte CommPort, int RemoteID);

		[DllImport("RF215_Com.dll")]
		public static extern int BMusic_Control(byte CommPort, byte On_Off);

		[DllImport("RF215_Com.dll")]
		public static extern int BPowerOff(byte CommPort);

		[DllImport("RF215_Com.dll")]
		public static extern int BSet_ID(byte CommPort, int RemoteID);

		[DllImport("RF215_Com.dll")]
		public static extern int BFast_Answer(byte CommPort);

		[DllImport("RF215_Com.dll")]
		public static extern int BContinueFast_Answer(byte CommPort);

		public void SetParameter(string method, object value)
		{
			if (method == "Set217Mode")
			{
				if ((bool)value)
				{
					_mode = Mode.Keypad;
				}
				else
				{
					_mode = Mode.Buzzer;
				}
			}
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
				if (_mode == Mode.Buzzer)
				{
					BPowerOff(Convert.ToByte(0));
					Thread.Sleep(200);
					BStop_Receiver(Convert.ToByte(0));
				}
				if (_mode == Mode.Keypad)
				{
					Stop_Receiver(Convert.ToByte(0));
					Thread.Sleep(200);
					DisConnect(0);
				}
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
			if (_mode == Mode.Buzzer && BuzzConnect(Convert.ToByte(0), max, 0, 0, 0, 0, 0, 0, 0, 0, 0) != 0)
			{
				Port = 0;
			}
			else if (_mode == Mode.Keypad && Connect(Convert.ToByte(0), MaxKeypad, MinKeypad, 0, 0, 0, 0, 0, 0, 0, 0, 0) != 0)
			{
				Port = 0;
			}
			if (Port == -1)
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
						if (num == 1)
						{
							continue;
						}
						if (_mode == Mode.Buzzer)
						{
							if (BuzzConnect(Convert.ToByte(num), max, 1, num, 0, 0, 0, 0, 0, 0, 0) != 0)
							{
								goto IL_016c;
							}
							Port = -1;
						}
						else
						{
							if (Connect(Convert.ToByte(num), MaxKeypad, MinKeypad, 1, num, 0, 0, 0, 0, 0, 0, 0) != 0)
							{
								goto IL_016c;
							}
							Port = -1;
						}
						goto end_IL_00e0;
						IL_016c:
						Port = num;
						break;
						end_IL_00e0:;
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
			BMusic_Control(Convert.ToByte(Port), Convert.ToByte(false));
			Activate();
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

		private void _receiver_OnQuizMasterRemotePressed(BuzzerWndMsgReceiver.QuizMasterRemoteCommands arg)
		{
			if (this.OnQuizMasterRemotePressed != null)
			{
				QuizMasterRemoteCommands arg2 = QuizMasterRemoteCommands.None;
				switch (arg)
				{
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.Power:
					arg2 = QuizMasterRemoteCommands.Power;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.Score:
					arg2 = QuizMasterRemoteCommands.Score;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.Chart:
					arg2 = QuizMasterRemoteCommands.Chart;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.Mode:
					arg2 = QuizMasterRemoteCommands.Mode;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.QuestionMark:
					arg2 = QuizMasterRemoteCommands.QuestionMark;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.Up:
					arg2 = QuizMasterRemoteCommands.Up;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.Down:
					arg2 = QuizMasterRemoteCommands.Down;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.Begin:
					arg2 = QuizMasterRemoteCommands.Begin;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.Stop:
					arg2 = QuizMasterRemoteCommands.Stop;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.Ok:
					arg2 = QuizMasterRemoteCommands.Ok;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.F1:
					arg2 = QuizMasterRemoteCommands.F1;
					break;
				case BuzzerWndMsgReceiver.QuizMasterRemoteCommands.F2:
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
			if (Port == -1)
			{
				return;
			}
			try
			{
				if (_mode == Mode.Keypad)
				{
					Stop_Receiver(Convert.ToByte(Port));
					Port = -1;
				}
			}
			catch
			{
			}
			try
			{
				if (_mode == Mode.Buzzer)
				{
					BPowerOff(Convert.ToByte(Port));
					Thread.Sleep(200);
				}
				Port = -1;
			}
			catch
			{
			}
		}

		public bool SetID(int id)
		{
			if (Port != -1)
			{
				if (_mode == Mode.Keypad)
				{
					return Set_ID(Convert.ToByte(Port), id) != 0;
				}
				return BSet_ID(Convert.ToByte(Port), id) != 0;
			}
			return false;
		}

		public void Deactivate()
		{
			if (Port == -1)
			{
				return;
			}
			try
			{
				if (_mode == Mode.Buzzer)
				{
					BStop_Receiver(Convert.ToByte(Port));
				}
			}
			catch
			{
			}
			try
			{
				if (_mode == Mode.Keypad)
				{
					Stop_Receiver(Convert.ToByte(Port));
				}
			}
			catch
			{
			}
		}

		public void AfterSetID()
		{
		}

		public bool Activate()
		{
			if (_mode == Mode.Keypad)
			{
				return Activate_Receiver(Convert.ToByte(Port), Convert.ToByte(0), Convert.ToByte(0)) == 0;
			}
			return BStart_Buzz(Convert.ToByte(Port), Convert.ToByte(false)) != 0;
		}

		public int ProgramTimeoutMS()
		{
			if (_mode == Mode.Buzzer)
			{
				return 15000;
			}
			return 4500;
		}

		public int MaxKeypads()
		{
			if (_mode == Mode.Keypad)
			{
				return 400;
			}
			return 100;
		}

		public bool SetRemote()
		{
			if (_mode == Mode.Keypad)
			{
				return SetID(MinKeypad);
			}
			return BSet_ID(Convert.ToByte(Port), 0) != 0;
		}

		public void StartPolling()
		{
			Activate();
		}

		public Bitmap GetSetIdPicture()
		{
			if (_mode == Mode.Keypad)
			{
				return Resources.keypad_press;
			}
			return Resources.buzzer;
		}

		public Bitmap GetNeutralPicture()
		{
			if (_mode == Mode.Keypad)
			{
				return Resources.keypad;
			}
			return Resources.buzzer_neutral;
		}

		public Bitmap GetDisabledNeutralPicture()
		{
			if (_mode == Mode.Keypad)
			{
				return Resources.keypad_grey;
			}
			return Resources.buzzer_neutral;
		}

		public Bitmap GetAutoProgramConfirmPicture()
		{
			if (_mode == Mode.Keypad)
			{
				return Resources.keypad_single;
			}
			return Resources.buzzer_single;
		}

		public void GetDisplayStrings(out string indicatorGroupBox, out string manualGroupBox, out string manualLabel, out string manualButton, out string autoGroupBox, out string autoLabel)
		{
			if (_mode == Mode.Keypad)
			{
				indicatorGroupBox = Resources.IndicatorGroupBoxKeypad;
				manualGroupBox = Resources.ManualGroupBoxKeypad;
				manualLabel = Resources.ManualLabelKeypad;
				manualButton = Resources.ManualButtonKeypad;
				autoGroupBox = Resources.AutoGroupBoxKeypad;
				autoLabel = Resources.AutoLabelKeypad;
			}
			else
			{
				indicatorGroupBox = Resources.IndicatorGroupBoxBuzzer;
				manualGroupBox = Resources.ManualGroupBoxBuzzer;
				manualLabel = Resources.ManualLabelBuzzer;
				manualButton = Resources.ManualButtonBuzzer;
				autoGroupBox = Resources.AutoGroupBoxBuzzer;
				autoLabel = Resources.AutoLabelBuzzer;
			}
		}
	}
}
