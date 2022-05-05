using System.Windows;
using System.Windows.Data;

namespace ProcessorSimulator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private CollectionView _collectionView;

		public MainWindow()
		{
			InitializeComponent();
			_collectionView = (CollectionView)CollectionViewSource.GetDefaultView(_CommandsDataGrid.ItemsSource);
			_collectionView.Filter = SearchBoxFilter;
		}

		private void _SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			_collectionView.Refresh();
		}

		private bool SearchBoxFilter(object item)
		{
			bool returnValue;

			if (string.IsNullOrEmpty(_SearchBox.Text))
			{
				returnValue = true;
			}
			else
			{
				var commandDescription = item as CommandDescription;

				returnValue = commandDescription.Name.IndexOf(_SearchBox.Text, System.StringComparison.OrdinalIgnoreCase) >= 0;
			}

			return returnValue;
		}
	}
}