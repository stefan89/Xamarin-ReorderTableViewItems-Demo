using System;
using System.Collections.ObjectModel;

using UIKit;
using Foundation;
using CoreGraphics;

namespace ReorderTableViewDemo
{
	public partial class CustomImplViewController : UIViewController
	{
		ObservableCollection<string> _stringItems;
		UIView _snapshotOfSelectedCell = null;
		NSIndexPath _indexPathStartPosition = null;

		public CustomImplViewController (IntPtr handle) : base (handle)
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
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Title = "Custom Reorder Implementation";
			NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB (25, 173, 234);

			tableView.Source = new DemoTableSource (_stringItems);

			//Create a new long press gesture
			UILongPressGestureRecognizer longPressGesture = new UILongPressGestureRecognizer { MinimumPressDuration = 0.4 };
			longPressGesture.AddTarget(() => HandleLongPress(longPressGesture));
			tableView.AddGestureRecognizer(longPressGesture);
		}

		void HandleLongPress(UILongPressGestureRecognizer recognizer)
		{
			CGPoint longPressLocation = recognizer.LocationInView (tableView);
			NSIndexPath indexPathCurrent = tableView.IndexPathForRowAtPoint (longPressLocation);

			if (recognizer.State == UIGestureRecognizerState.Began) {

				if (indexPathCurrent != null) {
					UITableViewCell selectedCell = tableView.CellAt (indexPathCurrent);

					_indexPathStartPosition = indexPathCurrent;

					_snapshotOfSelectedCell = GetSnapshotFromTableViewCell (selectedCell);
					CGPoint center = selectedCell.Center;

					_snapshotOfSelectedCell.Center = center;
					_snapshotOfSelectedCell.Alpha = 0.0f;
					tableView.AddSubview (_snapshotOfSelectedCell);

					//Add the snapshot as subview, centered at cell's center
					UIView.Animate (0.25, () => {
						center.Y = longPressLocation.Y;
						_snapshotOfSelectedCell.Center = center;
						_snapshotOfSelectedCell.Transform = CGAffineTransform.MakeScale (1.05f, 1.05f);
						_snapshotOfSelectedCell.Alpha = 0.98f;
						selectedCell.Alpha = 0.0f;
						selectedCell.Hidden = true;
					});
				}
			}
			else if (recognizer.State == UIGestureRecognizerState.Changed) {
				CGPoint center = _snapshotOfSelectedCell.Center;
				center.Y = longPressLocation.Y;
				_snapshotOfSelectedCell.Center = center;

				//Is destination position valid and is it different from the start position?
				if (indexPathCurrent != null && !indexPathCurrent.Equals (_indexPathStartPosition)) {

					//Update data source
					_stringItems.Move (_indexPathStartPosition.Row, indexPathCurrent.Row);

					//Move the rows
					tableView.MoveRow (_indexPathStartPosition, indexPathCurrent);

					//Update start position so it is in sync with UI changes
					_indexPathStartPosition = indexPathCurrent;
				}
			}
			else { //ended or failed
				//Clean up
				UITableViewCell cell = tableView.CellAt (_indexPathStartPosition);
				cell.Alpha = 0.0f;

				UIView.Animate (0.25f, 
					() => { //animation
						_snapshotOfSelectedCell.Center = cell.Center;
						_snapshotOfSelectedCell.Transform = CGAffineTransform.MakeIdentity ();
						_snapshotOfSelectedCell.Alpha = 0.0f;
						cell.Alpha = 1.0f;
					}, 
					() => { //completion
						cell.Hidden = false;
						_indexPathStartPosition = null;
						_snapshotOfSelectedCell.RemoveFromSuperview ();
						_snapshotOfSelectedCell = null;
					}
				);
			}
		}

		UIView GetSnapshotFromTableViewCell(UITableViewCell cellToSnapShot)
		{
			//Change background color for the long pressed table view cell
			cellToSnapShot.SelectedBackgroundView = new UIView { BackgroundColor = UIColor.White };

			UIGraphics.BeginImageContextWithOptions (cellToSnapShot.Bounds.Size, false, 0);
			cellToSnapShot.Layer.RenderInContext (UIGraphics.GetCurrentContext ());
			UIImage image = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();

			UIView snapshotView = new UIImageView(image);
			snapshotView.Layer.MasksToBounds = false;
			snapshotView.Layer.CornerRadius = 0;
			snapshotView.Layer.ShadowOffset = new CGSize(-5.0, 0.0);
			snapshotView.Layer.ShadowRadius = 5.0f;
			snapshotView.Layer.ShadowOpacity = 0.4f;

			return snapshotView;
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
				if (cell == null){ 
					cell = new UITableViewCell (UITableViewCellStyle.Default, _cellIdentifier); 
				}
				cell.TextLabel.Text = stringItem;

				return cell;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow (indexPath, true);
			}
		}
	}
}