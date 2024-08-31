using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Programmer
{
	public class WndMsgReceiver : Form
	{
		public delegate void OnRF217KeyPressedHandler(int nKeyPad, int key);

		public delegate void OnRF217QuizMasterRemotePressedHandler(QuizMasterRemoteCommands arg);

		public delegate void SetIDSucceededHandler();

		public enum QuizMasterRemoteCommands
		{
			None,
			Power,
			Score,
			Chart,
			Mode,
			QuestionMark,
			Up,
			Down,
			Begin,
			Stop,
			Ok,
			F1,
			F2
		}

		private IContainer components;

		public event OnRF217KeyPressedHandler OnKeyPressed;

		public event OnRF217QuizMasterRemotePressedHandler OnQuizMasterRemotePressed;

		public event SetIDSucceededHandler OnSetIDSucceeded;

		public WndMsgReceiver()
		{
			InitializeComponent();
		}

		private void WndMsgReceiver_Load(object sender, EventArgs e)
		{
			((Control)this).set_Visible(false);
		}

		protected unsafe override void WndProc(ref Message msg)
		{
			if (((Message)(ref msg)).get_Msg() == 111)
			{
				int[] array = new int[7];
				byte* ptr = (byte*)((Message)(ref msg)).get_LParam().ToPointer();
				int.Parse(ptr->ToString());
				for (int i = 0; i < 7; i++)
				{
					array[i] = int.Parse(ptr[i].ToString());
				}
				int num = 0;
				int iD = (array[1] & 0x7F) * 16 + (array[2] & 0x7F);
				num = array[5];
				int questionNo = array[6] & 0x7F;
				if (array[0] == 2 || array[0] == 3)
				{
					int num2 = array[1];
					int num3 = array[2];
				}
				ReceiveCommand(array[0], iD, array[3], array[4], num, questionNo);
			}
			((Form)this).WndProc(ref msg);
		}

		public void ReceiveCommand(int Info_Type, int ID, int mType, int mMode, int Answer, int QuestionNo)
		{
			switch (Info_Type)
			{
			case 1:
			{
				QuizMasterRemoteCommands arg = QuizMasterRemoteCommands.None;
				switch (Answer)
				{
				case 128:
					arg = QuizMasterRemoteCommands.Up;
					break;
				case 129:
					arg = QuizMasterRemoteCommands.Ok;
					break;
				case 130:
					arg = QuizMasterRemoteCommands.Power;
					break;
				case 131:
					arg = QuizMasterRemoteCommands.Chart;
					break;
				case 132:
					arg = QuizMasterRemoteCommands.Score;
					break;
				case 133:
					arg = QuizMasterRemoteCommands.Begin;
					break;
				case 134:
					arg = QuizMasterRemoteCommands.Stop;
					break;
				case 135:
					arg = QuizMasterRemoteCommands.Down;
					break;
				case 136:
					arg = QuizMasterRemoteCommands.Mode;
					break;
				case 137:
					arg = QuizMasterRemoteCommands.QuestionMark;
					break;
				case 138:
					arg = QuizMasterRemoteCommands.F1;
					break;
				case 139:
					arg = QuizMasterRemoteCommands.F2;
					break;
				}
				if (this.OnQuizMasterRemotePressed != null)
				{
					this.OnQuizMasterRemotePressed(arg);
				}
				break;
			}
			case 2:
				if (mType == 1 && this.OnKeyPressed != null)
				{
					this.OnKeyPressed(ID, Answer);
				}
				break;
			case 3:
				if (this.OnSetIDSucceeded != null)
				{
					this.OnSetIDSucceeded();
				}
				break;
			case 4:
				break;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			((Form)this).Dispose(disposing);
		}

		private void InitializeComponent()
		{
			((Control)this).SuspendLayout();
			((ContainerControl)this).set_AutoScaleDimensions(new SizeF(6f, 13f));
			((ContainerControl)this).set_AutoScaleMode((AutoScaleMode)1);
			((Form)this).set_ClientSize(new Size(116, 0));
			((Form)this).set_MaximizeBox(false);
			((Form)this).set_MinimizeBox(false);
			((Control)this).set_Name("WndMsgReceiver");
			((Control)this).set_Text("WndMsgReceiver");
			((Form)this).add_Load((EventHandler)WndMsgReceiver_Load);
			((Control)this).ResumeLayout(false);
		}
	}
}
