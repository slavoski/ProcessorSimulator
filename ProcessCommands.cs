using MaterialDesignThemes.Wpf;
using MvvmHelpers;
using System.Linq;

namespace ProcessorSimulator
{
	public class ProcessCommands
	{
		#region Properties

		private ObservableRangeCollection<Register> Registers
		{
			get;
			set;
		}

		public SnackbarMessageQueue SnackBoxMessage
		{
			get;
			set;
		} = new SnackbarMessageQueue();

		#endregion Properties

		#region constructor / destructor

		public ProcessCommands(ObservableRangeCollection<Register> registers)
		{
			Registers = registers;
		}

		#endregion constructor / destructor

		#region methods

		public bool BuildCommand(string textCommand)
		{
			var breakOnSpaces = textCommand.Split(" ");
			bool successful = true;

			if (breakOnSpaces.Length >= 5)
			{
				successful = BuildFiveParameterCommand(breakOnSpaces);
			}
			else if (breakOnSpaces.Length == 4)
			{
				successful = BuildFourParameterCommand(breakOnSpaces);
			}
			else if (breakOnSpaces.Length == 3)
			{
				successful = BuildThreeParameterCommand(breakOnSpaces);
			}
			else if (breakOnSpaces.Length == 2)
			{
				successful = BuildTwoParameterCommand(breakOnSpaces);
			}
			else
			{
				SnackBoxMessage.Enqueue($"Command ' {breakOnSpaces[0]} ' does not have enough parameters, Exiting Run");
				successful = false;
			}

			return successful;
		}

		private bool BuildFiveParameterCommand(string[] commandParameters)
		{
			return true;
		}

		private bool BuildFourParameterCommand(string[] commandParameters)
		{
			bool successful = true;
			var function = commandParameters[0];
			var parameter1 = commandParameters[1];
			var parameter2 = commandParameters[2];
			var parameter3 = commandParameters[3];

			switch (function)
			{
				case "add":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);

						register3.Value = register1.Value + register2.Value;
					}
					break;

				default:
					{
						SnackBoxMessage.Enqueue($"Command ' {function} ' does not exist , Exiting Run");
						successful = false;
					}
					break;
			}
			return successful;
		}

		private bool BuildThreeParameterCommand(string[] commandParameters)
		{
			bool successful = true;
			var function = commandParameters[0];
			var parameter1 = commandParameters[1];
			var parameter2 = commandParameters[2];

			switch (function)
			{
				case "loadImmediate":
					{
						var register1 = GetRegister(parameter1);

						register1.Value = int.Parse(parameter2);
					}
					break;

				default:
					{
						SnackBoxMessage.Enqueue($"Command ' {function} ' does not exist , Exiting Run");
						successful = false;
					}
					break;
			}
			return successful;
		}

		private bool BuildTwoParameterCommand(string[] commandParameters)
		{
			return true;
		}

		private Register GetRegister(string registerName)
		{
			var register = Registers.Where(p => string.Compare(p.Name, registerName) == 0).FirstOrDefault();

			if (register == null)
			{
				SnackBoxMessage.Enqueue($"Register Name ' {registerName} ' does not exist , Exiting Run");
			}
			return register;
		}

		#endregion methods
	}
}