using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MusicPlayer
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
            da.Duration = new Duration(TimeSpan.FromMilliseconds(Mp3Player.RefreshInterval/5));
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
            _slider.ValueChanged -= slider_ValueChanged;
            _slider.ValueChanged += slider_ValueChanged;
            anim.Pause();
            Mp3Player.TimeChanged -= Mp3Player_TimeChanged;
        }

        void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Mp3Player.CurrentTime = TimeSpan.FromSeconds(e.NewValue);
        }

        void Mp3Player_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            da.To = e.Current.TotalSeconds;
            anim.Begin();
        }
        Storyboard anim;
        DoubleAnimation da;
        void Mp3Player_SongChanged(object sender, SongChangedEventArgs e)
        {
            _slider.Visibility = Visibility.Visible;
            da.To = 0;
            anim.Begin();
            _slider.Maximum = e.Total.TotalSeconds;
        }
    }
}
