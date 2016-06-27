using System;
using CoreGraphics;
using Foundation;
using SlidingPanels.Lib.PanelContainers;
using UIKit;

namespace SlidingPanels.Lib.PanelContainers
{
	public class RightOverlappingPanelContainer : OverlappingPanelContainer
	{
		#region Data Members

		/// <summary>
		/// Gets the panel position.
		/// </summary>
		/// <value>The panel position.</value>
		public override CGRect PanelPosition {
			get {

				return new CGRect {
					X = View.Bounds.Width - Size.Width,
					Y = _navigationBarSize,
					Width = Size.Width,
					Height = View.Bounds.Height - _navigationBarSize
				};

			}
		}

		public float PanelYOffset { get; set; }

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="SlidingPanels.Lib.PanelContainers.LeftPanelContainer"/> class.
		/// </summary>
		/// <param name="panel">Panel.</param>
		public RightOverlappingPanelContainer (UIViewController panel) : base (panel, PanelType.OverlappingRightPanel)
		{
			MaskBackgroundColor = UIColor.Clear;
		}

		#endregion

		public override void SetFrames ()
		{
			var frame = PanelPosition;
			var bounds = View.Frame; // früher war hier "UIScreen.MainScreen.Bounds" zugewiesen, was bei iPads zu Problem führte, dass das Panel zu breit war
									 // Wenn schon rausgeschoben, dann den Wert nehmen. Sonst Standard
			var x = (_slidingEnded || (PanelVC.View.Frame.X != View.Bounds.Width && View.Bounds.Width - Size.Width != PanelPosition.X)) ? PanelVC.View.Frame.X : PanelPosition.X;
			var height = (bounds.Height + PanelYOffset < bounds.Height) ? bounds.Height + (-1 * PanelYOffset) : bounds.Height - PanelYOffset;

			PanelVC.View.Frame = new CGRect (0, PanelYOffset, bounds.Width, height);

			// View verschieben, da diese sonst unsichtbar über den anderen Views liegt
			frame.X = x;
			View.Frame = frame;
		}
	}
}

