using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.IO;
using System.Windows;

namespace ProcessorSimulator.VM
{
	public class MainWindowViewModel : BaseViewModel
	{
		#region member variables

		private int _addressIndex = 0;
		private int _commandIndex = -1;
		private int _currentOperation = 0;
		private string _fileName = "";
		private bool _isFileLoaded;
		private bool _isFileParsed;
		private string _mainFile = "Load File to Edit";
		private int _registerIndex = -1;
		private int _tabIndex = 0;
		private string m_fullFilePath = "";

		#endregion member variables

		#region properties

		public int AddressIndex
		{
			get => _addressIndex;
			set
			{
				_addressIndex = value;
				OnPropertyChanged(nameof(AddressIndex));
			}
		}

		public ObservableRangeCollection<Operation> AllOperations
		{
			get;
			set;
		} = new ObservableRangeCollection<Operation>();

		public int CommandIndex
		{
			get => _commandIndex;
			set
			{
				_commandIndex = value;
				OnPropertyChanged(nameof(CommandIndex));
			}
		}

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

		public bool IsFileParsed
		{
			get => _isFileParsed && _isFileLoaded;
			set
			{
				_isFileParsed = value;
				OnPropertyChanged(nameof(IsFileParsed));
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

		public int RegisterIndex
		{
			get => _registerIndex;
			set
			{
				_registerIndex = value;
				OnPropertyChanged();
			}
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

		public int TabIndex
		{
			get => _tabIndex;
			set
			{
				_tabIndex = value;
				OnPropertyChanged(nameof(TabIndex));
			}
		}

		#endregion properties

		#region Commands

		public Command CreateNewFileCommand
		{
			get;
			set;
		}

		public Command OpenCommand
		{
			get;
			set;
		}

		public Command ParseCommands
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
			InitializeRegisters();
			InitializeCommands();
		}

		#endregion constructor / destructor

		#region methods

		private void Clear()
		{
			AllOperations.Clear();
			ProcessCommands.Clear();
			Registers[(int)RegisterEnums.pc].ResetPC();
			foreach (var register in Registers)
			{
				register.ClearValues();
			}
			_currentOperation = 0;
			RegisterIndex = -1;
			CommandIndex = -1;
		}

		private void CreateNewFile()
		{
			SaveFileDialog sfd = new SaveFileDialog()
			{
				DefaultExt = ".sjw",
				FileName = "ShouldJustWork",
				Filter = "Simulator Just Works Files (.sjw)|*.sjw",
				OverwritePrompt = true,
				Title = "Save Glorious File"
			};

			if (sfd.ShowDialog() == true)
			{
				File.WriteAllText(sfd.FileName, "");
				OpenFileWithPath(sfd.FileName, sfd.SafeFileName);
			}
		}

		private void InitializeCommands()
		{
			OpenCommand = new Command(() => OpenFile());
			SaveCommand = new Command(() => SaveFile());
			RunCommand = new Command(() => RunCode());
			RunOneCommand = new Command(() => RunCodeOneStep());
			ParseCommands = new Command(() => ParseAllCommands());
			ProcessCommands = new ProcessCommands(Registers, AllOperations) { SnackBoxMessage = SnackBoxMessage };
			CreateNewFileCommand = new Command(() => CreateNewFile());
		}

		private void InitializeRegisters()
		{
			Registers = new ObservableRangeCollection<Register>()
			{
				new Register() { Name="$zero", Number=0, EnumID=RegisterEnums.Zero},
				new Register() { Name="$at", Number=1, EnumID=RegisterEnums.at},
				new Register() { Name="$v0", Number=2, EnumID=RegisterEnums.vo},
				new Register() { Name="$v1", Number=3, EnumID=RegisterEnums.v1},
				new Register() { Name="$a0", Number=4, EnumID=RegisterEnums.a0},
				new Register() { Name="$a1", Number=5, EnumID=RegisterEnums.a1},
				new Register() { Name="$a2", Number=6, EnumID=RegisterEnums.a2},
				new Register() { Name="$a3", Number=7, EnumID=RegisterEnums.a3},
				new Register() { Name="$t0", Number=8, EnumID=RegisterEnums.t0},
				new Register() { Name="$t1", Number=9, EnumID=RegisterEnums.t1},
				new Register() { Name="$t2", Number=10, EnumID=RegisterEnums.t2},
				new Register() { Name="$t3", Number=11, EnumID=RegisterEnums.t3},
				new Register() { Name="$t4", Number=12, EnumID=RegisterEnums.t4},
				new Register() { Name="$t5", Number=13, EnumID=RegisterEnums.t5},
				new Register() { Name="$t6", Number=14, EnumID=RegisterEnums.t6},
				new Register() { Name="$t7", Number=15, EnumID=RegisterEnums.t7},
				new Register() { Name="$s0", Number=16, EnumID=RegisterEnums.s0},
				new Register() { Name="$s1", Number=17, EnumID=RegisterEnums.s1},
				new Register() { Name="$s2", Number=18, EnumID=RegisterEnums.s2},
				new Register() { Name="$s3", Number=19, EnumID=RegisterEnums.s3},
				new Register() { Name="$s4", Number=20, EnumID=RegisterEnums.s4},
				new Register() { Name="$s5", Number=21, EnumID=RegisterEnums.s5},
				new Register() { Name="$s6", Number=22, EnumID=RegisterEnums.s6},
				new Register() { Name="$s7", Number=23, EnumID=RegisterEnums.s7},
				new Register() { Name="$t8", Number=24, EnumID=RegisterEnums.t8},
				new Register() { Name="$t9", Number=25, EnumID=RegisterEnums.t9},
				new Register() { Name="$k0", Number=26, EnumID=RegisterEnums.k0},
				new Register() { Name="$k1", Number=27, EnumID=RegisterEnums.k1},
				new Register() { Name="$gp", Number=28, EnumID=RegisterEnums.gp},
				new Register() { Name="$sp", Number=29, EnumID=RegisterEnums.sp},
				new Register() { Name="$fp", Number=30, EnumID=RegisterEnums.fp},
				new Register() { Name="$ra", Number=31, EnumID=RegisterEnums.ra},
				new Register() { Name="$pc", Number=32, EnumID=RegisterEnums.pc, ShowDecimalValue=Visibility.Collapsed, Value=4194304 },
				new Register() { Name="$hi", Number=33, EnumID=RegisterEnums.hi},
				new Register() { Name="$lo", Number=34, EnumID=RegisterEnums.lo},
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
				OpenFileWithPath(openFileDialog.FileName, openFileDialog.SafeFileName);
			}
		}

		private void OpenFileWithPath(string _fullFilePathName, string _fileName)
		{
			m_fullFilePath = _fullFilePathName;
			FileName = _fileName;
			StreamReader streamReader = new StreamReader(m_fullFilePath);
			MainFile = streamReader.ReadToEnd();
			IsFileLoaded = true;
			streamReader.Close();
			Clear();
			IsFileParsed = false;
		}

		private void ParseAllCommands()
		{
			SaveFile();
			Clear();
			ParseText();
			IsFileParsed = true;
			TabIndex = 1;
		}

		private void ParseText()
		{
			ProcessCommands.CurrentLine = 0;

			foreach (string line in MainFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (!ProcessCommands.BuildCommand(line))
				{
					break;
				}
			}

			ProcessCommands.GoThroughBranchesThatDidNotExist();

			Registers[(int)RegisterEnums.pc].ResetPC();
			foreach (var register in Registers)
			{
				register.ClearValues();
			}
		}

		private void RunCode()
		{
			while (_currentOperation < AllOperations.Count)
			{
				RunCodeStep();
			}

			CommandIndex = -1;
			RegisterIndex = -1;
		}

		private void RunCodeOneStep()
		{
			if (_currentOperation < AllOperations.Count)
			{
				RunCodeStep();
			}
			else
			{
				CommandIndex = -1;
				RegisterIndex = -1;
			}
		}

		private void RunCodeStep()
		{
			var command = AllOperations[_currentOperation];
			Registers[(int)RegisterEnums.pc].Value = command.IsBranch ? AllOperations[command.OpCodeToGoTo].Address : command.Address;
			RegisterIndex = command.IsBranch && command.DestinationRegister == null ? -1 : command.DestinationRegister.Number;
			command.CodeToRun();
			CommandIndex = _currentOperation;
			_currentOperation = command.IsBranch ? command.OpCodeToGoTo : _currentOperation + 1;
		}

		private void SaveFile()
		{
			File.WriteAllText(m_fullFilePath, MainFile);
		}

		#endregion methods
	}
}