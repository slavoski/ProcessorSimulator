using MvvmHelpers;

namespace ProcessorSimulator
{
	public class Register : BaseViewModel
	{
		#region member variable

		private int _value = 0;

		#endregion member variable

		#region Properties

		public string Name
		{
			get;
			set;
		}

		public string Number
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
	}
}