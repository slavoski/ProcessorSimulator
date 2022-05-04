namespace ProcessorSimulator
{
	public class Operation
	{
		#region properties

		public int Address
		{
			get;
			set;
		}

		public string BranchName
		{
			get;
			set;
		}

		public string CodeLine
		{
			get;
			set;
		}

		public bool DoesBranchExistYet
		{
			get;
			set;
		}

		public bool IsBranch
		{
			get;
			set;
		}

		public uint OpCode
		{
			get;
			set;
		}

		public int OpCodeToGoTo
		{
			get;
			set;
		}

		public string OriginalCommand
		{
			get;
			set;
		}

		public int Result
		{
			get;
			set;
		}

		public RegisterEnums ResultingRegister
		{
			get;
			set;
		}

		#endregion properties
	}
}