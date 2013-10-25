using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Jean_Doe.MusicControl
{
    public static class ItemsControlExtensions
    {
        public static void ScrollToCenterOfView(this ItemsControl itemsControl, object item)
        {
            // Scroll immediately if possible
            if (!itemsControl.TryScrollToCenterOfView(item))
            {
                // Otherwise wait until everything is loaded, then scroll
                if (itemsControl is ListBox) ((ListBox)itemsControl).ScrollIntoView(item);
                itemsControl.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    itemsControl.TryScrollToCenterOfView(item);
                }));
            }
        }
        static T getChild<T>(DependencyObject p) where T : DependencyObject
        {
            if (p == null)
                return null;
            if (p is T)
                return p as T;
            DependencyObject c = null;
            var childrenCount = VisualTreeHelper.GetChildrenCount(p);
            if (childrenCount > 0)
                for (int i = 0; i < childrenCount; i++)
                {
                    c = VisualTreeHelper.GetChild(p, i);
                    var r = getChild<T>(c);
                    if (r != null)
                        return r;
                }
            return null;
        }
        private static bool TryScrollToCenterOfView(this ItemsControl itemsControl, object item)
        {
            // Find the container
            var index = itemsControl.Items.IndexOf(item);
            if (index == -1) return false;
            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as UIElement;
            if (container == null)
            {
            }

            // Find the ScrollContentPresenter
            ScrollContentPresenter presenter = getChild<ScrollContentPresenter>(itemsControl);
            if (presenter == null) return false;

            // Find the IScrollInfo
            var scrollInfo =
                !presenter.CanContentScroll ? presenter :
                presenter.Content as IScrollInfo ??
                FirstVisualChild(presenter.Content as ItemsPresenter) as IScrollInfo ??
                presenter;

            // Compute the center point of the container relative to the scrollInfo
            //Size size = container.RenderSize;
            //Point center = container.TransformToAncestor((Visual)scrollInfo).Transform(new Point(size.Width / 2, size.Height / 2));
            //center.Y += scrollInfo.VerticalOffset;
            //center.X += scrollInfo.HorizontalOffset;
            Point center = new Point();
            // Adjust for logical scrolling
            if (scrollInfo is StackPanel || scrollInfo is VirtualizingPanel)
            {
                double logicalCenter = index + 0.5;

                if (scrollInfo is VirtualizingTilePanel)
                {
                    var vsp = scrollInfo as VirtualizingTilePanel;
                    center.Y = vsp.ChildHeight * index / vsp.ChildrenPerRow;
                }
                else
                {
                    dynamic x = scrollInfo;
                    Orientation orientation = x.Orientation;
                    if (orientation == Orientation.Horizontal)
                        center.X = logicalCenter;
                    else
                        center.Y = logicalCenter;
                }
            }

            // Scroll the center of the container to the center of the viewport
            if (scrollInfo.CanVerticallyScroll) scrollInfo.SetVerticalOffset(CenteringOffset(center.Y, scrollInfo.ViewportHeight, scrollInfo.ExtentHeight));
            if (scrollInfo.CanHorizontallyScroll) scrollInfo.SetHorizontalOffset(CenteringOffset(center.X, scrollInfo.ViewportWidth, scrollInfo.ExtentWidth));
            return true;
        }

        private static double CenteringOffset(double center, double viewport, double extent)
        {
            return Math.Min(extent - viewport, Math.Max(0, center - viewport / 2));
        }
        private static DependencyObject FirstVisualChild(Visual visual)
        {
            if (visual == null) return null;
            if (VisualTreeHelper.GetChildrenCount(visual) == 0) return null;
            return VisualTreeHelper.GetChild(visual, 0);
        }
    }
}
