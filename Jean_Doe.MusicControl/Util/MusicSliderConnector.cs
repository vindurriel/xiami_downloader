using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Jean_Doe.MusicControl
{
    public class MusicSliderConnector
    {
        public Slider _slider;
        public MusicSliderConnector(Slider slider)
        {
            _slider = slider;
            slider.Visibility = Visibility.Hidden;
            Mp3Player.TimeChanged += Mp3Player_TimeChanged;
            Mp3Player.SongChanged += Mp3Player_SongChanged;
            slider.PreviewMouseDown += slider_MouseLeftButtonDown;
            slider.PreviewMouseUp += slider_MouseLeftButtonUp;

            anim = new Storyboard();
            da = new DoubleAnimation();
            da.Duration = new Duration(TimeSpan.FromMilliseconds(Mp3Player.RefreshInterval / 5));
            da.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTarget(da, _slider);
            Storyboard.SetTargetProperty(da, new PropertyPath("Value"));
            anim.Children.Add(da);
        }


        void slider_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _slider.ValueChanged -= slider_ValueChanged;
            Mp3Player.TimeChanged -= Mp3Player_TimeChanged;
            Mp3Player.TimeChanged += Mp3Player_TimeChanged;
        }

        void slider_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            anim.Pause();
            _slider.ValueChanged -= slider_ValueChanged;
            _slider.ValueChanged += slider_ValueChanged;
            Mp3Player.TimeChanged -= Mp3Player_TimeChanged;
        }

        void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (canReposition)
                Mp3Player.CurrentTime = e.NewValue;
        }

        void Mp3Player_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            da.To = e.Current;
            anim.Begin();
        }
        Storyboard anim;
        DoubleAnimation da;
        bool canReposition = false;
        void Mp3Player_SongChanged(object sender, SongChangedEventArgs e)
        {
            _slider.Visibility = Visibility.Visible;
            canReposition = SongViewModel.GetId(e.Id).Song.DownloadState == "complete";
            _slider.Cursor = canReposition ? System.Windows.Input.Cursors.Hand : System.Windows.Input.Cursors.No;
            if (_slider.Maximum != e.Total)
            {
                da.To = 0;
                anim.Begin();
                _slider.Maximum = e.Total;
            }
        }
    }
}
