using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessorSimulator
{
	public class CommandDescription : BaseViewModel
	{
		public string Name
		{
			get;
			set;
		}

		public string Parameter1
		{
			get;
			set;
		}
		public string Parameter2
		{
			get;
			set;
		}
		public string Parameter3
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public string Example
		{
			get;
			set;
		}
	}
}
