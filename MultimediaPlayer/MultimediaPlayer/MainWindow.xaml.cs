using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MultimediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BindingList<FileInfo> _playList = new BindingList<FileInfo>(); //List lưu danh sách bài hát
        MediaPlayer _player = new MediaPlayer();
        DispatcherTimer _timer;
        int _lastIndex = -1; //Vị trí của bài hát đang chạy
        bool _isPlaying = false;
        string _filename; //Tên bài hát đang chạy

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += timer_Tick;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (_player.Source != null && _lastIndex != -1)
            {
                var converter = new NameConverter();
                var shortname = converter.Convert(_filename, null, null, null);

                var currentPos = _player.Position.ToString(@"mm\:ss");
                var duration = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");

                Title = String.Format($"{currentPos} / {duration} - {shortname}");
            }
            else
                Title = "No file selected...";
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            playListBox.ItemsSource = _playList;
        }
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            screen.Multiselect = true;
            screen.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (screen.ShowDialog() == true)
            {
                foreach (string filename in screen.FileNames)
                {
                    var info = new FileInfo(filename);
                    _playList.Add(info);
                }
            }
        }
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if(playListBox.SelectedIndex >= 0)
            {
                _lastIndex = playListBox.SelectedIndex;
                PlaySelectedIndex(_lastIndex);
            } else
            {
                MessageBox.Show("No file selected. Please try again!");
            }
        }
        private void PlaySelectedIndex(int i)
        {
            _filename = _playList[i].Name;

            _player.Open(new Uri(_playList[i].FullName, UriKind.Absolute));
            //System.Threading.Thread.Sleep(500);
            //var duration = _player.NaturalDuration.TimeSpan;
            //var testDuration = new TimeSpan(duration.Hours, duration.Minutes, duration.Seconds - 20);
            //_player.Position = testDuration;

            _player.Play();
            _isPlaying = true;
            _timer.Start();
        }
        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if(_lastIndex != -1)
            {
                if (_isPlaying)
                    _player.Pause();
                else
                    _player.Play();
                _isPlaying = !_isPlaying;
            }
            else
            {
                MessageBox.Show("Error! Please try again");
            }

        }
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastIndex != -1)
            {
                _player.Stop();
                _isPlaying = false;
            }
            else
            {
                MessageBox.Show("Error! Please try again");
            }
        }
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (playListBox.SelectedIndex >= 0)
            {
                for(int i = playListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    var item = playListBox.SelectedItems[i] as FileInfo;
                    if(string.Compare(_filename,item.Name) == 0)
                    {
                        _player.Stop();
                        _filename.Remove(0);
                        _lastIndex = -1;
                        _isPlaying = false;
                    }
                    _playList.Remove(item);

                }
            }
            else
            {
                MessageBox.Show("No file selected. Please try again!");
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {

        }

       
    }
}
