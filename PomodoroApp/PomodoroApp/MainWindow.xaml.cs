using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;

namespace PomodoroApp
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private TimeSpan timeLeft;
        private bool isRunning = false;
        private bool isWorkSession = true;
        private int currentSession = 1;

        // Timer durations in minutes
        private int workDuration = 25;
        private int shortBreakDuration = 5;
        private int longBreakDuration = 15;

        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();
            UpdateTimerDisplay();
            UpdateArcSegment(1.0); // Start with full circle
        }

        private void SetupTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            ResetTimer();
        }

        private void ResetTimer()
        {
            if (isWorkSession)
            {
                timeLeft = TimeSpan.FromMinutes(workDuration);
                TimerTypeBrush.Color = (Color)ColorConverter.ConvertFromString("#BF616A");
                ArcSegment.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF616A"));
                TimerTypeTextBlock.Text = "WORK SESSION";
            }
            else
            {
                if (currentSession % 4 == 0)
                {
                    timeLeft = TimeSpan.FromMinutes(longBreakDuration);
                    TimerTypeBrush.Color = (Color)ColorConverter.ConvertFromString("#5E81AC");
                    ArcSegment.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5E81AC"));
                    TimerTypeTextBlock.Text = "LONG BREAK";
                }
                else
                {
                    timeLeft = TimeSpan.FromMinutes(shortBreakDuration);
                    TimerTypeBrush.Color = (Color)ColorConverter.ConvertFromString("#A3BE8C");
                    ArcSegment.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A3BE8C"));
                    TimerTypeTextBlock.Text = "SHORT BREAK";
                }
            }

            UpdateTimerDisplay();
            UpdateArcSegment(1.0); // Reset to full circle
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));

            if (timeLeft.TotalSeconds <= 0)
            {
                timer.Stop();

                if (isWorkSession)
                {
                    if (currentSession % 4 == 0)
                    {
                        // Reset session count after 4 work sessions
                        currentSession = 0;
                    }

                    isWorkSession = false;
                }
                else
                {
                    isWorkSession = true;
                    currentSession++;
                    UpdateSessionIndicators();
                }

                ResetTimer();
                isRunning = false;
                UpdatePlayPauseIcon();
                // You may want to play a sound or notification here
            }

            UpdateTimerDisplay();
            UpdateProgressArc();
        }

        private void UpdateTimerDisplay()
        {
            TimerTextBlock.Text = $"{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            SessionCountTextBlock.Text = $"{currentSession}/4";
        }

        private void UpdateProgressArc()
        {
            double totalSeconds;

            if (isWorkSession)
            {
                totalSeconds = workDuration * 60;
            }
            else
            {
                if (currentSession % 4 == 0)
                {
                    totalSeconds = longBreakDuration * 60;
                }
                else
                {
                    totalSeconds = shortBreakDuration * 60;
                }
            }

            double progress = timeLeft.TotalSeconds / totalSeconds;
            UpdateArcSegment(progress);
        }

        private void UpdateArcSegment(double progress)
        {
            // This is the key function that updates the timer arc
            if (progress <= 0)
            {
                ArcSegment.Visibility = Visibility.Collapsed;
                return;
            }

            ArcSegment.Visibility = Visibility.Visible;

            // Calculate the end point of the arc based on progress
            double angle = progress * 360.0;
            double radians = (angle - 90) * (Math.PI / 180.0);

            // Size of the circle
            double radius = 115; // Slightly smaller than the 125px radius to account for StrokeThickness
            double centerX = 125;
            double centerY = 125;

            // Calculate the end point
            double endX = centerX + (radius * Math.Cos(radians));
            double endY = centerY + (radius * Math.Sin(radians));

            // Update the Arc segment
            ArcFigure.StartPoint = new Point(centerX, centerY - radius); // Top center point (12 o'clock position)
            Arc.Point = new Point(endX, endY);
            Arc.Size = new Size(radius, radius);
            Arc.IsLargeArc = angle > 180;
            Arc.SweepDirection = SweepDirection.Clockwise;
        }

        private void UpdateSessionIndicators()
        {
            Brush activeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF616A"));
            Brush inactiveBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B4252"));

            Session1.Fill = currentSession >= 1 ? activeBrush : inactiveBrush;
            Session2.Fill = currentSession >= 2 ? activeBrush : inactiveBrush;
            Session3.Fill = currentSession >= 3 ? activeBrush : inactiveBrush;
            Session4.Fill = currentSession >= 4 ? activeBrush : inactiveBrush;
        }

        private void UpdatePlayPauseIcon()
        {
            if (isRunning)
            {
                PlayPauseIcon.Data = Geometry.Parse("M14,19H18V5H14M6,19H10V5H6V19Z"); // Pause icon
            }
            else
            {
                PlayPauseIcon.Data = Geometry.Parse("M8,5.14V19.14L19,12.14L8,5.14Z"); // Play icon
            }
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                timer.Stop();
            }
            else
            {
                timer.Start();
            }

            isRunning = !isRunning;
            UpdatePlayPauseIcon();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            isRunning = false;
            ResetTimer();
            UpdatePlayPauseIcon();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            isRunning = false;

            if (isWorkSession)
            {
                isWorkSession = false;
            }
            else
            {
                isWorkSession = true;
                currentSession++;
                if (currentSession > 4) currentSession = 1;
                UpdateSessionIndicators();
            }

            ResetTimer();
            UpdatePlayPauseIcon();
        }

        private void WorkDurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            workDuration = (int)e.NewValue;
            if (isWorkSession && !isRunning)
            {
                ResetTimer();
            }
        }

        private void BreakDurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            shortBreakDuration = (int)e.NewValue;
            if (!isWorkSession && currentSession % 4 != 0 && !isRunning)
            {
                ResetTimer();
            }
        }

        private void LongBreakDurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            longBreakDuration = (int)e.NewValue;
            if (!isWorkSession && currentSession % 4 == 0 && !isRunning)
            {
                ResetTimer();
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        public class Task : INotifyPropertyChanged
        {
            private string _description;
            private bool _isCompleted;

            public string Description
            {
                get { return _description; }
                set
                {
                    if (_description != value)
                    {
                        _description = value;
                        OnPropertyChanged(nameof(Description));
                    }
                }
            }

            public bool IsCompleted
            {
                get { return _isCompleted; }
                set
                {
                    if (_isCompleted != value)
                    {
                        _isCompleted = value;
                        OnPropertyChanged(nameof(IsCompleted));
                        OnPropertyChanged(nameof(TextDecorationValue));
                    }
                }
            }

            // Use TextDecoration as a TextDecorationCollection property
            public TextDecorationCollection TextDecorationValue
            {
                get
                {
                    if (IsCompleted)
                    {
                        return TextDecorations.Strikethrough;
                    }
                    return null;
                }
            }

            public Task(string description)
            {
                _description = description;
                _isCompleted = false;
            }

            // Implement INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTask();
        }

        private void NewTaskTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AddNewTask();
            }
        }

        private void AddNewTask()
        {
            string taskDescription = NewTaskTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(taskDescription))
            {
                TasksListBox.Items.Add(new Task(taskDescription));
                NewTaskTextBox.Text = string.Empty;
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ListBoxItem item = FindAncestor<ListBoxItem>(button);
                if (item != null)
                {
                    TasksListBox.Items.Remove(item.DataContext);
                }
            }
        }

        private void TaskCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                Task task = checkBox.DataContext as Task;
                if (task != null)
                {
                    // Trigger UI update for text decoration
                    TasksListBox.Items.Refresh();
                }
            }
        }

        // Helper method to find ancestor
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
}