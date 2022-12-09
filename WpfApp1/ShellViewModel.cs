using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace WpfApp1
{
    public class ShellViewModel : INotifyPropertyChanged
    {
        DispatcherTimer timer;

        public ShellViewModel()
        {
            timer = new DispatcherTimer();
timer.Interval = TimeSpan.FromSeconds(0.5);
timer.Tick += OnTick;

            StartCommand = new DelegateCommand(this.Start, this.CanStart);
            StopCommand = new DelegateCommand(this.Stop, this.CanStop);
            
            this.Objects = new ObservableCollection<Visual3D>();
            Objects.Add(new DefaultLights());
            Objects.Add(new GridLinesVisual3D{Length = 20, Width = 20, MinorDistance=1, MajorDistance=1 ,Thickness=0.01});
            Objects.Add(new BoxVisual3D { Center = new Point3D(0,0,0) });
            Objects.Add(new BoxVisual3D { Center = new Point3D(0,1,0) });
            Objects.Add(new BoxVisual3D { Center = new Point3D(1,1,0) });
        }

        private void OnTick(object sender, EventArgs e)
        {
            Generation++;

            foreach (var cell in Objects.OfType<BoxVisual3D>())
            {
                
            }
        }

        public DelegateCommand StopCommand { get; set; }

        private bool CanStop() => IsRunning;

        private void Stop()
        {
            timer.Stop();
            IsRunning = false;
        }


        public bool IsRunning
        {
            get => isRunning;
            private set
            {
                if (value == isRunning) return;
                isRunning = value;
                OnPropertyChanged();
                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            }
        }

        private int generation = 0;
        private bool isRunning = false;

        public int Generation
        {
            get => generation;
            private set => SetField(ref generation, value);
        }

        private bool CanStart() => !IsRunning;

        private void Start()
        {
            timer.Start();
            IsRunning = true;
        }

        public DelegateCommand StartCommand { get; set; }

        public ObservableCollection<Visual3D> Objects { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}