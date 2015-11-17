/// Copyright (C) 2013 Pat Laplante & Frank Caico
///
/// Permission is hereby granted, free of charge, to  any person obtaining a copy 
/// of this software and associated documentation files (the "Software"), to deal 
/// in the Software without  restriction, including without limitation the rights 
/// to use, copy,  modify,  merge, publish,  distribute,  sublicense, and/or sell 
/// copies of the  Software,  and  to  permit  persons  to   whom the Software is 
/// furnished to do so, subject to the following conditions:
///
///     The above  copyright notice  and this permission notice shall be included 
///     in all copies or substantial portions of the Software.
///
///     THE  SOFTWARE  IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
///     OR   IMPLIED,   INCLUDING  BUT   NOT  LIMITED   TO   THE   WARRANTIES  OF 
///     MERCHANTABILITY,  FITNESS  FOR  A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
///     IN NO EVENT SHALL  THE AUTHORS  OR COPYRIGHT  HOLDERS  BE  LIABLE FOR ANY 
///     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT 
///     OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION  WITH THE SOFTWARE OR 
///     THE USE OR OTHER DEALINGS IN THE SOFTWARE.
/// -----------------------------------------------------------------------------

using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace SlidingPanels.Lib.PanelContainers
{
    /// <summary>
    /// Container class for Sliding Panels located on the left edge of the device screen
    /// </summary>
    public class LeftOverlappingPanelContainer : PanelContainer
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

        private bool _slidingEnded;

		/// <summary>
		/// Observer der Änderung der Orientation des Geräts.
		/// </summary>
		private NSObject _orientationDidChangeObserver;

		/// <summary>
		/// Gibt an, ob die Berechnung der Bildschirmgröße für iOS7 angepasst werden muss.
		/// </summary>
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

        private nfloat _navigationBarSize { 
           get 
           {
                try 
                {
                    var windows = UIApplication.SharedApplication.Windows;
                    var found = false;
                    UINavigationController rootController = null;
                    for (int i = 0; !found && i < windows.Length; ++i)
                    {
                        var window = windows[i];
                        rootController = window.RootViewController as UINavigationController;
                        found = rootController != null;
                    }

					nfloat height;
					if (!_shooldAdoptForIOS7){
                    	height = (found) ? rootController.NavigationBar.Bounds.Height : 0.0f;
					} else{
						height = (found) ? rootController.NavigationBar.Bounds.Height : 0.0f;
					}

					return height + (!_shooldAdoptForIOS7 ? UIApplication.SharedApplication.StatusBarFrame.Size.Height : UIApplication.SharedApplication.StatusBarFrame.Size.Width);
                } 
                catch 
                {
                    // Per Hand abgemessen. Sollte eigentlich verhindert werden
                    return 62.5f;
                }
           } 
        }

        /// <summary>
        /// Gets the panel position.
        /// </summary>
        /// <value>The panel position.</value>
        public CGRect PanelPosition
        {
            get
            {
				if (!_shooldAdoptForIOS7)
				{
					return new CGRect
					{
						X = -Size.Width,
						Y = _navigationBarSize,
						Width = Size.Width,
						Height = View.Bounds.Height - _navigationBarSize
					};
				}
				else
				{
					return new CGRect
					{
						X = -Size.Height,
						Y = _navigationBarSize,
						Width = Size.Height,
						Height = View.Bounds.Width - _navigationBarSize
					};
				}
            }
        }

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingPanels.Lib.PanelContainers.LeftPanelContainer"/> class.
        /// </summary>
        /// <param name="panel">Panel.</param>
        public LeftOverlappingPanelContainer (UIViewController panel) : base(panel, PanelType.OverlappingLeftPanel)
        {

        }

        #endregion

        private void SetFrames() 
        {
            var frame = PanelPosition;
			var bounds = View.Frame; // früher war hier "UIScreen.MainScreen.Bounds" zugewiesen, was bei iPads zu Problem führte, dass das Panel zu breit war
            // Wenn schon rausgeschoben, dann den Wert nehmen. Sonst Standard
            var x = (_slidingEnded || (PanelVC.View.Frame.X != 0 && PanelVC.View.Frame.X != PanelPosition.X)) ? PanelVC.View.Frame.X : PanelPosition.X;
            PanelVC.View.Frame = new CGRect(x,0,bounds.Width, bounds.Height);

            // View verschieben, da diese sonst unsichtbar über den anderen Views liegt
            frame.X = 0;
            View.Frame = frame;
        }


        #region View Lifecycle

        /// <summary>
        /// Called after the Panel is loaded for the first time
        /// </summary>
        public override void ViewDidLoad ()
        {
            _slidingEnded = false;
            base.ViewDidLoad ();
            SetFrames();
        }

        /// <summary>
        /// Called whenever the Panel is about to become visible
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            SetFrames();
            _appeared = true;
            _slidingEnded = false;

			// ein Hack für das Registrieren der Orientation des Geräts aus http://stackoverflow.com/a/26283253 (siehe "ViewWillDisappear")
			// (denn "DidRotate" wird in dieser Klasse nicht aufgerufen, weil es sich nicht um den TopViewController handelt)
			_orientationDidChangeObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification,
				notification =>	PanelMask.RecalculateMaksSize()
			);
			UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
        }

		/// <summary>
		/// Called whenever the Panel is about to become invisible
		/// </summary>
		/// <param name="animated">If set to <c>true</c> animated.</param>
		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			// ein Hack für das Registrieren der Orientation des Geräts aus http://stackoverflow.com/a/26283253 (siehe "ViewWillAppear")
			// (denn "DidRotate" wird in dieser Klasse nicht aufgerufen, weil es sich nicht um den TopViewController handelt)
			UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
			if (_orientationDidChangeObserver != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_orientationDidChangeObserver);
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
        public override CGRect GetTopViewPositionWhenSliderIsVisible(CGRect topViewCurrentFrame)
        {
            return topViewCurrentFrame;
        }

        /// <summary>
        /// Returns a rectangle representing the location and size of the top view 
        /// when this Panel is hidden
        /// </summary>
        /// <returns>The top view position when slider is visible.</returns>
        /// <param name="topViewCurrentFrame">Top view current frame.</param>
        public override CGRect GetTopViewPositionWhenSliderIsHidden(CGRect topViewCurrentFrame)
        {
            topViewCurrentFrame.X = 0;
            return topViewCurrentFrame;
        }

        public bool IsPanelVisible() 
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
        public override bool CanStartSliding(CGPoint touchPosition, CGRect topViewCurrentFrame)
        {
            if (!IsVisible)
            {
                return (touchPosition.X >= 0.0f && touchPosition.X <= EdgeTolerance);
            }
            else
            {
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

            if (frame.X >= 0)
            { 
                frame.X = 0;
            }
            var percent = PanelMask.Percent;
            if (frame.X != 0)
            {
                float diff = (float) Math.Abs(PanelPosition.X / frame.X);
                if (Math.Abs(diff) > float.Epsilon)
                {
                    percent = PanelMask.Percent - PanelMask.Percent / diff;
                }
            }

            PanelMask.MaskView(percent);

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

        public override void Show()
        {
            View.Layer.ZPosition = 0;
            View.Hidden = false;
            _slidingEnded = false;
        }

        public override void Hide()
        {
            base.Hide();
            _appeared = false;
            _slidingEnded = false;
        }
        #endregion
    }
}

