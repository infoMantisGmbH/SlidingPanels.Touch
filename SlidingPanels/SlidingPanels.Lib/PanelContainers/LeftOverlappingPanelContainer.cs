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
	public class LeftOverlappingPanelContainer : OverlappingPanelContainer
    {
        #region Data Members
        /// <summary>
        /// Gets the panel position.
        /// </summary>
        /// <value>The panel position.</value>
		public override CGRect PanelPosition
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

		public override void SetFrames ()
		{
			var frame = PanelPosition;
			var bounds = View.Frame; // früher war hier "UIScreen.MainScreen.Bounds" zugewiesen, was bei iPads zu Problem führte, dass das Panel zu breit war
									 // Wenn schon rausgeschoben, dann den Wert nehmen. Sonst Standard
			var x = (_slidingEnded || (PanelVC.View.Frame.X != 0 && PanelVC.View.Frame.X != PanelPosition.X)) ? PanelVC.View.Frame.X : PanelPosition.X;
			PanelVC.View.Frame = new CGRect (x, 0, bounds.Width, bounds.Height);

			// View verschieben, da diese sonst unsichtbar über den anderen Views liegt
			frame.X = 0;
			View.Frame = frame;
		}
    }
}

