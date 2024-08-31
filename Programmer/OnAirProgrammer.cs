using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OnAirBuzzerLibrary;
using OnAirBuzzerLibrary.Messages;
using OnAirBuzzerLibrary.OnAir;
using Programmer.Properties;

namespace Programmer
{
	internal class OnAirProgrammer : IProgrammer
	{
		private int _programmingNow = -1;

		private bool _connected;

		public event OnKeypadPressedHandler OnKeyPressed;

		public event OnQuizMasterRemotePressedHandler OnQuizMasterRemotePressed;

		public event SetIDSucceededHandler OnSetIDSucceeded;

		public event SetIDFailedHandler OnSetIDFailed;

		public bool Activate()
		{
			_programmingNow = -1;
			StartPolling();
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
			return true;
		}

		public bool Connect(Form owningForm, int max, int min)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			if (!_connected)
			{
				_connected = OnAirBuzzer.get_Instance().Begin();
				if (_connected)
				{
					Unhook();
					OnAirBuzzer.get_Instance().add_OnButtonPress(new ButtonPressEventHandler(Instance_OnButtonPress));
					OnAirBuzzer.get_Instance().add_OnChange(new ChangedEventHandler(Instance_OnChange));
					Thread.Sleep(200);
					OnAirBuzzer.get_Instance().ReportDevices();
					Thread.Sleep(100);
					OnAirBuzzer.get_Instance().ReportVersions();
					Thread.Sleep(100);
					StartPolling();
				}
			}
			return _connected;
		}

		private void Instance_OnChange(Buzzer buzzer, string property)
		{
			if (_programmingNow == -1)
			{
				return;
			}
			if (buzzer.get_Identifier() == _programmingNow && property.StartsWith("Connect"))
			{
				SetIDSucceededHandler onSetIDSucceeded = this.OnSetIDSucceeded;
				if (onSetIDSucceeded != null)
				{
					onSetIDSucceeded();
				}
			}
			else
			{
				SetIDFailedHandler onSetIDFailed = this.OnSetIDFailed;
				if (onSetIDFailed != null)
				{
					onSetIDFailed();
				}
			}
			_programmingNow = -1;
		}

		private void Instance_OnButtonPress(Buzzer buzzer, BuzzerButtonEnum state)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected I4, but got Unknown
			if (_programmingNow >= 0)
			{
				SetIDFailedHandler onSetIDFailed = this.OnSetIDFailed;
				if (onSetIDFailed != null)
				{
					onSetIDFailed();
				}
				return;
			}
			int key = (int)state;
			OnKeypadPressedHandler onKeyPressed = this.OnKeyPressed;
			if (onKeyPressed != null)
			{
				onKeyPressed(buzzer.get_Identifier(), key);
			}
		}

		public void Deactivate()
		{
		}

		public void Disconnect()
		{
			if (_connected)
			{
				Unhook();
				OnAirBuzzer.get_Instance().EndQuestion();
				Thread.Sleep(200);
				OnAirBuzzer.get_Instance().End();
				Thread.Sleep(500);
				_connected = false;
			}
		}

		private void Unhook()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			OnAirBuzzer.get_Instance().remove_OnButtonPress(new ButtonPressEventHandler(Instance_OnButtonPress));
			OnAirBuzzer.get_Instance().remove_OnChange(new ChangedEventHandler(Instance_OnChange));
		}

		public Bitmap GetAutoProgramConfirmPicture()
		{
			return Resources.onairbuzzer;
		}

		public Bitmap GetDisabledNeutralPicture()
		{
			return Resources.onairbuzzer;
		}

		public void GetDisplayStrings(out string indicatorGroupBox, out string manualGroupBox, out string manualLabel, out string manualButton, out string autoGroupBox, out string autoLabel)
		{
			indicatorGroupBox = "Buzzer received";
			manualGroupBox = "OnAir Buzzer assignment";
			manualLabel = "Buzzer to set";
			manualButton = "Assign Buzzer";
			autoGroupBox = "Automatic buzzer assignment";
			autoLabel = "Start";
		}

		public Bitmap GetNeutralPicture()
		{
			return Resources.onairbuzzer;
		}

		public Bitmap GetSetIdPicture()
		{
			return Resources.onairbuzzer_programming;
		}

		public int MaxKeypads()
		{
			return 360;
		}

		public int ProgramTimeoutMS()
		{
			return 10000;
		}

		public bool SetID(int id)
		{
			_programmingNow = id;
			OnAirBuzzer.get_Instance().EndQuestion();
			Thread.Sleep(100);
			OnAirBuzzer.get_Instance().Pair(id);
			return true;
		}

		public void SetParameter(string method, object value)
		{
			if (method == "EnablePairing")
			{
				OnAirBuzzer.get_Instance().EnablePairing();
			}
			else if (method == "DisablePairing")
			{
				OnAirBuzzer.get_Instance().DisablePairing();
				Thread.Sleep(100);
				StartPolling();
			}
		}

		public bool SetRemote()
		{
			return false;
		}

		public void StartPolling()
		{
			OnAirBuzzer.get_Instance().BeginQuestion((ushort)0, false);
		}
	}
}
