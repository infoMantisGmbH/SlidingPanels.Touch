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

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="SlidingPanels.Lib.PanelContainers.LeftPanelContainer"/> class.
		/// </summary>
		/// <param name="panel">Panel.</param>
		public RightOverlappingPanelContainer (UIViewController panel) : base (panel, PanelType.OverlappingRightPanel)
		{

		}

		#endregion

		public override void SetFrames ()
		{
			var frame = PanelPosition;
			var bounds = View.Frame; // früher war hier "UIScreen.MainScreen.Bounds" zugewiesen, was bei iPads zu Problem führte, dass das Panel zu breit war
									 // Wenn schon rausgeschoben, dann den Wert nehmen. Sonst Standard
			var x = (_slidingEnded || (PanelVC.View.Frame.X != View.Bounds.Width && View.Bounds.Width - Size.Width != PanelPosition.X)) ? PanelVC.View.Frame.X : PanelPosition.X;
			PanelVC.View.Frame = new CGRect (x, 0, bounds.Width, bounds.Height);

			// View verschieben, da diese sonst unsichtbar über den anderen Views liegt
			frame.X = x;
			View.Frame = frame;
		}
	}
}

