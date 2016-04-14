using System;
using System.Collections.ObjectModel;

using UIKit;
using Foundation;

namespace ReorderTableViewDemo
{
	public partial class DefaultImplViewController : UIViewController
	{
		ObservableCollection<string> _stringItems;

		public DefaultImplViewController (IntPtr handle) : base (handle)
		{
			_stringItems = new ObservableCollection<string> {
				"1e item",
				"2e item",
				"3e item",
				"4e item",
				"5e item",
				"6e item",
				"7e item",
				"8e item",
				"9e item",
				"10e item"
			};

			Title = "Default Reorder Implementation";
			NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB (25, 173, 234);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			tableView.Source = new DemoTableSource (_stringItems);

//1 Set Editing to true
			tableView.SetEditing (true, false); //necessary to be able to move table cells

//4 Make tableview cells selectable while editing
			tableView.AllowsSelectionDuringEditing = true;
		}

		class DemoTableSource : UITableViewSource
		{
			ObservableCollection<string> _stringItems;
			string _cellIdentifier = "TableCell";

			public DemoTableSource (ObservableCollection<string> stringItems)
			{
				_stringItems = stringItems;
			}

			public override nint NumberOfSections (UITableView tableView)
			{
				return 1;
			}

			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return _stringItems.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				string stringItem = _stringItems[indexPath.Row];

				UITableViewCell cell = tableView.DequeueReusableCell (_cellIdentifier);
				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Default, _cellIdentifier); 
				}
				cell.TextLabel.Text = stringItem;

				return cell;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow (indexPath, true);
			}

			#region movecells

//2 Hide editing icon
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return UITableViewCellEditingStyle.None;
			}
		
//3 Set 'CanMoveRow' to true and implement method 'MoveRow'
			public override bool CanMoveRow (UITableView tableView, NSIndexPath indexPath)
			{
				return true;
			}

			public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
			{
				_stringItems.Move (sourceIndexPath.Row, destinationIndexPath.Row);

				tableView.ReloadData ();
			}

			#endregion
		}
	}
}