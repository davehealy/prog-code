using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using IQLib;
using Programmer.Properties;

namespace Programmer
{
	public class ProgrammerForm : Form
	{
		private enum TimerMode
		{
			Unknown,
			ModeReceive,
			ModeProgrammingManual,
			ModeProgrammingAuto,
			ModeProgrammingRemote,
			ModeReceiving
		}

		private IProgrammer _programmer;

		private Timer _timer;

		private const int StandardReceiver = 0;

		private const int PlusReceiver = 1;

		private const int BuzzerReceiver = 2;

		private const int GalaxyBuzzerReceiver = 3;

		private const int TorchReceiver = 4;

		private const int ScorerReceiver = 5;

		private const int GalaxyReceiver = 6;

		private int _onAirBuzzerIndex = -1;

		private const int ReceiveBlinkTimerIntervalFast = 600;

		private const int ReceiveBlinkTimerIntervalSlow = 2500;

		private const int ReceiveBlinkWaitTimerInterval = 4000;

		private string ReceivingIndicator = "---";

		private TimerMode _timerMode;

		private SpeechSynthesizer _speech = new SpeechSynthesizer();

		private GroupBox _onAirPairingUI;

		private int _currentAutoId;

		private IContainer components;

		private GroupBox groupBoxManual;

		private Label labelManual;

		private NumericUpDown numericKeypadId;

		private GroupBox groupBoxIndicator;

		private Label labelKeypadReceived;

		private Button buttonSet;

		private PictureBox pictureBoxKeypad;

		private GroupBox groupBoxAutomatic;

		private Button buttonStop;

		private Button buttonStart;

		private Label label3;

		private Label labelAutomatic;

		private NumericUpDown numericIncrement;

		private NumericUpDown numericKeypadStart;

		private GroupBox groupBox4;

		private TextBox textBoxKeypadString;

		private Label labelKeypadString;

		private Button buttonConnect;

		private PictureBox pictureBoxStatusManual;

		private PictureBox pictureBoxStatusAuto;

		private PictureBox pictureBoxStatusConnect;

		private ComboBox comboBoxReceiver;

		private Label labelReceiver;

		private Button buttonSetRemote;

		private GroupBox groupBoxPresenter;

		private Label label4;

		private PictureBox pictureBoxStatusRemote;

		private Panel panel1;

		private Label label5;

		private PictureBox pictureBox1;

		private CheckBox checkBoxAutoIncrement;

		public string AppDataFolder
		{
			get
			{
				string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuizXpress");
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				return text;
			}
		}

		public string LogFolder
		{
			get
			{
				string text = Path.Combine(AppDataFolder, "Logfiles");
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				return text;
			}
		}

		public string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public ProgrammerForm()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			InitializeComponent();
		}

		private void ProgrammerForm_Load(object sender, EventArgs e)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			try
			{
				((ListControl)comboBoxReceiver).set_SelectedIndex(0);
				ResetUI();
				if (QXEnvironment.get_IsOnAir())
				{
					_onAirBuzzerIndex = comboBoxReceiver.get_Items().Add((object)"OnAir Buzzer Receiver");
				}
				((Control)this).set_Text(string.Format("{0} - version {1}", ((Control)this).get_Text(), AssemblyVersion));
				try
				{
					_speech = new SpeechSynthesizer();
					if (_speech.GetInstalledVoices().Count > 0)
					{
						_speech.SelectVoiceByHints((VoiceGender)2);
					}
					else
					{
						_speech = null;
					}
				}
				catch
				{
				}
			}
			catch (Exception exc)
			{
				ReportException(exc);
			}
		}

		private void ResetUI()
		{
			((Control)buttonStop).set_Enabled(false);
			if (_programmer != null)
			{
				pictureBoxKeypad.set_Image((Image)_programmer.GetDisabledNeutralPicture());
				UpdateLabels();
			}
			else
			{
				pictureBoxKeypad.set_Image((Image)Resources.usb);
			}
			pictureBoxStatusAuto.set_Image((Image)null);
			pictureBoxStatusConnect.set_Image((Image)null);
			pictureBoxStatusManual.set_Image((Image)null);
			pictureBoxStatusRemote.set_Image((Image)null);
			DisableAll();
			((Control)buttonConnect).set_Enabled(true);
			((Control)textBoxKeypadString).set_Enabled(true);
			((Control)labelKeypadString).set_Enabled(true);
			((Control)labelReceiver).set_Enabled(true);
			((Control)comboBoxReceiver).set_Enabled(true);
			Timer timer = _timer;
			if (timer != null)
			{
				timer.Stop();
			}
			_timerMode = TimerMode.Unknown;
			ResetReceiveLabel();
		}

		private void UpdateLabels()
		{
			string indicatorGroupBox;
			string manualGroupBox;
			string manualLabel;
			string manualButton;
			string autoGroupBox;
			string autoLabel;
			_programmer.GetDisplayStrings(out indicatorGroupBox, out manualGroupBox, out manualLabel, out manualButton, out autoGroupBox, out autoLabel);
			((Control)groupBoxIndicator).set_Text(indicatorGroupBox);
			((Control)groupBoxManual).set_Text(manualGroupBox);
			((Control)labelManual).set_Text(manualLabel);
			((Control)groupBoxAutomatic).set_Text(autoGroupBox);
			((Control)labelAutomatic).set_Text(autoLabel);
			((Control)groupBoxIndicator).set_Text(indicatorGroupBox);
			((Control)buttonSet).set_Text(manualButton);
		}

		private void _programmer_OnSetIDSucceeded()
		{
			if (((Control)this).get_InvokeRequired())
			{
				((Control)this).BeginInvoke((Delegate)(Action)delegate
				{
					_programmer_OnSetIDSucceeded();
				});
				return;
			}
			Thread.Sleep(750);
			if (_timerMode == TimerMode.ModeProgrammingManual)
			{
				pictureBoxStatusManual.set_Image((Image)Resources.ok);
				EndProgrammingManual();
			}
			else if (_timerMode == TimerMode.ModeProgrammingRemote)
			{
				pictureBoxStatusRemote.set_Image((Image)Resources.ok);
				EndProgrammingManual();
			}
			else if (_timerMode == TimerMode.ModeProgrammingAuto)
			{
				pictureBoxStatusAuto.set_Image((Image)Resources.ok);
				EndProgrammingAuto();
			}
		}

		private void _programmer_OnSetIDFailed()
		{
			if (((Control)this).get_InvokeRequired())
			{
				((Control)this).BeginInvoke((Delegate)(Action)delegate
				{
					_programmer_OnSetIDFailed();
				});
				return;
			}
			Thread.Sleep(750);
			if (_timerMode == TimerMode.ModeProgrammingManual)
			{
				pictureBoxStatusManual.set_Image((Image)Resources.error);
				EndProgrammingManual();
			}
			else if (_timerMode == TimerMode.ModeProgrammingRemote)
			{
				pictureBoxStatusRemote.set_Image((Image)Resources.error);
				EndProgrammingManual();
			}
			else if (_timerMode == TimerMode.ModeProgrammingAuto)
			{
				pictureBoxStatusAuto.set_Image((Image)Resources.error);
				EndProgrammingAuto();
			}
		}

		private void _programmer_OnQuizMasterRemotePressed(QuizMasterRemoteCommands arg)
		{
			if (((Control)this).get_InvokeRequired())
			{
				((Control)this).BeginInvoke((Delegate)(Action)delegate
				{
					_programmer_OnQuizMasterRemotePressed(arg);
				});
				return;
			}
			((Control)labelKeypadReceived).set_ForeColor(Color.Black);
			((Control)labelKeypadReceived).set_BackColor(SystemColors.ButtonFace);
			((Control)labelKeypadReceived).set_Text("REMOTE (" + arg.ToString() + ")");
			((Control)labelKeypadReceived).Update();
			_timerMode = TimerMode.ModeReceive;
			_timer.set_Interval(1000);
			_timer.Start();
			SpeechSynthesizer speech = _speech;
			if (speech != null)
			{
				speech.SpeakAsync("Remote control");
			}
		}

		private void SetReceiveLabelToProgramming()
		{
			((Control)labelKeypadReceived).set_ForeColor(Color.OrangeRed);
			((Control)labelKeypadReceived).set_BackColor(SystemColors.ButtonFace);
			((Control)labelKeypadReceived).set_Text(Resources.StatusProgramming);
			((Control)labelKeypadReceived).Update();
		}

		private void ResetReceiveLabel()
		{
			((Control)labelKeypadReceived).set_ForeColor(Color.Black);
			((Control)labelKeypadReceived).set_BackColor(SystemColors.ButtonFace);
			((Control)labelKeypadReceived).set_Text(ReceivingIndicator);
			((Control)labelKeypadReceived).Update();
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			try
			{
				_timer.Stop();
				switch (_timerMode)
				{
				case TimerMode.ModeProgrammingManual:
				case TimerMode.ModeProgrammingRemote:
					EndProgrammingManual();
					break;
				case TimerMode.ModeProgrammingAuto:
					EndProgrammingAuto();
					break;
				case TimerMode.ModeReceive:
					((Control)labelKeypadReceived).set_Text(ReceivingIndicator);
					((Control)labelKeypadReceived).set_BackColor(SystemColors.ButtonFace);
					_programmer.StartPolling();
					_timerMode = TimerMode.ModeReceiving;
					_timer.set_Interval(4000);
					_timer.Start();
					break;
				case TimerMode.ModeReceiving:
					if (((Control)labelKeypadReceived).get_Text().Contains("-"))
					{
						((Control)labelKeypadReceived).set_ForeColor(Color.DarkGreen);
						((Control)labelKeypadReceived).set_BackColor(SystemColors.ButtonFace);
						((Control)labelKeypadReceived).set_Text(Resources.StatusReceiving);
						_timer.set_Interval(600);
					}
					else
					{
						((Control)labelKeypadReceived).set_ForeColor(Color.Black);
						((Control)labelKeypadReceived).set_BackColor(SystemColors.ButtonFace);
						((Control)labelKeypadReceived).set_Text(ReceivingIndicator);
						_timer.set_Interval(2500);
					}
					_timer.Start();
					break;
				}
			}
			catch (Exception exc)
			{
				ReportException(exc);
			}
		}

		private void EndProgrammingAuto()
		{
			_programmer.AfterSetID();
			_programmer.Activate();
			pictureBoxKeypad.set_Image((Image)_programmer.GetAutoProgramConfirmPicture());
			((Control)pictureBoxKeypad).Update();
		}

		private void EndProgrammingManual()
		{
			_programmer.AfterSetID();
			_programmer.Activate();
			((Control)buttonSet).set_Enabled(true);
			((Control)buttonSetRemote).set_Enabled(true);
			((Control)numericKeypadId).set_Enabled(true);
			if (!_programmer.CanReceive() && checkBoxAutoIncrement.get_Checked() && numericKeypadId.get_Value() < numericKeypadId.get_Maximum())
			{
				NumericUpDown obj = numericKeypadId;
				decimal value = obj.get_Value();
				obj.set_Value(value + 1m);
			}
			ResetKeypadPicture();
			ResetReceiveLabel();
			_timerMode = TimerMode.ModeReceiving;
			_timer.set_Interval(600);
			_timer.Start();
		}

		private void _programmer_OnKeyPressed(int nKeyPad, int key)
		{
			if (((Control)this).get_InvokeRequired())
			{
				((Control)this).BeginInvoke((Delegate)(Action)delegate
				{
					_programmer_OnKeyPressed(nKeyPad, key);
				});
				return;
			}
			try
			{
				((Control)labelKeypadReceived).set_ForeColor(Color.Black);
				((Control)labelKeypadReceived).set_BackColor(GetQxButtonColorScheme()[key]);
				((Control)labelKeypadReceived).set_Text(nKeyPad + " - " + LetterFromKey(key));
				((Control)labelKeypadReceived).Update();
				if (_timerMode == TimerMode.ModeProgrammingAuto)
				{
					if (nKeyPad == _currentAutoId)
					{
						_currentAutoId += (int)numericIncrement.get_Value();
						numericKeypadStart.set_Value((decimal)_currentAutoId);
					}
					else
					{
						((Control)labelKeypadReceived).set_ForeColor(Color.Red);
						((Control)labelKeypadReceived).Update();
					}
					pictureBoxStatusAuto.set_Image((Image)null);
					SetID(_currentAutoId);
					_timer.set_Interval(_programmer.ProgramTimeoutMS());
					_timer.Start();
				}
				else
				{
					_timer.Stop();
					_timerMode = TimerMode.ModeReceive;
					_timer.set_Interval(1500);
					_timer.Start();
					if (checkBoxAutoIncrement.get_Checked() && numericKeypadId.get_Value() == (decimal)nKeyPad && numericKeypadId.get_Value() < numericKeypadId.get_Maximum())
					{
						NumericUpDown obj = numericKeypadId;
						decimal value = obj.get_Value();
						obj.set_Value(value + 1m);
					}
				}
				SpeechSynthesizer speech = _speech;
				if (speech != null)
				{
					speech.SpeakAsync(nKeyPad.ToString());
				}
			}
			catch (Exception exc)
			{
				ReportException(exc);
			}
		}

		private string LetterFromKey(int key)
		{
			switch (key)
			{
			case 0:
				return "FF";
			case 1:
				return "A";
			case 2:
				return "B";
			case 3:
				return "C";
			case 4:
				return "D";
			case 5:
				return "E";
			case 6:
				return "F";
			default:
				return "?";
			}
		}

		private void ProgrammerForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				if (_programmer != null)
				{
					Unhook();
					_programmer.Disconnect();
				}
			}
			catch (Exception exc)
			{
				ReportException(exc);
			}
		}

		private void buttonSet_Click(object sender, EventArgs e)
		{
			try
			{
				((Control)buttonSet).set_Enabled(false);
				((Control)numericKeypadId).set_Enabled(false);
				if (SetID((int)numericKeypadId.get_Value()))
				{
					_timerMode = TimerMode.ModeProgrammingManual;
					_timer.set_Interval(_programmer.ProgramTimeoutMS());
					_timer.Start();
					SetReceiveLabelToProgramming();
				}
				else
				{
					((Control)buttonSet).set_Enabled(true);
					((Control)numericKeypadId).set_Enabled(true);
				}
			}
			catch (Exception exc)
			{
				ReportException(exc);
			}
		}

		private void buttonStart_Click(object sender, EventArgs e)
		{
			try
			{
				((Control)buttonStart).set_Enabled(false);
				((Control)buttonStop).set_Enabled(true);
				pictureBoxStatusAuto.set_Image((Image)null);
				_currentAutoId = (int)numericKeypadStart.get_Value();
				SetID(_currentAutoId);
				_timerMode = TimerMode.ModeProgrammingAuto;
				_timer.set_Interval(_programmer.ProgramTimeoutMS());
				_timer.Start();
			}
			catch (Exception exc)
			{
				ReportException(exc);
			}
		}

		private void buttonStop_Click(object sender, EventArgs e)
		{
			try
			{
				((Control)buttonStart).set_Enabled(true);
				((Control)buttonStop).set_Enabled(false);
				_timerMode = TimerMode.Unknown;
				_timer.Stop();
				ResetKeypadPicture();
				ResetReceiveLabel();
				_programmer.Activate();
			}
			catch (Exception exc)
			{
				ReportException(exc);
			}
		}

		private void ResetKeypadPicture()
		{
			pictureBoxKeypad.set_Image((Image)_programmer.GetNeutralPicture());
			((Control)pictureBoxKeypad).Update();
		}

		private void DisableAll()
		{
			Enable(false);
		}

		private void EnableAll()
		{
			Enable(true);
		}

		private void Enable(bool enable)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			foreach (Control item in (ArrangedElementCollection)((Control)this).get_Controls())
			{
				Control val = item;
				if (val is GroupBox)
				{
					foreach (Control item2 in (ArrangedElementCollection)val.get_Controls())
					{
						item2.set_Enabled(enable);
					}
				}
				else
				{
					val.set_Enabled(enable);
				}
			}
		}

		private bool SetID(int id)
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			_programmer.Deactivate();
			SpeechSynthesizer speech = _speech;
			if (speech != null)
			{
				speech.SpeakAsync(Resources.StatusProgramming.Replace(".", string.Empty) + " " + id);
			}
			if (!_programmer.SetID(id))
			{
				MessageBox.Show("Failed to set keypad ID, please retry.", "Programmer", (MessageBoxButtons)0, (MessageBoxIcon)16);
				return false;
			}
			pictureBoxStatusManual.set_Image((Image)null);
			pictureBoxKeypad.set_Image((Image)_programmer.GetSetIdPicture());
			((Control)pictureBoxKeypad).Update();
			return true;
		}

		private void buttonConnect_Click(object sender, EventArgs e)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0279: Invalid comparison between Unknown and I4
			//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fd: Expected O, but got Unknown
			pictureBoxStatusConnect.set_Image((Image)null);
			((Control)pictureBoxStatusConnect).Update();
			try
			{
				char[] separator = new char[1] { '-' };
				string[] array = ((Control)textBoxKeypadString).get_Text().Split(separator, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 2)
				{
					pictureBoxStatusConnect.set_Image((Image)Resources.error);
					MessageBox.Show(Resources.InvalidKeypadRange);
					return;
				}
				int num = 0;
				int num2 = 0;
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					short result;
					if (short.TryParse(array2[i], out result))
					{
						if (result < num || num == 0)
						{
							num = result;
						}
						if (result > num2 || num2 == 0)
						{
							num2 = result;
						}
					}
				}
				if (_programmer != null)
				{
					Unhook();
					_programmer.Deactivate();
					_programmer.Disconnect();
				}
				if (((ListControl)comboBoxReceiver).get_SelectedIndex() == _onAirBuzzerIndex)
				{
					_programmer = new OnAirProgrammer();
					((Control)groupBoxAutomatic).set_Visible(false);
					LoadOnAirPairingUI();
				}
				else
				{
					if (_onAirPairingUI != null)
					{
						((Control)_onAirPairingUI).set_Visible(false);
						((Control)groupBoxAutomatic).set_Visible(true);
					}
					switch (((ListControl)comboBoxReceiver).get_SelectedIndex())
					{
					case 0:
						_programmer = new EnjoyProgrammer();
						break;
					case 1:
						_programmer = new PlusProgrammer();
						break;
					case 2:
						_programmer = new BuzzerProgrammer();
						break;
					case 4:
						_programmer = new GalaxyBuzzerProgrammer(GalaxyBuzzerProgrammer.GalaxyBuzzerType.Torch305V);
						break;
					case 3:
						_programmer = new GalaxyBuzzerProgrammer(GalaxyBuzzerProgrammer.GalaxyBuzzerType.Buzzer303S);
						break;
					case 5:
						_programmer = new ScoreboardProgrammer();
						break;
					case 6:
						_programmer = new GalaxyWristbandsProgrammer();
						break;
					}
				}
				if (num2 == 0 || num == 0 || num2 > _programmer.MaxKeypads())
				{
					MessageBox.Show(string.Format(Resources.InvalidKeypadRange, _programmer.MaxKeypads()));
					pictureBoxStatusConnect.set_Image((Image)Resources.error);
					return;
				}
				while (true)
				{
					Cursor.set_Current(Cursors.get_WaitCursor());
					if (_programmer.Connect((Form)(object)this, num2, num))
					{
						break;
					}
					if (_programmer is EnjoyProgrammer)
					{
						_programmer = new BuzzerProgrammer();
						_programmer.SetParameter("Set217Mode", true);
						if (_programmer.Connect((Form)(object)this, num2, num))
						{
							break;
						}
						_programmer = new PlusProgrammer
						{
							Is237 = true
						};
						if (_programmer.Connect((Form)(object)this, num2, num))
						{
							break;
						}
					}
					if ((int)MessageBox.Show(Resources.ConnectFailed, Resources.AppName, (MessageBoxButtons)4, (MessageBoxIcon)32) == 7)
					{
						pictureBoxStatusConnect.set_Image((Image)Resources.error);
						Cursor.set_Current(Cursors.get_Default());
						return;
					}
				}
				numericKeypadId.set_Maximum((decimal)num2);
				numericKeypadId.set_Minimum((decimal)num);
				numericKeypadId.set_Value((decimal)num);
				Hook();
				Cursor.set_Current(Cursors.get_Default());
				_timerMode = TimerMode.ModeReceiving;
				Timer val = new Timer();
				val.set_Interval(600);
				_timer = val;
				_timer.add_Tick((EventHandler)_timer_Tick);
				_timer.Start();
				pictureBoxKeypad.set_Image((Image)_programmer.GetNeutralPicture());
				EnableAll();
				((Control)textBoxKeypadString).set_Enabled(false);
				((Control)buttonConnect).set_Enabled(false);
				pictureBoxStatusConnect.set_Image((Image)Resources.ok);
				((Control)groupBoxAutomatic).set_Enabled(_programmer.CanAutoProgram());
				((Control)groupBoxPresenter).set_Enabled(_programmer.CanProgramRemote());
				UpdateLabels();
			}
			catch (Exception exc)
			{
				ReportException(exc);
			}
		}

		private void LoadOnAirPairingUI()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected O, but got Unknown
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			if (_onAirPairingUI == null)
			{
				GroupBox val = new GroupBox();
				((Control)val).set_Location(((Control)groupBoxAutomatic).get_Location());
				((Control)val).set_Size(((Control)groupBoxAutomatic).get_Size());
				((Control)val).set_Text("Pairing new Buzzers");
				_onAirPairingUI = val;
				Size size = new Size(75, 26);
				((Control)this).get_Controls().Add((Control)(object)_onAirPairingUI);
				Button val2 = new Button();
				((Control)val2).set_Size(size);
				((Control)val2).set_Location(new Point(10, 24));
				((Control)val2).set_Text("Start Pairing");
				Button val3 = val2;
				((Control)val3).add_Click((EventHandler)delegate
				{
					_timerMode = TimerMode.Unknown;
					_timer.Stop();
					((Control)labelKeypadReceived).set_Text("pairing...");
					((Control)labelKeypadReceived).set_ForeColor(Color.MediumPurple);
					_programmer.SetParameter("EnablePairing", null);
				});
				((Control)_onAirPairingUI).get_Controls().Add((Control)(object)val3);
				Button val4 = new Button();
				((Control)val4).set_Location(new Point(10, 55));
				((Control)val4).set_Size(size);
				((Control)val4).set_Text("Stop Pairing");
				Button val5 = val4;
				((Control)val5).add_Click((EventHandler)delegate
				{
					_programmer.SetParameter("DisablePairing", null);
					_timerMode = TimerMode.ModeReceiving;
					_timer.Start();
				});
				((Control)_onAirPairingUI).get_Controls().Add((Control)(object)val5);
			}
			else
			{
				((Control)_onAirPairingUI).set_Visible(true);
			}
		}

		private void Hook()
		{
			_programmer.OnKeyPressed += _programmer_OnKeyPressed;
			_programmer.OnQuizMasterRemotePressed += _programmer_OnQuizMasterRemotePressed;
			_programmer.OnSetIDSucceeded += _programmer_OnSetIDSucceeded;
			_programmer.OnSetIDFailed += _programmer_OnSetIDFailed;
		}

		private void Unhook()
		{
			_programmer.OnKeyPressed -= _programmer_OnKeyPressed;
			_programmer.OnQuizMasterRemotePressed -= _programmer_OnQuizMasterRemotePressed;
			_programmer.OnSetIDSucceeded -= _programmer_OnSetIDSucceeded;
			_programmer.OnSetIDFailed -= _programmer_OnSetIDFailed;
		}

		private void ReportException(Exception exc)
		{
			ReportException(exc, Resources.AppName);
		}

		private void ReportException(Exception exc, string caption)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			string text = Path.Combine(LogFolder, "programmer." + DateTime.Now.ToString("yyyy.MM.dd") + ".log");
			try
			{
				File.AppendAllText(text, string.Format("{0}: {1}{2}", DateTime.Now.ToString(), exc.ToString(), Environment.NewLine));
			}
			catch
			{
			}
			MessageBox.Show(string.Format("{0}\n\nMore information can be found in the logfile at '{1}'", exc.Message, text), caption, (MessageBoxButtons)0, (MessageBoxIcon)16);
		}

		public Color[] GetQxButtonColorScheme()
		{
			return new Color[8]
			{
				Color.Red,
				Color.FromArgb(0, 173, 238),
				Color.FromArgb(244, 121, 34),
				Color.FromArgb(125, 194, 66),
				Color.FromArgb(244, 236, 20),
				Color.FromArgb(130, 71, 130),
				Color.FromArgb(232, 35, 47),
				Color.Gray
			};
		}

		private void comboBoxReceiver_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_programmer != null)
			{
				_programmer.Deactivate();
				_programmer.Disconnect();
				_programmer = null;
				ResetUI();
				pictureBoxStatusConnect.set_Image((Image)null);
			}
			if (((ListControl)comboBoxReceiver).get_SelectedIndex() == 5)
			{
				((Control)textBoxKeypadString).set_Text("1-10");
			}
			else if (((ListControl)comboBoxReceiver).get_SelectedIndex() == 2)
			{
				((Control)textBoxKeypadString).set_Text("1-20");
			}
			else if (((ListControl)comboBoxReceiver).get_SelectedIndex() == 0)
			{
				((Control)textBoxKeypadString).set_Text("1-50");
			}
			else if (((ListControl)comboBoxReceiver).get_SelectedIndex() == 3)
			{
				((Control)textBoxKeypadString).set_Text("1-64");
			}
			else
			{
				((Control)textBoxKeypadString).set_Text("1-100");
			}
		}

		private void pictureBoxStatusConnect_Click(object sender, EventArgs e)
		{
		}

		private void buttonSetRemote_Click(object sender, EventArgs e)
		{
			try
			{
				((Control)buttonSetRemote).set_Enabled(false);
				_programmer.Deactivate();
				if (_programmer.SetRemote())
				{
					pictureBoxKeypad.set_Image((Image)Resources.remote);
					((Control)pictureBoxKeypad).Update();
					_timerMode = TimerMode.ModeProgrammingRemote;
					_timer.set_Interval(_programmer.ProgramTimeoutMS());
					_timer.Start();
				}
				else
				{
					pictureBoxStatusRemote.set_Image((Image)Resources.error);
					((Control)buttonSetRemote).set_Enabled(true);
				}
			}
			catch (Exception exc)
			{
				ReportException(exc);
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
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected O, but got Unknown
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Expected O, but got Unknown
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Expected O, but got Unknown
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Expected O, but got Unknown
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Expected O, but got Unknown
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Expected O, but got Unknown
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Expected O, but got Unknown
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Expected O, but got Unknown
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Expected O, but got Unknown
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Expected O, but got Unknown
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Expected O, but got Unknown
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Expected O, but got Unknown
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Expected O, but got Unknown
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Expected O, but got Unknown
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Expected O, but got Unknown
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Expected O, but got Unknown
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Expected O, but got Unknown
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Expected O, but got Unknown
			//IL_0be7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bf1: Expected O, but got Unknown
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ProgrammerForm));
			groupBoxManual = new GroupBox();
			checkBoxAutoIncrement = new CheckBox();
			pictureBoxStatusManual = new PictureBox();
			buttonSet = new Button();
			labelManual = new Label();
			numericKeypadId = new NumericUpDown();
			groupBoxIndicator = new GroupBox();
			labelKeypadReceived = new Label();
			pictureBoxKeypad = new PictureBox();
			groupBoxAutomatic = new GroupBox();
			pictureBoxStatusAuto = new PictureBox();
			buttonStop = new Button();
			buttonStart = new Button();
			label3 = new Label();
			labelAutomatic = new Label();
			numericIncrement = new NumericUpDown();
			numericKeypadStart = new NumericUpDown();
			groupBox4 = new GroupBox();
			comboBoxReceiver = new ComboBox();
			pictureBoxStatusConnect = new PictureBox();
			buttonConnect = new Button();
			textBoxKeypadString = new TextBox();
			labelReceiver = new Label();
			labelKeypadString = new Label();
			buttonSetRemote = new Button();
			groupBoxPresenter = new GroupBox();
			panel1 = new Panel();
			pictureBox1 = new PictureBox();
			label5 = new Label();
			pictureBoxStatusRemote = new PictureBox();
			label4 = new Label();
			((Control)groupBoxManual).SuspendLayout();
			((ISupportInitialize)pictureBoxStatusManual).BeginInit();
			((ISupportInitialize)numericKeypadId).BeginInit();
			((Control)groupBoxIndicator).SuspendLayout();
			((ISupportInitialize)pictureBoxKeypad).BeginInit();
			((Control)groupBoxAutomatic).SuspendLayout();
			((ISupportInitialize)pictureBoxStatusAuto).BeginInit();
			((ISupportInitialize)numericIncrement).BeginInit();
			((ISupportInitialize)numericKeypadStart).BeginInit();
			((Control)groupBox4).SuspendLayout();
			((ISupportInitialize)pictureBoxStatusConnect).BeginInit();
			((Control)groupBoxPresenter).SuspendLayout();
			((Control)panel1).SuspendLayout();
			((ISupportInitialize)pictureBox1).BeginInit();
			((ISupportInitialize)pictureBoxStatusRemote).BeginInit();
			((Control)this).SuspendLayout();
			componentResourceManager.ApplyResources(groupBoxManual, "groupBoxManual");
			((Control)groupBoxManual).get_Controls().Add((Control)(object)checkBoxAutoIncrement);
			((Control)groupBoxManual).get_Controls().Add((Control)(object)pictureBoxStatusManual);
			((Control)groupBoxManual).get_Controls().Add((Control)(object)buttonSet);
			((Control)groupBoxManual).get_Controls().Add((Control)(object)labelManual);
			((Control)groupBoxManual).get_Controls().Add((Control)(object)numericKeypadId);
			((Control)groupBoxManual).set_Name("groupBoxManual");
			groupBoxManual.set_TabStop(false);
			componentResourceManager.ApplyResources(checkBoxAutoIncrement, "checkBoxAutoIncrement");
			((Control)checkBoxAutoIncrement).set_Name("checkBoxAutoIncrement");
			((ButtonBase)checkBoxAutoIncrement).set_UseVisualStyleBackColor(true);
			componentResourceManager.ApplyResources(pictureBoxStatusManual, "pictureBoxStatusManual");
			((Control)pictureBoxStatusManual).set_Name("pictureBoxStatusManual");
			pictureBoxStatusManual.set_TabStop(false);
			componentResourceManager.ApplyResources(buttonSet, "buttonSet");
			((Control)buttonSet).set_Name("buttonSet");
			((ButtonBase)buttonSet).set_UseVisualStyleBackColor(true);
			((Control)buttonSet).add_Click((EventHandler)buttonSet_Click);
			componentResourceManager.ApplyResources(labelManual, "labelManual");
			((Control)labelManual).set_Name("labelManual");
			componentResourceManager.ApplyResources(numericKeypadId, "numericKeypadId");
			numericKeypadId.set_Maximum(new decimal(new int[4] { 400, 0, 0, 0 }));
			numericKeypadId.set_Minimum(new decimal(new int[4] { 1, 0, 0, 0 }));
			((Control)numericKeypadId).set_Name("numericKeypadId");
			numericKeypadId.set_Value(new decimal(new int[4] { 1, 0, 0, 0 }));
			componentResourceManager.ApplyResources(groupBoxIndicator, "groupBoxIndicator");
			((Control)groupBoxIndicator).get_Controls().Add((Control)(object)labelKeypadReceived);
			((Control)groupBoxIndicator).set_Name("groupBoxIndicator");
			groupBoxIndicator.set_TabStop(false);
			componentResourceManager.ApplyResources(labelKeypadReceived, "labelKeypadReceived");
			((Control)labelKeypadReceived).set_Name("labelKeypadReceived");
			componentResourceManager.ApplyResources(pictureBoxKeypad, "pictureBoxKeypad");
			pictureBoxKeypad.set_Image((Image)Resources.keypad);
			((Control)pictureBoxKeypad).set_Name("pictureBoxKeypad");
			pictureBoxKeypad.set_TabStop(false);
			componentResourceManager.ApplyResources(groupBoxAutomatic, "groupBoxAutomatic");
			((Control)groupBoxAutomatic).get_Controls().Add((Control)(object)pictureBoxStatusAuto);
			((Control)groupBoxAutomatic).get_Controls().Add((Control)(object)buttonStop);
			((Control)groupBoxAutomatic).get_Controls().Add((Control)(object)buttonStart);
			((Control)groupBoxAutomatic).get_Controls().Add((Control)(object)label3);
			((Control)groupBoxAutomatic).get_Controls().Add((Control)(object)labelAutomatic);
			((Control)groupBoxAutomatic).get_Controls().Add((Control)(object)numericIncrement);
			((Control)groupBoxAutomatic).get_Controls().Add((Control)(object)numericKeypadStart);
			((Control)groupBoxAutomatic).set_Name("groupBoxAutomatic");
			groupBoxAutomatic.set_TabStop(false);
			componentResourceManager.ApplyResources(pictureBoxStatusAuto, "pictureBoxStatusAuto");
			((Control)pictureBoxStatusAuto).set_Name("pictureBoxStatusAuto");
			pictureBoxStatusAuto.set_TabStop(false);
			componentResourceManager.ApplyResources(buttonStop, "buttonStop");
			((Control)buttonStop).set_Name("buttonStop");
			((ButtonBase)buttonStop).set_UseVisualStyleBackColor(true);
			((Control)buttonStop).add_Click((EventHandler)buttonStop_Click);
			componentResourceManager.ApplyResources(buttonStart, "buttonStart");
			((Control)buttonStart).set_Name("buttonStart");
			((ButtonBase)buttonStart).set_UseVisualStyleBackColor(true);
			((Control)buttonStart).add_Click((EventHandler)buttonStart_Click);
			componentResourceManager.ApplyResources(label3, "label3");
			((Control)label3).set_Name("label3");
			componentResourceManager.ApplyResources(labelAutomatic, "labelAutomatic");
			((Control)labelAutomatic).set_Name("labelAutomatic");
			componentResourceManager.ApplyResources(numericIncrement, "numericIncrement");
			numericIncrement.set_Minimum(new decimal(new int[4] { 1, 0, 0, 0 }));
			((Control)numericIncrement).set_Name("numericIncrement");
			numericIncrement.set_Value(new decimal(new int[4] { 1, 0, 0, 0 }));
			componentResourceManager.ApplyResources(numericKeypadStart, "numericKeypadStart");
			numericKeypadStart.set_Maximum(new decimal(new int[4] { 400, 0, 0, 0 }));
			numericKeypadStart.set_Minimum(new decimal(new int[4] { 1, 0, 0, 0 }));
			((Control)numericKeypadStart).set_Name("numericKeypadStart");
			numericKeypadStart.set_Value(new decimal(new int[4] { 1, 0, 0, 0 }));
			componentResourceManager.ApplyResources(groupBox4, "groupBox4");
			((Control)groupBox4).get_Controls().Add((Control)(object)comboBoxReceiver);
			((Control)groupBox4).get_Controls().Add((Control)(object)pictureBoxStatusConnect);
			((Control)groupBox4).get_Controls().Add((Control)(object)buttonConnect);
			((Control)groupBox4).get_Controls().Add((Control)(object)textBoxKeypadString);
			((Control)groupBox4).get_Controls().Add((Control)(object)labelReceiver);
			((Control)groupBox4).get_Controls().Add((Control)(object)labelKeypadString);
			((Control)groupBox4).set_Name("groupBox4");
			groupBox4.set_TabStop(false);
			componentResourceManager.ApplyResources(comboBoxReceiver, "comboBoxReceiver");
			comboBoxReceiver.set_DropDownStyle((ComboBoxStyle)2);
			((ListControl)comboBoxReceiver).set_FormattingEnabled(true);
			comboBoxReceiver.get_Items().AddRange(new object[7]
			{
				componentResourceManager.GetString("comboBoxReceiver.Items"),
				componentResourceManager.GetString("comboBoxReceiver.Items1"),
				componentResourceManager.GetString("comboBoxReceiver.Items2"),
				componentResourceManager.GetString("comboBoxReceiver.Items3"),
				componentResourceManager.GetString("comboBoxReceiver.Items4"),
				componentResourceManager.GetString("comboBoxReceiver.Items5"),
				componentResourceManager.GetString("comboBoxReceiver.Items6")
			});
			((Control)comboBoxReceiver).set_Name("comboBoxReceiver");
			comboBoxReceiver.add_SelectedIndexChanged((EventHandler)comboBoxReceiver_SelectedIndexChanged);
			componentResourceManager.ApplyResources(pictureBoxStatusConnect, "pictureBoxStatusConnect");
			((Control)pictureBoxStatusConnect).set_Name("pictureBoxStatusConnect");
			pictureBoxStatusConnect.set_TabStop(false);
			((Control)pictureBoxStatusConnect).add_Click((EventHandler)pictureBoxStatusConnect_Click);
			componentResourceManager.ApplyResources(buttonConnect, "buttonConnect");
			((Control)buttonConnect).set_Name("buttonConnect");
			((ButtonBase)buttonConnect).set_UseVisualStyleBackColor(true);
			((Control)buttonConnect).add_Click((EventHandler)buttonConnect_Click);
			componentResourceManager.ApplyResources(textBoxKeypadString, "textBoxKeypadString");
			((Control)textBoxKeypadString).set_Name("textBoxKeypadString");
			componentResourceManager.ApplyResources(labelReceiver, "labelReceiver");
			((Control)labelReceiver).set_Name("labelReceiver");
			componentResourceManager.ApplyResources(labelKeypadString, "labelKeypadString");
			((Control)labelKeypadString).set_Name("labelKeypadString");
			componentResourceManager.ApplyResources(buttonSetRemote, "buttonSetRemote");
			((Control)buttonSetRemote).set_Name("buttonSetRemote");
			((ButtonBase)buttonSetRemote).set_UseVisualStyleBackColor(true);
			((Control)buttonSetRemote).add_Click((EventHandler)buttonSetRemote_Click);
			componentResourceManager.ApplyResources(groupBoxPresenter, "groupBoxPresenter");
			((Control)groupBoxPresenter).get_Controls().Add((Control)(object)panel1);
			((Control)groupBoxPresenter).get_Controls().Add((Control)(object)pictureBoxStatusRemote);
			((Control)groupBoxPresenter).get_Controls().Add((Control)(object)label4);
			((Control)groupBoxPresenter).get_Controls().Add((Control)(object)buttonSetRemote);
			((Control)groupBoxPresenter).set_Name("groupBoxPresenter");
			groupBoxPresenter.set_TabStop(false);
			componentResourceManager.ApplyResources(panel1, "panel1");
			((Control)panel1).set_BackColor(SystemColors.Info);
			((Control)panel1).get_Controls().Add((Control)(object)pictureBox1);
			((Control)panel1).get_Controls().Add((Control)(object)label5);
			((Control)panel1).set_Name("panel1");
			componentResourceManager.ApplyResources(pictureBox1, "pictureBox1");
			pictureBox1.set_Image((Image)Resources.info);
			((Control)pictureBox1).set_Name("pictureBox1");
			pictureBox1.set_TabStop(false);
			componentResourceManager.ApplyResources(label5, "label5");
			((Control)label5).set_Name("label5");
			componentResourceManager.ApplyResources(pictureBoxStatusRemote, "pictureBoxStatusRemote");
			((Control)pictureBoxStatusRemote).set_Name("pictureBoxStatusRemote");
			pictureBoxStatusRemote.set_TabStop(false);
			componentResourceManager.ApplyResources(label4, "label4");
			((Control)label4).set_Name("label4");
			((Form)this).set_AcceptButton((IButtonControl)(object)buttonSet);
			componentResourceManager.ApplyResources(this, "$this");
			((ContainerControl)this).set_AutoScaleMode((AutoScaleMode)1);
			((Control)this).get_Controls().Add((Control)(object)groupBoxPresenter);
			((Control)this).get_Controls().Add((Control)(object)groupBox4);
			((Control)this).get_Controls().Add((Control)(object)pictureBoxKeypad);
			((Control)this).get_Controls().Add((Control)(object)groupBoxIndicator);
			((Control)this).get_Controls().Add((Control)(object)groupBoxAutomatic);
			((Control)this).get_Controls().Add((Control)(object)groupBoxManual);
			((Form)this).set_FormBorderStyle((FormBorderStyle)3);
			((Form)this).set_MaximizeBox(false);
			((Form)this).set_MinimizeBox(false);
			((Control)this).set_Name("ProgrammerForm");
			((Form)this).add_FormClosing(new FormClosingEventHandler(ProgrammerForm_FormClosing));
			((Form)this).add_Load((EventHandler)ProgrammerForm_Load);
			((Control)groupBoxManual).ResumeLayout(false);
			((Control)groupBoxManual).PerformLayout();
			((ISupportInitialize)pictureBoxStatusManual).EndInit();
			((ISupportInitialize)numericKeypadId).EndInit();
			((Control)groupBoxIndicator).ResumeLayout(false);
			((Control)groupBoxIndicator).PerformLayout();
			((ISupportInitialize)pictureBoxKeypad).EndInit();
			((Control)groupBoxAutomatic).ResumeLayout(false);
			((Control)groupBoxAutomatic).PerformLayout();
			((ISupportInitialize)pictureBoxStatusAuto).EndInit();
			((ISupportInitialize)numericIncrement).EndInit();
			((ISupportInitialize)numericKeypadStart).EndInit();
			((Control)groupBox4).ResumeLayout(false);
			((Control)groupBox4).PerformLayout();
			((ISupportInitialize)pictureBoxStatusConnect).EndInit();
			((Control)groupBoxPresenter).ResumeLayout(false);
			((Control)panel1).ResumeLayout(false);
			((ISupportInitialize)pictureBox1).EndInit();
			((ISupportInitialize)pictureBoxStatusRemote).EndInit();
			((Control)this).ResumeLayout(false);
		}
	}
}
