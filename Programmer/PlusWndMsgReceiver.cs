using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Programmer
{
	public class PlusWndMsgReceiver : Form
	{
		public delegate void OnRF317KeyPressedHandler(int nKeyPad, string key, string time);

		public delegate void OnRF317QuizMasterRemotePressedHandler(QuizMasterRemoteCommands arg);

		public delegate void SetIDSucceededHandler();

		public delegate void SetIDFailedHandler();

		private IContainer components;

		public event OnRF317KeyPressedHandler OnKeyPressed;

		public event OnRF317QuizMasterRemotePressedHandler OnQuizMasterRemotePressed;

		public event SetIDSucceededHandler OnSetIDSucceeded;

		public event SetIDFailedHandler OnSetIDFailed;

		public PlusWndMsgReceiver()
		{
			InitializeComponent();
		}

		private void PlusWndMsgReceiver_Load(object sender, EventArgs e)
		{
			((Control)this).set_Visible(false);
		}

		protected unsafe override void WndProc(ref Message msg)
		{
			if (((Message)(ref msg)).get_Msg() == 111)
			{
				byte[] array = new byte[5];
				byte[] array2 = new byte[250];
				byte* ptr = (byte*)((Message)(ref msg)).get_LParam().ToPointer();
				for (int i = 0; i < 5; i++)
				{
					array[i] = Convert.ToByte(ptr[i]);
				}
				switch (array[0])
				{
				case 1:
				{
					QuizMasterRemoteCommands arg = QuizMasterRemoteCommands.None;
					switch (array[4] & 0x3F)
					{
					case 0:
						arg = QuizMasterRemoteCommands.Up;
						break;
					case 1:
						arg = QuizMasterRemoteCommands.Ok;
						break;
					case 2:
						arg = QuizMasterRemoteCommands.Power;
						break;
					case 3:
						arg = QuizMasterRemoteCommands.Chart;
						break;
					case 4:
						arg = QuizMasterRemoteCommands.Score;
						break;
					case 5:
						arg = QuizMasterRemoteCommands.Begin;
						break;
					case 6:
						arg = QuizMasterRemoteCommands.Stop;
						break;
					case 7:
						arg = QuizMasterRemoteCommands.Down;
						break;
					case 8:
						arg = QuizMasterRemoteCommands.Mode;
						break;
					case 9:
						arg = QuizMasterRemoteCommands.QuestionMark;
						break;
					case 10:
						arg = QuizMasterRemoteCommands.F1;
						break;
					case 11:
						arg = QuizMasterRemoteCommands.F2;
						break;
					}
					OnRF317QuizMasterRemotePressedHandler onQuizMasterRemotePressed = this.OnQuizMasterRemotePressed;
					if (onQuizMasterRemotePressed != null)
					{
						onQuizMasterRemotePressed(arg);
					}
					break;
				}
				case 2:
				{
					int num = 5;
					int num2 = 0;
					string text = "";
					while (ptr[num] != 0)
					{
						array2[num2] = ptr[num];
						num++;
						num2++;
					}
					text = Encoding.ASCII.GetString(array2, 0, num2);
					int nKeyPad = (array[1] & 0x3F) * 32 + (array[2] & 0x3F);
					int num3 = array[4] & 0x3F;
					OnRF317KeyPressedHandler onKeyPressed = this.OnKeyPressed;
					if (onKeyPressed != null)
					{
						onKeyPressed(nKeyPad, text.Substring(0, 1), text.Substring(1));
					}
					break;
				}
				case 3:
				{
					byte b = array[2];
					int num4 = 65;
					if (array[4] == 65)
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
					break;
				}
				}
			}
			((Form)this).WndProc(ref msg);
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
			((Control)this).set_Name("PlusWndMsgReceiver");
			((Form)this).set_ShowInTaskbar(false);
			((Control)this).set_Text("WndMsgReceiver");
			((Form)this).add_Load((EventHandler)PlusWndMsgReceiver_Load);
			((Control)this).ResumeLayout(false);
		}
	}
}
