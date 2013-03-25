using Jean_Doe.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MusicPlayer
{
    public class MusicSliderConnector
    {
        public Slider _slider;
        public MusicSliderConnector(Slider slider)
        {
            _slider = slider;
            Mp3Player.TimeChanged += Mp3Player_TimeChanged;
            Mp3Player.SongChanged += Mp3Player_SongChanged;
            slider.PreviewMouseDown += slider_MouseLeftButtonDown;
            slider.PreviewMouseUp += slider_MouseLeftButtonUp;
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

            Mp3Player.TimeChanged -= Mp3Player_TimeChanged;
        }

        void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Mp3Player.CurrentTime = TimeSpan.FromSeconds(e.NewValue);
        }

        void Mp3Player_TimeChanged(object sender, Mp3Player.TimeChangedEventArgs e)
        {
            _slider.Value = e.Current.TotalSeconds;
        }
        void Mp3Player_SongChanged(object sender, Mp3Player.SongChangedEventArgs e)
        {
            _slider.Maximum = e.Total.TotalSeconds;
        }
    }
}
