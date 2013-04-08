using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MusicPlayer
{
    /// <summary>
    /// MyBalloonTip.xaml 的交互逻辑
    /// </summary>
    public partial class MyBalloonTip
    {
        private object viewModel;
        public object ViewModel
        {
            get { return viewModel; }
            set { viewModel = value; grid.DataContext = value; }
        }
        public MyBalloonTip()
        {
            InitializeComponent();
            grid.MouseEnter += grid_MouseEnter;
            grid.MouseLeave += grid_MouseLeave_1;
            grid.MouseMove += OnGridMouseMove;
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        void OnGridMouseMove(object sender, MouseEventArgs e)
        {
        }
        private void grid_MouseEnter(object sender, MouseEventArgs e)
        {
            //if we're already running the fade-out animation, do not interrupt anymore
            //(makes things too complicated for the sample)

            //the tray icon assigned this attached property to simplify access
            btn_close.Visibility = Visibility.Visible;
            TaskBarIcon.ResetBalloonCloseTimer();
        }
        private void grid_MouseLeave_1(object sender, MouseEventArgs e)
        {
            btn_close.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent"/>
        /// and setting the "Handled" property to true, we suppress the popup
        /// from being closed in order to display the fade-out animation.
        /// </summary>
        private void OnBalloonClosing(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Resolves the <see cref="TaskbarIcon"/> that displayed
        /// the balloon and requests a close action.
        /// </summary>
        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //the tray icon assigned this attached property to simplify access
            TaskBarIcon.CloseBalloon();
        }
        TaskbarIcon _parent;
        public TaskbarIcon TaskBarIcon
        {
            get
            {
                if (_parent == null) _parent = TaskbarIcon.GetParentTaskbarIcon(this);

                return _parent;
            }
        }
        /// <summary>
        /// If the users hovers over the balloon, we don't close it.
        /// </summary>


        /// <summary>
        /// Closes the popup once the fade-out animation completed.
        /// The animation was triggered in XAML through the attached
        /// BalloonClosing event.
        /// </summary>
        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            Popup pp = (Popup)Parent;
            pp.IsOpen = false;
        }

   
    }
}
