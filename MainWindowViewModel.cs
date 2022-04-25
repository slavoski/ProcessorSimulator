using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.IO;

namespace ProcessorSimulator.VM
{
	public class MainWindowViewModel : BaseViewModel
	{
		#region member variables

		private string _fileName = "";
		private string _fullFilePath = "";
		private bool _isFileLoaded;
		private string _mainFile = "Load File to Edit";
		private string _snackBoxMessage = "";

		#endregion member variables

		#region properties

		public string FileName
		{
			get => _fileName;
			set
			{
				_fileName = value;
				OnPropertyChanged(nameof(FileName));
			}
		}

		public bool IsFileLoaded
		{
			get => _isFileLoaded;
			set
			{
				_isFileLoaded = value;
				OnPropertyChanged(nameof(IsFileLoaded));
			}
		}

		public string MainFile
		{
			get => _mainFile;
			set
			{
				_mainFile = value;
				OnPropertyChanged(nameof(MainFile));
			}
		}

		public ProcessCommands ProcessCommands
		{
			get;
			set;
		}

		public ObservableRangeCollection<Register> Registers
		{
			get;
			set;
		} = new ObservableRangeCollection<Register>();

		public SnackbarMessageQueue SnackBoxMessage
		{
			get;
			set;
		} = new SnackbarMessageQueue();

		#endregion properties

		#region Commands

		public Command OpenCommand
		{
			get;
			set;
		}

		public Command RunCommand
		{
			get;
			set;
		}

		public Command RunOneCommand
		{
			get;
			set;
		}

		public Command SaveCommand
		{
			get;
			set;
		}

		#endregion Commands

		#region constructor / destructor

		public MainWindowViewModel()
		{
			InitializeCommands();
			InitializeRegisters();
			ProcessCommands = new ProcessCommands(Registers) { SnackBoxMessage = SnackBoxMessage };
		}

		#endregion constructor / destructor

		#region methods

		private void BuildCallStack()
		{
		}

		private void InitializeCommands()
		{
			OpenCommand = new Command(() => OpenFile());
			SaveCommand = new Command(() => SaveFile());
			RunCommand = new Command(() => RunCode());
			RunOneCommand = new Command(() => RunCodeOneStep());
		}

		private void InitializeRegisters()
		{
			Registers = new ObservableRangeCollection<Register>()
			{
				new Register() { Name="$zero", Number="0" },
				new Register() { Name="$at", Number="1" },
				new Register() { Name="$v0", Number="2"},
				new Register() { Name="$v1", Number="3"},
				new Register() { Name="$a0", Number="4"},
				new Register() { Name="$a1", Number="5"},
				new Register() { Name="$a2", Number="6"},
				new Register() { Name="$a3", Number="7"},
				new Register() { Name="$t0", Number="8"},
				new Register() { Name="$t1", Number="9"},
				new Register() { Name="$t2", Number="10"},
				new Register() { Name="$t3", Number="11"},
				new Register() { Name="$t4", Number="12"},
				new Register() { Name="$t5", Number="13"},
				new Register() { Name="$t6", Number="14"},
				new Register() { Name="$t7", Number="15"},
				new Register() { Name="$t8", Number="16"},
				new Register() { Name="$t9", Number="17"},
				new Register() { Name="$s0", Number="18"},
				new Register() { Name="$s1", Number="19"},
				new Register() { Name="$s2", Number="20"},
				new Register() { Name="$s3", Number="21"},
				new Register() { Name="$s4", Number="22"},
				new Register() { Name="$s5", Number="23"},
				new Register() { Name="$s6", Number="24"},
				new Register() { Name="$s7", Number="25"},
				new Register() { Name="$k0", Number="26"},
				new Register() { Name="$k1", Number="27"},
				new Register() { Name="$gp", Number="28"},
				new Register() { Name="$sp", Number="29"},
				new Register() { Name="$fp", Number="30"},
				new Register() { Name="$ra", Number="31"},
				new Register() { Name="$pc", },
				new Register() { Name="$hi", },
				new Register() { Name="$lo", },
			};
		}

		private void OpenFile()
		{
			var openFileDialog = new OpenFileDialog()
			{
				DefaultExt = ".sjw",
				Filter = "Simulator Just Works Files (.sjw)|*.sjw",
				Multiselect = false
			};

			if (openFileDialog.ShowDialog() == true)
			{
				_fullFilePath = openFileDialog.FileName;
				FileName = openFileDialog.SafeFileName;

				StreamReader streamReader = new StreamReader(_fullFilePath);
				MainFile = streamReader.ReadToEnd();
				IsFileLoaded = true;
				streamReader.Close();
			}
		}

		private void ParseText()
		{
			foreach (string line in MainFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (!ProcessCommands.BuildCommand(line))
				{
					break;
				}
			}
		}

		private void RunCode()
		{
			SaveFile();
			ParseText();
		}

		private void RunCodeOneStep()
		{
			SaveFile();
			ParseText();
		}

		private void SaveFile()
		{
			File.WriteAllText(_fullFilePath, MainFile);
		}

		#endregion methods
	}
}