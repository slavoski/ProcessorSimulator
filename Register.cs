using MvvmHelpers;
using System.Windows;

namespace ProcessorSimulator
{
	public class Register : BaseViewModel
	{
		#region member variable

		private int _pc = 4194304;
		private int _value = 0;

		#endregion member variable

		#region Properties

		public RegisterEnums EnumID
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public int Number
		{
			get;
			set;
		}

		public Visibility ShowDecimalValue
		{
			get;
			set;
		}

		public int Value
		{
			get => _value;
			set
			{
				_value = value;
				OnPropertyChanged(nameof(Value));
			}
		}

		#endregion Properties

		#region methods

		public void ClearValues()
		{
			if (Name != "$pc")
			{
				Value = 0;
			}
			else
			{
				Value = _pc;
			}
		}

		public void IncrementPC()
		{
			Value += 4;
		}

		public void ResetPC()
		{
			Value = _pc;
		}

		#endregion methods
	}
}