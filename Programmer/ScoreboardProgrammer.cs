using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using BuzzerHAL;
using Programmer.Properties;

namespace Programmer
{
	internal class ScoreboardProgrammer : IProgrammer
	{
		private IScoreboardControl _scoreboardControl;

		public event OnKeypadPressedHandler OnKeyPressed;

		public event OnQuizMasterRemotePressedHandler OnQuizMasterRemotePressed;

		public event SetIDSucceededHandler OnSetIDSucceeded;

		public event SetIDFailedHandler OnSetIDFailed;

		public bool Connect(Form owningForm, int max, int min)
		{
			_scoreboardControl = ScoreboardControlFactory.Create((ScoreboardDevice)0);
			bool num = _scoreboardControl.Connect(owningForm, min, max);
			if (num)
			{
				_scoreboardControl.AllShowId();
			}
			return num;
		}

		public void Disconnect()
		{
			IScoreboardControl scoreboardControl = _scoreboardControl;
			if (scoreboardControl != null)
			{
				scoreboardControl.AllPowerOff();
			}
			Thread.Sleep(500);
			IScoreboardControl scoreboardControl2 = _scoreboardControl;
			if (scoreboardControl2 != null)
			{
				scoreboardControl2.Disconnect();
			}
		}

		public bool SetID(int id)
		{
			IScoreboardControl scoreboardControl = _scoreboardControl;
			if (scoreboardControl != null)
			{
				scoreboardControl.SetId(id);
			}
			return true;
		}

		public void Deactivate()
		{
		}

		public void AfterSetID()
		{
			IScoreboardControl scoreboardControl = _scoreboardControl;
			if (scoreboardControl != null)
			{
				scoreboardControl.AllShowId();
			}
		}

		public bool Activate()
		{
			return true;
		}

		public int ProgramTimeoutMS()
		{
			return 7000;
		}

		public int MaxKeypads()
		{
			return 64;
		}

		public bool SetRemote()
		{
			return false;
		}

		public void StartPolling()
		{
		}

		public Bitmap GetSetIdPicture()
		{
			return Resources.scoreboard_programming;
		}

		public Bitmap GetNeutralPicture()
		{
			return Resources.scoreboard_neutral;
		}

		public Bitmap GetDisabledNeutralPicture()
		{
			return null;
		}

		public Bitmap GetAutoProgramConfirmPicture()
		{
			return null;
		}

		public void GetDisplayStrings(out string indicatorGroupBox, out string manualGroupBox, out string manualLabel, out string manualButton, out string autoGroupBox, out string autoLabel)
		{
			indicatorGroupBox = string.Empty;
			manualGroupBox = Resources.ManualGroupBoxScoreboard;
			manualLabel = Resources.ManualLabelScoreboard;
			manualButton = Resources.ManualButtonScoreboard;
			autoGroupBox = Resources.AutoGroupBoxScoreboard;
			autoLabel = Resources.AutoLabelScoreboard;
		}

		public void SetParameter(string method, object value)
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
	}
}
