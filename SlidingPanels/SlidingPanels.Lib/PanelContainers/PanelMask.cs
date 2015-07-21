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
        private static void CheckAndCreateMask()
        {
            if (View == null || _mask != null)
                return;

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
            _mask = new UIView(new CGRect(0, frame.Y + navBarHeight, frame.Width, frame.Height));
            _mask.BackgroundColor = UIColor.FromRGB(0, 0, 0);
            _mask.Layer.Opacity = 0.0f;
            _mask.Layer.ZPosition = -10;

            View.AddSubview(_mask);

        }

        /// <summary>
        /// Setzt die Opacity der Maske anhand des übergebenen Prozentwertes.
        /// </summary>
        /// <param name="percent">Percent.</param>
        public static void MaskView(float percent)
        {
            if (View == null)
                return;
            CheckAndCreateMask();
            BringMaskToFront();
            _mask.Layer.Opacity = percent / 100f;
        }

        /// <summary>
        /// Brings the mask to front.
        /// </summary>
        public static void BringMaskToFront()
        {
            if (View == null)
                return;
            
            CheckAndCreateMask();
            _mask.Layer.ZPosition = 100;
            View.BringSubviewToFront(_mask);
        }
    }
}

