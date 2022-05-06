﻿using MaterialDesignThemes.Wpf;
using MvvmHelpers;
using System.Collections.Generic;
using System.Linq;

namespace ProcessorSimulator
{
	public class ProcessCommands
	{
		#region Properties

		private Dictionary<string, uint> FuncOpCode = new Dictionary<string, uint>(){
			{ "add", 32 },
			{ "addi", 536870912 },
			{ "subtract", 34 },
			{ "multiply", 24 },
			{ "divide", 26 },
			{ "loadImmediate", 536870912 },
			{ "or", 37 },
			{ "xor", 38 },
			{ "nor", 39 },
			{ "and", 36 },
			{ "shiftLeftLogical", 0 },
			{ "shiftRightLogical", 2 },
			{ "branchIf<", 42 },
			{ "branchIf>", 42 },
			{ "branchIf<=", 42 },
			{ "branchIf>=", 42 },
			{ "branchIf==", 4 },
		};

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
			bool successful;
			try
			{
				CurrentLine++;

				var breakOnSpaces = textCommand.Split(" ");

				if (breakOnSpaces.Length == 4)
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
					uint opCode = (uint)(4294967040 + branch.OperationGoToIndex);

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
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);

						if (register1 == null || register2 == null)
						{
							break;
						}

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, register3),
							CodeLine = $"add ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							CodeToRun = () => { register3.Value = register2.Value + register1.Value; },
							DestinationRegister = register3
						});
					}
					break;

				case "addi":
					{
						var register1 = GetRegister(parameter1);
						int valueToAdd = 0;

						if (register1 == null)
						{
							register1 = GetRegister(parameter2);
							valueToAdd = int.Parse(parameter1);
						}
						else
						{
							valueToAdd = int.Parse(parameter2);
						}

						if (register1 == null)
						{
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$at", parameter2 }, "", false);
							register1 = GetRegister("$at");
						}

						var register3 = GetRegister(parameter3);

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeIType(function, register1, register3, valueToAdd),
							CodeLine = $"addi ${register1.Number} {valueToAdd} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							CodeToRun = () => { register3.Value = register1.Value + valueToAdd; },
							DestinationRegister = register3
						}); ;
					}
					break;

				case "subtract":
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
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$k0", parameter2 }, "", false);
							register2 = GetRegister("$k0");
						}

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, register3),
							CodeLine = $"subtract ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							DestinationRegister = register3,
							CodeToRun = () => { register3.Value = register1.Value - register2.Value; },
						});
					}
					break;

				case "multiply":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);

						var loRegister = GetRegister("$lo");
						var hiRegister = GetRegister("$hi");

						if (register1 == null)
						{
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$at", parameter1 }, "", false);
							register1 = GetRegister("$at");
						}

						if (register2 == null)
						{
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$k0", parameter2 }, "", false);
							register2 = GetRegister("$k0");
						}

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, register3),
							CodeLine = $"multiply ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							DestinationRegister = register3,
							CodeToRun = () =>
							{
								long result = (long)register1.Value * (long)register2.Value;
								var lo = (uint)result;
								int hi = (int)(result >> 32);

								loRegister.Value = (int)lo;
								hiRegister.Value = hi;

								register3.Value = (int)lo;
							}
						});
					}
					break;

				case "divide":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);
						var loRegister = GetRegister("$lo");
						var hiRegister = GetRegister("$hi");

						if (register1 == null)
						{
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$at", parameter1 }, "", false);
							register1 = GetRegister("$at");
						}

						if (register2 == null)
						{
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$k0", parameter2 }, "", false);
							register2 = GetRegister("$k0");
						}

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, register3),
							CodeLine = $"divide ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							DestinationRegister = register3,
							CodeToRun = () =>
							{
								if (register2.Value != 0)
								{
									long result = (long)register1.Value / (long)register2.Value;
									var lo = (uint)result;
									int hi = (int)(result >> 32);

									loRegister.Value = (int)lo;
									hiRegister.Value = hi;

									register3.Value = (int)lo;
								}
								else
								{
									SnackBoxMessage.Enqueue($"Cannot Divide By Zero!");
								}
							}
						});
					}
					break;

				case "or":
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
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$k0", parameter2 }, "", false);
							register2 = GetRegister("$k0");
						}

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, register3),
							CodeLine = $"or ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							CodeToRun = () => { register3.Value = register2.Value | register1.Value; },
							DestinationRegister = register3
						});
					}
					break;

				case "nor":
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
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$k0", parameter2 }, "", false);
							register2 = GetRegister("$k0");
						}

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, register3),
							CodeLine = $"nor ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							CodeToRun = () => { register3.Value = ~(register2.Value | register1.Value); },
							DestinationRegister = register3
						});
					}
					break;

				case "xor":
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
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$k0", parameter2 }, "", false);
							register2 = GetRegister("$k0");
						}

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, register3),
							CodeLine = $"xor ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							CodeToRun = () => { register3.Value = register2.Value ^ register1.Value; },
							DestinationRegister = register3
						});
					}
					break;

				case "and":
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
							BuildThreeParameterCommand(new string[] { "loadImmediate", "$k0", parameter2 }, "", false);
							register2 = GetRegister("$k0");
						}

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, register3),
							CodeLine = $"and ${register1.Number} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							CodeToRun = () => { register3.Value = register2.Value & register1.Value; },
							DestinationRegister = register3
						});
					}
					break;

				case "branchIf<":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var at = GetRegister("$at");

						var operation = new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, at),
							CodeLine = $"branchIf< ${register1.Number} ${register2.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							IsBranch = true,
							DoesBranchExistYet = true,
							DestinationRegister = at
						};

						operation.CodeToRun = () =>
						{
							var branch = Branches.Where(p => string.Equals(p.Name, parameter3)).FirstOrDefault();
							at.Value = (register1.Value < register2.Value) ? 1 : 0;

							operation.OpCodeToGoTo = at.Value == 1 ? branch.OperationGoToIndex : ((operation.Address - 4194304) / 4) + 1;
						};

						AllOperations.Add(operation);
					}
					break;

				case "branchIf>":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var at = GetRegister("$at");

						var operation = new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, at),
							CodeLine = $"branchIf> ${register1.Number} ${register2.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							IsBranch = true,
							DoesBranchExistYet = true,
							DestinationRegister = at
						};

						operation.CodeToRun = () =>
						{
							var branch = Branches.Where(p => string.Equals(p.Name, parameter3)).FirstOrDefault();
							at.Value = (register1.Value > register2.Value) ? 1 : 0;

							operation.OpCodeToGoTo = at.Value == 1 ? branch.OperationGoToIndex : ((operation.Address - 4194304) / 4) + 1;
						};

						AllOperations.Add(operation);
					}
					break;

				case "branchIf<=":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var at = GetRegister("$at");

						var operation = new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, at),
							CodeLine = $"branchIf<= ${register1.Number} ${register2.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							IsBranch = true,
							DoesBranchExistYet = true,
							DestinationRegister = at
						};

						operation.CodeToRun = () =>
						{
							var branch = Branches.Where(p => string.Equals(p.Name, parameter3)).FirstOrDefault();
							at.Value = (register1.Value <= register2.Value) ? 1 : 0;

							operation.OpCodeToGoTo = at.Value == 1 ? branch.OperationGoToIndex : ((operation.Address - 4194304) / 4) + 1;
						};

						AllOperations.Add(operation);
					}
					break;

				case "branchIf>=":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var at = GetRegister("$at");

						var operation = new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, register1, register2, at),
							CodeLine = $"branchIf>= ${register1.Number} ${register2.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							IsBranch = true,
							DoesBranchExistYet = true,
							DestinationRegister = at
						};

						operation.CodeToRun = () =>
						{
							var branch = Branches.Where(p => string.Equals(p.Name, parameter3)).FirstOrDefault();
							at.Value = (register1.Value >= register2.Value) ? 1 : 0;

							operation.OpCodeToGoTo = at.Value == 1 ? branch.OperationGoToIndex : ((operation.Address - 4194304) / 4) + 1;
						};

						AllOperations.Add(operation);
					}
					break;

				case "branchIf==":
					{
						var register1 = GetRegister(parameter1);
						var register2 = GetRegister(parameter2);
						var at = GetRegister("$at");

						var operation = new Operation()
						{
							Address = pcRegister.Value,
							CodeLine = $"branchIf== ${register1.Number} ${register2.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							IsBranch = true,
							DoesBranchExistYet = true,
							DestinationRegister = at,
						};

						var branch = Branches.Where(p => string.Equals(p.Name, parameter3)).FirstOrDefault();
						if (branch != null)
						{
							operation.OpCode = GetOpCodeIType(function, register1, register2, AllOperations[at.Value == 1 ? branch.OperationGoToIndex : ((operation.Address - 4194304) / 4)].Address << 16 >> 16);
						}
						else
						{
							operation.OpCode = GetOpCodeIType(function, register1, register2, operation.Address << 16 >> 16);
						}

						operation.CodeToRun = () =>
						{
							var branch = Branches.Where(p => string.Equals(p.Name, parameter3)).FirstOrDefault();
							at.Value = (register1.Value == register2.Value) ? 1 : 0;

							operation.OpCodeToGoTo = at.Value == 1 ? branch.OperationGoToIndex : ((operation.Address - 4194304) / 4) + 1;
						};

						AllOperations.Add(operation);
					}
					break;

				case "shiftLeftLogical":
					{
						var shiftAmount = int.Parse(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);

						var loRegister = GetRegister("$lo");
						var hiRegister = GetRegister("$hi");

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, null, register2, register3, shiftAmount),
							CodeLine = $"shiftLeftLogical {shiftAmount} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							DestinationRegister = register3,
							CodeToRun = () =>
							{
								long result = register2.Value << shiftAmount;
								var lo = (uint)result;
								int hi = (int)(result >> 32);

								loRegister.Value = (int)lo;
								hiRegister.Value = hi;

								register3.Value = (int)lo;
							}
						});
					}
					break;

				case "shiftRightLogical":
					{
						var shiftAmount = int.Parse(parameter1);
						var register2 = GetRegister(parameter2);
						var register3 = GetRegister(parameter3);

						var loRegister = GetRegister("$lo");
						var hiRegister = GetRegister("$hi");

						AllOperations.Add(new Operation()
						{
							Address = pcRegister.Value,
							OpCode = GetOpCodeRType(function, null, register2, register3, shiftAmount),
							CodeLine = $"shiftRightLogical {shiftAmount} ${register2.Number} ${register3.Number}",
							OriginalCommand = CurrentLine + ": " + textCommand,
							DestinationRegister = register3,
							CodeToRun = () =>
							{
								long result = register2.Value >> shiftAmount;
								var lo = (uint)result;
								int hi = (int)(result >> 32);

								loRegister.Value = (int)lo;
								hiRegister.Value = hi;

								register3.Value = (int)lo;
							}
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
			--CurrentLine;

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
							OpCode = GetOpCodeIType(function, null, register1, int.Parse(parameter2)),
							CodeLine = $"addi ${register1.Number} $0 " + string.Format("0x{0}", register1.Value.ToString("X8")),
							OriginalCommand = writeOriginalCommand ? CurrentLine + ": " + textCommand : "",
							DestinationRegister = register1,
							CodeToRun = () => { register1.Value = int.Parse(parameter2); }
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
							uint opCode = (uint)(67239935 + branch.OperationGoToIndex);

							AllOperations.Add(new Operation()
							{
								Address = pcRegister.Value,
								OpCode = opCode,
								CodeLine = $"branch 0x{opCode:X8}",
								OriginalCommand = CurrentLine + ": " + textCommand,
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
								OriginalCommand = CurrentLine + ": " + textCommand,
								IsBranch = true,
								BranchName = parameter1,
							});
							break;
						}
					}
					break;
			}

			pcRegister.IncrementPC();

			return successful;
		}

		private uint GetOpCodeIType(string function, Register source, Register dest, int val)
		{
			// Opcode		rs (source)			rt (destination)	Integer
			if ((val >> 16) > 0) { SnackBoxMessage.Enqueue($"Integer value ' {val} ' is larger than 16 bits , Exiting Run"); }
			if (source == null) { source = GetRegister("$zero"); }
			uint opcode = FuncOpCode[function] + (uint)(source.Number << 21) + (uint)(dest.Number << 16) + (uint)(val);
			return opcode;
		}

		private uint GetOpCodeRType(string function, Register reg1 = null, Register reg2 = null, Register reg3 = null, int shiftAmount = 0)
		{
			uint opcode = 0;
			// Opcode		rs (source1)		rt (source2)		rd (destination)		shift(0)		function

			switch (function)
			{
				case "multiply":
				case "divide":
					opcode = FuncOpCode[function] + (uint)(reg1.Number << 21) + (uint)(reg2.Number << 16);
					break;

				case "shiftLeftLogical":
				case "shiftRightLogical":
					opcode = FuncOpCode[function] + (uint)(reg2.Number << 16) + (uint)(reg3.Number << 11) + (uint)(shiftAmount << 6);
					break;

				default:
					opcode = FuncOpCode[function] + (uint)(reg1.Number << 21) + (uint)(reg2.Number << 16) + (uint)(reg3.Number << 11);
					break;
			}

			return opcode;
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