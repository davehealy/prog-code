using System.Drawing;
using System.Windows.Forms;

namespace Programmer
{
	internal interface IProgrammer
	{
		event OnKeypadPressedHandler OnKeyPressed;

		event OnQuizMasterRemotePressedHandler OnQuizMasterRemotePressed;

		event SetIDSucceededHandler OnSetIDSucceeded;

		event SetIDFailedHandler OnSetIDFailed;

		bool Connect(Form owningForm, int max, int min);

		void Disconnect();

		bool SetID(int id);

		void Deactivate();

		void AfterSetID();

		bool Activate();

		int ProgramTimeoutMS();

		int MaxKeypads();

		bool SetRemote();

		void StartPolling();

		Bitmap GetSetIdPicture();

		Bitmap GetNeutralPicture();

		Bitmap GetDisabledNeutralPicture();

		Bitmap GetAutoProgramConfirmPicture();

		void GetDisplayStrings(out string indicatorGroupBox, out string manualGroupBox, out string manualLabel, out string manualButton, out string autoGroupBox, out string autoLabel);

		void SetParameter(string method, object value);

		bool CanAutoProgram();

		bool CanProgramRemote();

		bool CanReceive();
	}
}
