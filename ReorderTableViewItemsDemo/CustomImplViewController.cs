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
		NSIndexPath _currentIndexPathSelectedCell = null;

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

			Title = "Custom Reorder Implementation";
			NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB (25, 173, 234);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			tableView.Source = new DemoTableSource (_stringItems);

//1 Create long press gesture and add this to the tableview
			UILongPressGestureRecognizer longPressGesture = new UILongPressGestureRecognizer { MinimumPressDuration = 0.4 };
			longPressGesture.AddTarget(() => HandleLongPress(longPressGesture));
			tableView.AddGestureRecognizer(longPressGesture);
		}

		void HandleLongPress(UILongPressGestureRecognizer recognizer)
		{
			CGPoint longPressLocation = recognizer.LocationInView (tableView);
			NSIndexPath currentIndexPathLongPress = tableView.IndexPathForRowAtPoint (longPressLocation);

//2 Handle begin state for long press gesture
			if (recognizer.State == UIGestureRecognizerState.Began) {

				if (currentIndexPathLongPress != null) {
					UITableViewCell selectedCell = tableView.CellAt (currentIndexPathLongPress);

					_currentIndexPathSelectedCell = currentIndexPathLongPress;

//3 Create snapshot for UITableViewCell
					_snapshotOfSelectedCell = GetSnapshotFromTableViewCell (selectedCell);
					CGPoint center = selectedCell.Center;

					_snapshotOfSelectedCell.Center = center;
					_snapshotOfSelectedCell.Alpha = 0.0f;

//4 Add the snapshot as subview to the tableview
					tableView.AddSubview (_snapshotOfSelectedCell);

//5 make snapshot animated visible -> centered at long pressed position
					UIView.Animate (0.25, () => {
						center.Y = longPressLocation.Y;
						_snapshotOfSelectedCell.Center = center;
						_snapshotOfSelectedCell.Transform = CGAffineTransform.MakeScale (1.05f, 1.05f); //make snapshot a little bit bigger than the original cell
						_snapshotOfSelectedCell.Alpha = 0.98f;
						selectedCell.Alpha = 0.0f;
						selectedCell.Hidden = true;
					});
				}
			}

//6 Handle changed state for long press gesture
			else if (recognizer.State == UIGestureRecognizerState.Changed) {

//7 Move snapshot to new longpressed position
				CGPoint center = _snapshotOfSelectedCell.Center;
				center.Y = longPressLocation.Y;
				_snapshotOfSelectedCell.Center = center;

//8 Check: is destination position valid and is it different from the previous index path?
				if (currentIndexPathLongPress != null && !currentIndexPathLongPress.Equals (_currentIndexPathSelectedCell)) {
				
//9 Update data source
					_stringItems.Move (_currentIndexPathSelectedCell.Row, currentIndexPathLongPress.Row);

//10 Move the cell in the tableview
					tableView.MoveRow (_currentIndexPathSelectedCell, currentIndexPathLongPress);

//11 Update previous position so it is in sync with UI changes
					_currentIndexPathSelectedCell = currentIndexPathLongPress;
				}
			}

//12 Handle ended state for long press gesture -> clean up
			else { //State is ended or failed
				UITableViewCell selectedCell = tableView.CellAt (_currentIndexPathSelectedCell);

//13 Make snapshat invisible and remove it from the superview - also make the selected cell visible again
				UIView.Animate (0.25f,
					() => { //animation
						_snapshotOfSelectedCell.Center = selectedCell.Center;
						_snapshotOfSelectedCell.Transform = CGAffineTransform.MakeIdentity (); //restores view to original size
						_snapshotOfSelectedCell.Alpha = 0.0f;
						selectedCell.Alpha = 1.0f;
					},
					() => { //completion
						selectedCell.SelectedBackgroundView = new UIView { BackgroundColor = UIColor.FromRGB (208, 208, 208) };
						selectedCell.Hidden = false;
						_currentIndexPathSelectedCell = null;
	
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
					cell.SelectedBackgroundView = new UIView { BackgroundColor = UIColor.FromRGB (208, 208, 208) };
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