using MaterialDesignThemes.Wpf;
using MvvmHelpers;
using System.Collections.Generic;
using System.Linq;

namespace ProcessorSimulator
{
	public class ProcessCommands
	{
		#region Properties

		public ObservableRangeCollection<Operation> AllOperations
		{
			get;
			set;
		}

		public int CurrentLine
		{
			get;
			set;
		} = 0;

		public SnackbarMessageQueue SnackBoxMessage
		{
			get;
			set;
		} = new SnackbarMessageQueue();

		private List<BranchInfo> Branches
		{
			get;
			set;
		} = new List<BranchInfo>();

		private ObservableRangeCollection<Register> Registers
		{
			get;
			set;
		}

		#endregion Properties

		#region constructor / destructor

		public ProcessCommands(ObservableRangeCollection<Register> registers, ObservableRangeCollection<Operation> operations)
		{
			Registers = registers;
			AllOperations = operations;
		}

		#endregion constructor / destructor

		#region methods

		public bool BuildCommand(string textCommand)
		{
			bool successful = true;
			try
			{
				CurrentLine++;

				var breakOnSpaces = textCommand.Split(" ");

				if (breakOnSpaces.Length >= 5)
				{
					successful = BuildFiveParameterCommand(breakOnSpaces, textCommand);
				}
				else if (breakOnSpaces.Length == 4)
				{
					successful = BuildFourParameterCommand(breakOnSpaces, textCommand);
				}
				else if (breakOnSpaces.Length == 3)
				{
					successful = BuildThreeParameterCommand(breakOnSpaces, textCommand);
				}
				else if (breakOnSpaces.Length == 2)
				{
					successful = BuildTwoParameterCommand(breakOnSpaces, textCommand);
				}
				else if (breakOnSpaces.Length == 1)
				{
					successful = BuildOneParameterCommand(breakOnSpaces, textCommand);
				}
				else
				{
					SnackBoxMessage.Enqueue($"Command ' {breakOnSpaces[0]} ' does not have enough parameters, Exiting Run");
					successful = false;
				}
			}
			catch
			{
				SnackBoxMessage.Enqueue($"Bad Parsing: Please Check the Following Line: {CurrentLine}:{textCommand}");
				successful = false;
			}
			return successful;
		}

		public void GoThroughBranchesThatDidNotExist()
		{
			var branchesToFind = AllOperations.Where(p => p.IsBranch && !p.DoesBranchExistYet).ToList();

			foreach (var operation in branchesToFind)
			{
				var branch = Branches.Where(p => string.Equals(p.Name, operation.BranchName)).FirstOrDefault();

				if (branch != null)
				{
					var opCode = 4294967040 + branch.OperationGoToIndex;

					operation.OpCode = opCode;
					operation.CodeLine = $"branch 0x{opCode:X8}";
					operation.OpCodeToGoTo = branch.OperationGoToIndex;
					operation.DoesBranchExistYet = true;
				}
				else
				{
					SnackBoxMessage.Enqueue($"Branch ' {operation.BranchName} ' does not exist , Exiting Run");
					break;
				}
			}
		}

		internal void Clear()
		{
			Branches.Clear();
		}

		private bool BuildFiveParameterCommand(string[] commandParameters, string textCommand)
		{
			return true;
		}

		private bool BuildFourParameterCommand(string[] commandParameters, string textCommand, bool writeOriginalCommand = true)
		{
			bool successful = true;
			var function = commandParameters[0];
			var parameter1 = commandParameters[1];
			var parameter2 = commandParameters[2];
			var parameter3 = commandParameters[3];
			var pcRegister = Registers[(int)RegisterEnums.pc];

			switch (function)
			{
				case "add":
				case "addi":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);

						if (register1 == null)
						{
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$at", parameter1 }, "", false);
							register1 = GetRegister("$at");
						}

						if (register2 == null)
						{
							BuildThreeParameterCommand(new string[] { "loadImmediate", parameter3, parameter2 }, "", false);
							register2 = new Register() { Value = register3.Value };
						}

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							CodeLine = $"add ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							Result = register1.Value + register2.Value,
							ResultingRegister = register3.EnumID
						});
					}
					break;

				case "subtract":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							CodeLine = $"subtract ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							Result = register1.Value - register2.Value,
							ResultingRegister = register3.EnumID
						});
					}
					break;

				case "multiply":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							CodeLine = $"multiply ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							Result = register1.Value * register2.Value,
							ResultingRegister = register3.EnumID
						});
					}
					break;

				case "divide":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							CodeLine = $"divide ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							Result = register1.Value / register2.Value,
							ResultingRegister = register3.EnumID
						});
					}
					break;

				default:
					{
						SnackBoxMessage.Enqueue($"Command ' {function} ' does not exist , Exiting Run");
						successful = false;
					}
					break;
			}

			pcRegister.IncrementPC();

			return successful;
		}

		private bool BuildOneParameterCommand(string[] commandParameters, string textCommand)
		{
			bool successful = true;
			var function = commandParameters[0];

			if (function.EndsWith(";"))
			{
				Branches.Add(new BranchInfo() { Name = function[0..^1], OperationGoToIndex = AllOperations.Count });
			}

			return successful;
		}

		private bool BuildThreeParameterCommand(string[] commandParameters, string textCommand, bool writeOriginalCommand = true)
		{
			bool successful = true;
			var function = commandParameters[0];
			var parameter1 = commandParameters[1];
			var parameter2 = commandParameters[2];
			var pcRegister = Registers[(int)RegisterEnums.pc];

			switch (function)
			{
				case "loadImmediate":
					{
						var register1 = GetRegister(parameter1);
						register1.Value = int.Parse(parameter2);
						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							CodeLine = $"loadImmediate ${register1.Number} $0 " + string.Format("0x{0}", register1.Value.ToString("X8")),
							OriginalCommand = writeOriginalCommand ? CurrentLine + ": " + textCommand : "",
							Result = int.Parse(parameter2),
							ResultingRegister = register1.EnumID
						});
					}
					break;

				default:
					{
						SnackBoxMessage.Enqueue($"Command ' {function} ' does not exist , Exiting Run");
						successful = false;
					}
					break;
			}

			pcRegister.IncrementPC();

			return successful;
		}

		private bool BuildTwoParameterCommand(string[] commandParameters, string textCommand)
		{
			bool successful = true;
			var function = commandParameters[0];
			var parameter1 = commandParameters[1];
			var pcRegister = Registers[(int)RegisterEnums.pc];

			switch (function)
			{
				case "branch":
					{
						var branch = Branches.Where(p => string.Equals(p.Name, parameter1)).FirstOrDefault();

						if (branch != null)
						{
							//var offset = AllOperations[branch.OperationGoToIndex].OpCode - pcRegister;

							var opCode = 67239935 + branch.OperationGoToIndex;

							AllOperations.Add(new Operation()
							{
								Address = pcRegister.Value,
								OpCode = opCode,
								CodeLine = $"branch 0x{opCode:X8}",
								OriginalCommand = textCommand,
								IsBranch = true,
								DoesBranchExistYet = true,
								BranchName = parameter1,
								OpCodeToGoTo = branch.OperationGoToIndex,
							});
						}
						else
						{
							AllOperations.Add(new Operation()
							{
								Address = pcRegister.Value,
								OriginalCommand = textCommand,
								IsBranch = true,
								BranchName = parameter1,
							});
							break;
						}
					}
					break;
			}

			return successful;
		}

		private Register GetRegister(string registerName)
		{
			var register = Registers.Where(p => string.Compare(p.Name, registerName) == 0).FirstOrDefault();

			if (register == null && registerName[0] == '$')
			{
				SnackBoxMessage.Enqueue($"Register Name ' {registerName} ' does not exist , Exiting Run");
			}
			return register;
		}

		#endregion methods
	}
}