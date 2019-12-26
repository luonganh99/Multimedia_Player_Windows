using Gma.System.MouseKeyHook;
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
using System.Windows.Forms;
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
        private IKeyboardMouseEvents _hook;
        const string FILE_SAVE = "LastPlayList.txt";

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += timer_Tick;

            // Dang ky su kien hook
            _hook = Hook.GlobalEvents();
            _hook.KeyUp += KeyUp_hook_Play;
            _hook.KeyUp += KeyUp_hook_Next;
            _hook.KeyUp += KeyUp_hook_Prev;

            _player.MediaEnded += _player_MediaNext;
        }
        private void KeyUp_hook_Play(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control&&e.Shift&& (e.KeyCode == Keys.P))
            {
                if (_lastIndex != -1)
                {
                    if (_isPlaying)
                        _player.Pause();
                    else
                        _player.Play();
                    _isPlaying = !_isPlaying;
                }
                //_lastIndex++;
                //PlaySelectedIndex(_lastIndex);
            }
        }
        private void KeyUp_hook_Next(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control && e.Shift && (e.KeyCode == Keys.N))
            {
                if (playListBox.SelectedIndex < _playList.Count() - 1)
                {
                    //play Next
                    _lastIndex++;
                    playListBox.SelectedIndex = _lastIndex;
                    PlaySelectedIndex(_lastIndex);
                }
            }
        }
        private void KeyUp_hook_Prev(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control && e.Shift && (e.KeyCode == Keys.M))
            {
                if (playListBox.SelectedIndex >=0)
                {
                    //play Prev
                    _lastIndex--;
                    playListBox.SelectedIndex = _lastIndex;
                    PlaySelectedIndex(_lastIndex);
                }
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if (_player.Source != null && _lastIndex != -1)
            {
                var converter = new NameConverter();
                var shortname = converter.Convert(_filename, null, null, null);

                var currentPos = _player.Position.ToString(@"mm\:ss");
                var duration = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");

                //Title = String.Format($"{currentPos} / {duration} - {shortname}");
                lbTimer.Content = String.Format($"{currentPos} / {duration}");
                lbSong.Content = String.Format($"{shortname}");
            }
            else
                Title = "No file selected...";
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(File.Exists(FILE_SAVE))
            {
                var Reader = new StreamReader(FILE_SAVE);
                var fistline = Reader.ReadLine();
                int n = int.Parse(fistline);
                var secondline = Reader.ReadLine();
                _lastIndex = int.Parse(secondline);
                playListBox.SelectedIndex = _lastIndex;
                for (int i = 0; i < n; i++)
                {
                    var info = new FileInfo(Reader.ReadLine());
                    _playList.Add(info);
                }
            }
            playListBox.ItemsSource = _playList;
        }
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new Microsoft.Win32.OpenFileDialog();
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
                System.Windows.MessageBox.Show("No file selected. Please try again!");
            }
        }
        private void _player_MediaNext(object sender, EventArgs e)
        {
            //Tuần tự
            if (playListBox.SelectedIndex <_playList.Count()-1)
            {
                //play Next
                _lastIndex++;
                playListBox.SelectedIndex = _lastIndex;
                PlaySelectedIndex(_lastIndex);
            }
            else
            {
                //play lại từ đầu
                _lastIndex = 0;
                playListBox.SelectedIndex = _lastIndex;
                PlaySelectedIndex(_lastIndex);
            }
        }
        private void PlaySelectedIndex(int i)
        {
            _filename = _playList[i].Name;

            _player.Open(new Uri(_playList[i].FullName, UriKind.Absolute));
            //System.Threading.Thread.Sleep(500);
            //var duration = _player.NaturalDuration.TimeSpan;
            //var testDuration = new TimeSpan(duration.Hours, duration.Minutes, duration.Seconds - 5);
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
                System.Windows.MessageBox.Show("Error! Please try again");
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
                System.Windows.MessageBox.Show("Error! Please try again");
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
                System.Windows.MessageBox.Show("No file selected. Please try again!");
            }
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            _player.MediaEnded -= _player_MediaNext;
            _player.MediaEnded -= _player_MediaRandom;
            _player.MediaEnded += _player_MediaRepeat;


        }
        private void Random_Click(object sender, RoutedEventArgs e)
        {
            _player.MediaEnded -= _player_MediaNext;
            _player.MediaEnded -= _player_MediaRepeat;
            _player.MediaEnded += _player_MediaRandom;

        }
        private void _player_MediaRandom(object sender, EventArgs e)
        {
          
            Random random = new Random();
            _lastIndex = random.Next(0, _playList.Count() - 1);
            playListBox.SelectedIndex = _lastIndex;
            PlaySelectedIndex(_lastIndex);
        }
        private void _player_MediaRepeat(object sender, EventArgs e)
        {             
            playListBox.SelectedIndex = _lastIndex;
            PlaySelectedIndex(_lastIndex);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var screen = new Microsoft.Win32.SaveFileDialog();
            if (screen.ShowDialog() == true)
            {
                var fname = screen.FileName;
                var Writer = new StreamWriter(fname);
                Writer.WriteLine(_playList.Count());
                for (int i=0;i<_playList.Count();i++)
                {
                    Writer.WriteLine(_playList[i].FullName);
                }
                Writer.Close();
                System.Windows.MessageBox.Show("Đã lưu danh sach bài hát!");
            }          
        }
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var screen = new Microsoft.Win32.OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var fname = screen.FileName;
                var Reader = new StreamReader(fname);
                var fistline = Reader.ReadLine();
                int n = int.Parse(fistline);
                for (int i = 0; i < n; i++)
                {
                    var info = new FileInfo(Reader.ReadLine());
                    _playList.Add(info);
                    
                }
                Reader.Close();
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            var Writer = new StreamWriter(FILE_SAVE);
            Writer.WriteLine(_playList.Count());
            Writer.WriteLine(_lastIndex);
            for (int i = 0; i < _playList.Count(); i++)
            {
                Writer.WriteLine(_playList[i].FullName);
            }
            Writer.Close();

            _hook.KeyUp -= KeyUp_hook_Play;
            _hook.KeyUp -= KeyUp_hook_Next;
            _hook.KeyUp -= KeyUp_hook_Prev;
            _hook.Dispose();

        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (playListBox.SelectedIndex < _playList.Count() - 1)
            {
                //play Next
                _lastIndex++;
                playListBox.SelectedIndex = _lastIndex;
                PlaySelectedIndex(_lastIndex);
            }
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            if (playListBox.SelectedIndex >= 0)
            {
                //play Prev
                _lastIndex--;
                playListBox.SelectedIndex = _lastIndex;
                PlaySelectedIndex(_lastIndex);
            }
        }
    }
}
