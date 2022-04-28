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

		public string Code
		{
			get;
			set;
		}

		public string CodeLine
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