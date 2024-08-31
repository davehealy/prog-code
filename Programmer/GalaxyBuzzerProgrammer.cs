using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Programmer.Properties;

namespace Programmer
{
	internal class GalaxyBuzzerProgrammer : IProgrammer
	{
		public enum Intensity
		{
			Off,
			Low,
			Middle,
			High
		}

		public enum GalaxyBuzzerType
		{
			Torch305V,
			Buzzer303S
		}

		private RF21xDevice rf;

		private RF21xMessage message;

		private Timer _messageTimer = new Timer();

		private GalaxyBuzzerType _buzzerType;

		public int MinKeypad { get; set; }

		public event OnKeypadPressedHandler OnKeyPressed;

		public event OnQuizMasterRemotePressedHandler OnQuizMasterRemotePressed;

		public event SetIDSucceededHandler OnSetIDSucceeded;

		public event SetIDFailedHandler OnSetIDFailed;

		internal GalaxyBuzzerProgrammer(GalaxyBuzzerType buzzerType)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			rf = new RF21xDevice();
			message = new RF21xMessage();
			_buzzerType = buzzerType;
			_messageTimer.set_Interval(20);
			_messageTimer.add_Tick((EventHandler)messageTimer_Tick);
		}

		public bool Activate()
		{
			return !rf.startQuiz(rf21x.RF21X_QT_Single, 0, 0, 1);
		}

		public void AfterSetID()
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

		public bool Connect(Form owningForm, int max, int min)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			MinKeypad = min;
			int num = 3;
			while (true)
			{
				if (num == 0)
				{
					return false;
				}
				int rF21X_DT_RF = rf21x.RF21X_DT_RF219;
				if (rf.open(rF21X_DT_RF, "hid://", min, max))
				{
					break;
				}
				num--;
				Thread.Sleep(300);
			}
			_messageTimer.set_Enabled(true);
			Thread.Sleep(1000);
			int rF21X_QT_Control = rf21x.RF21X_QT_Control;
			SWIGTYPE_p_void val = new SWIGTYPE_p_void(GCHandle.Alloc(new byte[10] { 8, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, GCHandleType.Pinned).AddrOfPinnedObject(), true);
			if (!rf.startDirectly(rF21X_QT_Control, val))
			{
				return false;
			}
			Activate();
			return true;
		}

		public void Deactivate()
		{
		}

		public void Disconnect()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			int rF21X_QT_Control = rf21x.RF21X_QT_Control;
			SWIGTYPE_p_void val = new SWIGTYPE_p_void(GCHandle.Alloc(new byte[10] { 9, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, GCHandleType.Pinned).AddrOfPinnedObject(), true);
			rf.startDirectly(rF21X_QT_Control, val);
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
			if (_buzzerType == GalaxyBuzzerType.Buzzer303S)
			{
				indicatorGroupBox = Resources.IndicatorGroupBoxBuzzer;
				manualGroupBox = Resources.ManualGroupBoxBuzzer;
				manualLabel = Resources.ManualLabelBuzzer;
				manualButton = Resources.ManualButtonBuzzer;
				autoGroupBox = Resources.AutoGroupBoxBuzzer;
				autoLabel = Resources.AutoLabelBuzzer;
			}
			else
			{
				indicatorGroupBox = Resources.IndicatorGroupBoxKeypad;
				manualGroupBox = Resources.ManualGroupBoxTorch;
				manualLabel = Resources.ManualLabelTorch;
				manualButton = Resources.ManualButtonTorch;
				autoGroupBox = Resources.AutoGroupBoxTorch;
				autoLabel = Resources.AutoLabelTorch;
			}
		}

		public Bitmap GetNeutralPicture()
		{
			if (_buzzerType == GalaxyBuzzerType.Buzzer303S)
			{
				return Resources.buzzer_galaxy_neutral;
			}
			return Resources.Torch_neutral;
		}

		public Bitmap GetSetIdPicture()
		{
			if (_buzzerType == GalaxyBuzzerType.Buzzer303S)
			{
				return Resources.buzzer_galaxy;
			}
			return Resources.Torch_press;
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
			return rf.setKeypadId(id - 1);
		}

		public void SetParameter(string method, object value)
		{
		}

		public bool SetRemote()
		{
			return rf.setKeypadId(MinKeypad);
		}

		public void StartPolling()
		{
			Activate();
		}

		private QuizMasterRemoteCommands QuizMasterRemoteCommandFromString(string commmand)
		{
			QuizMasterRemoteCommands result = QuizMasterRemoteCommands.None;
			switch (commmand)
			{
			case "power":
				result = QuizMasterRemoteCommands.Power;
				break;
			case "report":
				result = QuizMasterRemoteCommands.Score;
				break;
			case "result":
				result = QuizMasterRemoteCommands.Chart;
				break;
			case "mode":
				result = QuizMasterRemoteCommands.Mode;
				break;
			case "question":
				result = QuizMasterRemoteCommands.QuestionMark;
				break;
			case "up":
				result = QuizMasterRemoteCommands.Up;
				break;
			case "start/pause":
				result = QuizMasterRemoteCommands.Begin;
				break;
			case "ok":
				result = QuizMasterRemoteCommands.Ok;
				break;
			case "stop":
				result = QuizMasterRemoteCommands.Stop;
				break;
			case "f1":
				result = QuizMasterRemoteCommands.F1;
				break;
			case "down":
				result = QuizMasterRemoteCommands.Down;
				break;
			case "f2":
				result = QuizMasterRemoteCommands.F2;
				break;
			}
			return result;
		}

		private void messageTimer_Tick(object sender, EventArgs e)
		{
			if (!rf.getMessage(message))
			{
				return;
			}
			if (message.get_messageType() == rf21x.RF21X_MT_Teacher)
			{
				if (this.OnQuizMasterRemotePressed != null)
				{
					this.OnQuizMasterRemotePressed(QuizMasterRemoteCommandFromString(message.get_data()));
				}
			}
			else if (message.get_messageType() == rf21x.RF21X_MT_Student)
			{
				if (this.OnKeyPressed == null)
				{
					return;
				}
				int key = 0;
				int keypadId = message.get_keypadId();
				if (_buzzerType == GalaxyBuzzerType.Torch305V)
				{
					switch (message.get_data())
					{
					case "G":
						key = 0;
						break;
					case "A":
						key = 1;
						break;
					case "B":
						key = 2;
						break;
					case "C":
						key = 3;
						break;
					case "D":
						key = 4;
						break;
					case "E":
						key = 5;
						break;
					case "F":
						key = 6;
						break;
					}
				}
				else if (_buzzerType == GalaxyBuzzerType.Buzzer303S)
				{
					Intensity r;
					Intensity g;
					Intensity b;
					switch (message.get_data()[0])
					{
					case 'E':
						key = 0;
						Blink(keypadId, Color.White);
						break;
					case 'A':
						key = 1;
						ToIntensity(Color.Blue, out r, out g, out b);
						SendLight(keypadId, r, g, b);
						break;
					case 'B':
						key = 2;
						ToIntensity(Color.Red, out r, out g, out b);
						SendLight(keypadId, r, g, b);
						break;
					case 'C':
						key = 3;
						ToIntensity(Color.Green, out r, out g, out b);
						SendLight(keypadId, r, g, b);
						break;
					case 'D':
						key = 4;
						ToIntensity(Color.Yellow, out r, out g, out b);
						SendLight(keypadId, r, g, b);
						break;
					}
				}
				this.OnKeyPressed(message.get_keypadId(), key);
			}
			else
			{
				if (message.get_messageType() != rf21x.RF21X_MT_SetId)
				{
					return;
				}
				try
				{
					if (message.get_data() == "Success")
					{
						try
						{
							SetIDSucceededHandler onSetIDSucceeded = this.OnSetIDSucceeded;
							if (onSetIDSucceeded != null)
							{
								onSetIDSucceeded();
							}
							return;
						}
						catch
						{
							return;
						}
					}
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
				catch
				{
				}
			}
		}

		private void ToIntensity(Color color, out Intensity r, out Intensity g, out Intensity b)
		{
			r = ToIntensity(color.R);
			g = ToIntensity(color.G);
			b = ToIntensity(color.B);
		}

		private Intensity ToIntensity(byte color)
		{
			switch ((int)color / 85)
			{
			case 0:
				return Intensity.Off;
			case 1:
				return Intensity.Low;
			case 2:
				return Intensity.Middle;
			case 3:
				return Intensity.High;
			default:
				return Intensity.Off;
			}
		}

		private void SendLight(int device, Intensity r, Intensity g, Intensity b)
		{
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected O, but got Unknown
			byte b2 = 1;
			byte b3 = (byte)r;
			byte b4 = (byte)g;
			byte b5 = (byte)b;
			int rF21X_QT_Control = rf21x.RF21X_QT_Control;
			byte[] array = new byte[10];
			switch (_buzzerType)
			{
			case GalaxyBuzzerType.Torch305V:
				array[0] = 3;
				array[1] = Convert.ToByte((device - 1) / 16);
				array[2] = Convert.ToByte((device - 1) % 16);
				array[3] = 0;
				array[4] = b2;
				array[5] = b3;
				array[6] = b4;
				array[7] = b5;
				break;
			case GalaxyBuzzerType.Buzzer303S:
				array[0] = 3;
				array[1] = (byte)(device - 1);
				array[2] = 0;
				array[3] = b2;
				array[4] = b3;
				array[5] = b4;
				array[6] = b5;
				break;
			}
			SWIGTYPE_p_void val = new SWIGTYPE_p_void(GCHandle.Alloc(array, GCHandleType.Pinned).AddrOfPinnedObject(), true);
			rf.startDirectly(rF21X_QT_Control, val);
		}

		public void Blink(int nKeyPad, Color? color = null)
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Expected O, but got Unknown
			Intensity r;
			Intensity g;
			Intensity b;
			if (!color.HasValue)
			{
				ToIntensity(Color.White, out r, out g, out b);
			}
			else
			{
				ToIntensity(color.Value, out r, out g, out b);
			}
			int count = 0;
			Timer val = new Timer();
			val.set_Interval(400);
			Timer timer = val;
			timer.add_Tick((EventHandler)delegate
			{
				if (count % 2 == 0)
				{
					SendLight(nKeyPad, r, g, b);
				}
				else
				{
					SendLight(nKeyPad, Intensity.Off, Intensity.Off, Intensity.Off);
				}
				count++;
				if (count == 10)
				{
					timer.Stop();
				}
			});
			timer.Start();
		}
	}
}
