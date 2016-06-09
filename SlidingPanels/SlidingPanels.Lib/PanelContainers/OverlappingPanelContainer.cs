using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace SlidingPanels.Lib.PanelContainers
{
	public abstract class OverlappingPanelContainer : PanelContainer
	{
		#region Data Members

		/// <summary>
		/// starting X Coordinate of the top view
		/// </summary>
		private nfloat _topViewStartXPosition = 0.0f;

		/// <summary>
		/// X coordinate where the user touched when starting a slide operation
		/// </summary>
		private nfloat _touchPositionStartXPosition = 0.0f;

		/// <summary>
		/// Gibt an, ob das Menue aufgeklappt ist.
		/// </summary>
		private bool _appeared;

		protected bool _slidingEnded;

		/// <summary>
		/// Observer der Änderung der Orientation des Geräts.
		/// </summary>
		private NSObject _orientationDidChangeObserver;

		/// <summary>
		/// Gibt an, ob die Berechnung der Bildschirmgröße für iOS7 angepasst werden muss.
		/// </summary>
		protected static bool _shooldAdoptForIOS7 {
			get {
				// if the iOS version is greater or equal 8.0 an in portrait:
				return !(UIDevice.CurrentDevice.CheckSystemVersion (8, 0)
				|| (UIApplication.SharedApplication.StatusBarOrientation != UIInterfaceOrientation.LandscapeLeft
				&& UIApplication.SharedApplication.StatusBarOrientation != UIInterfaceOrientation.LandscapeRight));
			}
		}

		protected nfloat _navigationBarSize {
			get {
				try {
					var windows = UIApplication.SharedApplication.Windows;
					var found = false;
					UINavigationController rootController = null;
					for (int i = 0; !found && i < windows.Length; ++i) {
						var window = windows [i];
						rootController = window.RootViewController as UINavigationController;
						found = rootController != null;
					}

					nfloat height;
					if (!_shooldAdoptForIOS7) {
						height = (found) ? rootController.NavigationBar.Bounds.Height : 0.0f;
					} else {
						height = (found) ? rootController.NavigationBar.Bounds.Height : 0.0f;
					}

					return height + (!_shooldAdoptForIOS7 ? UIApplication.SharedApplication.StatusBarFrame.Size.Height : UIApplication.SharedApplication.StatusBarFrame.Size.Width);
				} catch {
					// Per Hand abgemessen. Sollte eigentlich verhindert werden
					return 62.5f;
				}
			}
		}

		/// <summary>
		/// Gets the panel position.
		/// </summary>
		/// <value>The panel position.</value>
		public abstract CGRect PanelPosition { get; }

		#endregion

		#region Konstruktor
		protected OverlappingPanelContainer(UIViewController panel, PanelType panelType) : base(panel, panelType){
			
		}
		#endregion

		public abstract void SetFrames ();

		#region View Lifecycle

		/// <summary>
		/// Called after the Panel is loaded for the first time
		/// </summary>
		public override void ViewDidLoad ()
		{
			_slidingEnded = false;
			base.ViewDidLoad ();
			SetFrames ();
		}

		/// <summary>
		/// Called whenever the Panel is about to become visible
		/// </summary>
		/// <param name="animated">If set to <c>true</c> animated.</param>
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			SetFrames ();
			_appeared = true;
			_slidingEnded = false;

			// ein Hack für das Registrieren der Orientation des Geräts aus http://stackoverflow.com/a/26283253 (siehe "ViewWillDisappear")
			// (denn "DidRotate" wird in dieser Klasse nicht aufgerufen, weil es sich nicht um den TopViewController handelt)
			_orientationDidChangeObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIDevice.OrientationDidChangeNotification,
				notification => PanelMask.RecalculateMaksSize ()
			);
			UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications ();
		}

		/// <summary>
		/// Called whenever the Panel is about to become invisible
		/// </summary>
		/// <param name="animated">If set to <c>true</c> animated.</param>
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			// ein Hack für das Registrieren der Orientation des Geräts aus http://stackoverflow.com/a/26283253 (siehe "ViewWillAppear")
			// (denn "DidRotate" wird in dieser Klasse nicht aufgerufen, weil es sich nicht um den TopViewController handelt)
			UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications ();
			if (_orientationDidChangeObserver != null) {
				NSNotificationCenter.DefaultCenter.RemoveObserver (_orientationDidChangeObserver);
			}
		}

		#endregion

		#region Position Methods

		/// <summary>
		/// Returns a rectangle representing the location and size of the top view 
		/// when this Panel is showing
		/// </summary>
		/// <returns>The top view position when slider is visible.</returns>
		/// <param name="topViewCurrentFrame">Top view current frame.</param>
		public override CGRect GetTopViewPositionWhenSliderIsVisible (CGRect topViewCurrentFrame)
		{
			return topViewCurrentFrame;
		}

		/// <summary>
		/// Returns a rectangle representing the location and size of the top view 
		/// when this Panel is hidden
		/// </summary>
		/// <returns>The top view position when slider is visible.</returns>
		/// <param name="topViewCurrentFrame">Top view current frame.</param>
		public override CGRect GetTopViewPositionWhenSliderIsHidden (CGRect topViewCurrentFrame)
		{
			topViewCurrentFrame.X = 0;
			return topViewCurrentFrame;
		}

		public bool IsPanelVisible ()
		{
			var size = PanelPosition;

			return PanelVC.View.Frame.X > (-1 * size.Width);
		}

		#endregion

		#region Sliding Methods

		/// <summary>
		/// Determines whether this instance can start sliding given the touch position and the 
		/// current location/size of the top view. 
		/// Note that touchPosition is in Screen coordinate.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="touchPosition">Touch position.</param>
		/// <param name="topViewCurrentFrame">Top view's current frame.</param>
		public override bool CanStartSliding (CGPoint touchPosition, CGRect topViewCurrentFrame)
		{
			if (!IsVisible) {
				return (touchPosition.X >= 0.0f && touchPosition.X <= EdgeTolerance);
			} else {
				return topViewCurrentFrame.Contains (touchPosition);
			}
		}

		/// <summary>
		/// Called when sliding has started on this Panel
		/// </summary>
		/// <param name="touchPosition">Touch position.</param>
		/// <param name="topViewCurrentFrame">Top view current frame.</param>
		public override void SlidingStarted (CGPoint touchPosition, CGRect topViewCurrentFrame)
		{
			_touchPositionStartXPosition = touchPosition.X;
			_topViewStartXPosition = topViewCurrentFrame.X;
		}

		/// <summary>
		/// Called while the user is sliding this Panel
		/// </summary>
		/// <param name="touchPosition">Touch position.</param>
		/// <param name="topViewCurrentFrame">Top view current frame.</param>
		public override CGRect Sliding (CGPoint touchPosition, CGRect topViewCurrentFrame)
		{
			nfloat translation = touchPosition.X - _touchPositionStartXPosition;

			CGRect frame = topViewCurrentFrame;

			frame.X = _topViewStartXPosition + translation;

			if (frame.X >= 0) {
				frame.X = 0;
			}
			var percent = PanelMask.Percent;
			if (frame.X != 0) {
				float diff = (float)Math.Abs (PanelPosition.X / frame.X);
				if (Math.Abs (diff) > float.Epsilon) {
					percent = PanelMask.Percent - PanelMask.Percent / diff;
				}
			}

			PanelMask.MaskView (percent);

			return frame;
		}

		/// <summary>
		/// Determines if a slide is complete
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="touchPosition">Touch position.</param>
		/// <param name="topViewCurrentFrame">Top view current frame.</param>
		public override bool SlidingEnded (CGPoint touchPosition, CGRect topViewCurrentFrame)
		{
			var rightPos = topViewCurrentFrame.X + Size.Width;
			// Das Menue ist offen und es wurde zwischen Menue und rechten Rand getappt --> Menue schließen
			_slidingEnded = (topViewCurrentFrame.X == 0 && IsVisible && _appeared) ? touchPosition.X <= rightPos : (rightPos >= (Size.Width / 2));
			return _slidingEnded;
		}

		public override void Show ()
		{
			View.Layer.ZPosition = 0;
			View.Hidden = false;
			_slidingEnded = false;
		}

		public override void Hide ()
		{
			base.Hide ();
			_appeared = false;
			_slidingEnded = false;
		}
		#endregion
	}
}

