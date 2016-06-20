using System;
using UIKit;
using CoreGraphics;

namespace SlidingPanels.Lib.PanelContainers
{
    public static class PanelMask
    {
        private static float _percent = -1f;
        /// <summary>
        /// The mask.
        /// </summary>
        private static UIView _mask;

		private static bool _shooldAdoptForIOS7
		{
			get
			{
				// if the iOS version is greater or equal 8.0 an in portrait:
				return !(UIDevice.CurrentDevice.CheckSystemVersion(8, 0)
					|| (UIApplication.SharedApplication.StatusBarOrientation != UIInterfaceOrientation.LandscapeLeft
						&& UIApplication.SharedApplication.StatusBarOrientation != UIInterfaceOrientation.LandscapeRight));
			}
		}

        public static float Percent 
        { 
            get { return _percent > 0 ? _percent : 70f; }
            set { _percent = value; } 
        }

        /// <summary>
        /// Die zu maskierende View. Wenn null, dann passiert nichts.
        /// </summary>
        /// <value>The view.</value>
        public static UIView View { get; set; }

        /// <summary>
        /// Erstellt die Maske, wenn diese noch nicht erstellt wurde.
        /// </summary>
		private static void CheckAndCreateMask(UIColor backgroundColor = null)
        {
            if (View == null || _mask != null)
                return;
           
			_mask = new UIView();

			RecalculateMaksSize();

			_mask.BackgroundColor = backgroundColor ?? UIColor.Black;
            _mask.Layer.Opacity = 0.0f;
            _mask.Layer.ZPosition = -10;

			if(backgroundColor != null){
				View.AddSubview(_mask);
			}
        }

		public static void RecalculateMaksSize()
		{
			var windows = UIApplication.SharedApplication.Windows;
			var found = false;
			UINavigationController viewController = null;
			for (int i = 0; !found && i < windows.Length; ++i)
			{
				var window = windows[i];
				viewController = window.RootViewController as UINavigationController;
				found = viewController != null;
			}
			var navBarHeight = (found) ? viewController.NavigationBar.Bounds.Height : 0f;

			var frame = UIScreen.MainScreen.ApplicationFrame;
			if (!_shooldAdoptForIOS7)
			{
				_mask.Frame = new CGRect(0, frame.Y + navBarHeight, frame.Width, frame.Height);
			}
			else
			{
				_mask.Frame = new CGRect(0, frame.X + navBarHeight, frame.Height, frame.Width);
			}

			View.SizeToFit();
		}

        /// <summary>
        /// Setzt die Opacity der Maske anhand des übergebenen Prozentwertes.
        /// </summary>
        /// <param name="percent">Percent.</param>
		public static void MaskView(float percent, UIColor backgroundcolor = null)
        {
            if (View == null)
                return;
			CheckAndCreateMask(backgroundcolor);
			BringMaskToFront(backgroundcolor);
            _mask.Layer.Opacity = percent / 100f;
        }

        /// <summary>
        /// Brings the mask to front.
        /// </summary>
		public static void BringMaskToFront(UIColor backgroundcolor = null)
        {
            if (View == null)
                return;
            
			CheckAndCreateMask (backgroundcolor);
            _mask.Layer.ZPosition = 100;
            View.BringSubviewToFront(_mask);
        }
    }
}

