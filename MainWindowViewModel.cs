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

		public ObservableRangeCollection<CommandDescription> CommandDescriptions
		{
			get;
			set;
		} = new ObservableRangeCollection<CommandDescription>();

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
			InitializeCommandDescriptions();
		}

		#endregion constructor / destructor

		#region methods

		public void InitializeCommandDescriptions()
		{
			CommandDescriptions = new ObservableRangeCollection<CommandDescription>()
			{
				new CommandDescription() { Name="add", Example="add $t1 $t2 $t1", Parameter1="Source Register", Parameter2="Source Register", Parameter3="Destination Register", Description="Add two registers together and output to destination register" },
				new CommandDescription() { Name="addi", Example="addi $t1 20 $t1 or add 20 10 $t1", Parameter1="Source Register/Immediate", Parameter2="Source Register/Immediate", Parameter3="Destination Register", Description="Add a register and one immediate or two immediates together and output to destination register" },
				new CommandDescription() { Name="subtract", Example="subtract $t1 2 $t1 or subtract 20 10 $t1", Parameter1="Source Register/Immediate Minuend", Parameter2="Source Register/Immediate Subtrahend", Parameter3="Destination Register", Description="Subtract two registers/immediates and output to destination register" },
				new CommandDescription() { Name="multiply", Example="multiply $t1 2 $t1 or multiply 20 10 $t1", Parameter1="Source Register/Immediate", Parameter2="Source Register/Immediate", Parameter3="Destination Register", Description="Multiply two registers/immediates and output to destination register" },
				new CommandDescription() { Name="divide", Example="divide $t1 2 $t1 or divide 20 10 $t1", Parameter1="Source Register/Immediate Dividend", Parameter2="Source Register/Immediate Divisor", Parameter3="Destination Register", Description="Divide two registers/immediates and output to destination register" },
				new CommandDescription() { Name="or", Example="or $t1 2 $t1 or or 20 10 $t1", Parameter1="Source Register/Immediate", Parameter2="Source Register/Immediate", Parameter3="Destination Register", Description="Bitwise or two registers/immediates and output to destination register" },
				new CommandDescription() { Name="nor", Example="nor $t1 2 $t1 or nor 20 10 $t1", Parameter1="Source Register/Immediate", Parameter2="Source Register/Immediate", Parameter3="Destination Register", Description="Bitwise nor two registers/immediates and output to destination register" },
				new CommandDescription() { Name="xor", Example="xor $t1 2 $t1 or xor 20 10 $t1", Parameter1="Source Register/Immediate", Parameter2="Source Register/Immediate", Parameter3="Destination Register", Description="Bitwise xor two registers/immediates and output to destination register" },
				new CommandDescription() { Name="and", Example="and $t1 2 $t1 or and 20 10 $t1", Parameter1="Source Register/Immediate", Parameter2="Source Register/Immediate", Parameter3="Destination Register", Description="Bitwise and two registers/immediates and output to destination register" },
				new CommandDescription() { Name="branchIf<", Example="branchIf< $t1 2 $t1", Parameter1="Left Source Register", Parameter2="Right Source Register", Parameter3="Destination Register", Description="Compare two registers and branch if the left register is less than the right register" },
				new CommandDescription() { Name="branchIf>", Example="branchIf> $t1 2 $t1", Parameter1="Left Source Register", Parameter2="Right Source Register", Parameter3="Destination Register", Description="Compare two registers and branch if the left register is greater than the right register" },
				new CommandDescription() { Name="branchIf<=", Example="branchIf<= $t1 2 $t1", Parameter1="Left Source Register", Parameter2="Right Source Register", Parameter3="Destination Register", Description="Compare two registers and branch if the left register is less than or equal to the right register" },
				new CommandDescription() { Name="branchIf>=", Example="branchIf>= $t1 2 $t1", Parameter1="Left Source Register", Parameter2="Right Source Register", Parameter3="Destination Register", Description="Compare two registers and branch if the left register is greater than or equal to the right register" },
				new CommandDescription() { Name="branchIf==", Example="branchIf== $t1 2 $t1", Parameter1="Left Source Register", Parameter2="Right Source Register", Parameter3="Destination Register", Description="Compare two registers and branch if the left register is equal to the right register" },
				new CommandDescription() { Name="shiftLeftLogical", Example="shiftLeftLogical 2 $t1 $t2", Parameter1="Immediate Shift Amount", Parameter2="Source Register", Parameter3="Destination Register", Description="Shift the Source Register Left by the Amount indicated and store in the destination register" },
				new CommandDescription() { Name="shiftRightLogical", Example="shiftRightLogical 2 $t1 $t2", Parameter1="Immediate Shift Amount", Parameter2="Source Register", Parameter3="Destination Register", Description="Shift the Source Register Right by the Amount indicated and store in the destination register" },
				new CommandDescription() { Name="loadImmediate", Example="loadImmediate $t1 2", Parameter1="Destination Register", Parameter2="Immediate", Description="Load the immediate in the destination register" },
				new CommandDescription() { Name="branch", Example="branch <BranchName>", Parameter1="String of the Branch Name", Description="Go to the indicated Branch" },
				new CommandDescription() { Name="<BranchName>;", Example="<BranchName>;", Parameter1="String of the Branch Name", Description="Create a branch with the passed in name. Note must end with ;" },
			};
		}

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

			TabIndex = 0;
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
				new Register() { Name="$pc", Number=32, EnumID=RegisterEnums.pc, ShowDecimalValue=Visibility.Collapsed, ShowRegisterNumber = Visibility.Collapsed, Value=4194304 },
				new Register() { Name="$hi", Number=33, EnumID=RegisterEnums.hi, ShowRegisterNumber = Visibility.Collapsed},
				new Register() { Name="$lo", Number=34, EnumID=RegisterEnums.lo, ShowRegisterNumber = Visibility.Collapsed},
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
			TabIndex = 0;
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
			TabIndex = 1;
			while (_currentOperation < AllOperations.Count)
			{
				RunCodeStep();
			}

			CommandIndex = -1;
			RegisterIndex = -1;
		}

		private void RunCodeOneStep()
		{
			TabIndex = 1;
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
			CommandIndex = _currentOperation;

			if (command.IsBranch && command.OpCodeToGoTo >= AllOperations.Count)
			{
				_currentOperation = AllOperations.Count;
				return;
			}

			Registers[(int)RegisterEnums.pc].Value = command.IsBranch ? AllOperations[command.OpCodeToGoTo].Address : command.Address;
			RegisterIndex = command.IsBranch && command.DestinationRegister == null ? -1 : command.DestinationRegister.Number;
			command.CodeToRun();
			_currentOperation = command.IsBranch ? command.OpCodeToGoTo : _currentOperation + 1;
		}

		private void SaveFile()
		{
			File.WriteAllText(m_fullFilePath, MainFile);
		}

		#endregion methods
	}
}