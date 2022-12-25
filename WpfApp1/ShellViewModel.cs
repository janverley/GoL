using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace WpfApp1
{
    public class ShellViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer timer;


        private int generation;
        private bool isRunning;
        private int numberOfCells;

        private readonly World world = new World();

        public ShellViewModel()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.01);
            timer.Tick += OnTick;

            StartCommand = new DelegateCommand(Start, ()=> !IsRunning);
            StopCommand = new DelegateCommand(Stop, ()=>IsRunning);
            TickCommand = new DelegateCommand(() => OnTick(this, EventArgs.Empty));
            SeedCommand = new DelegateCommand(DoSeed);

            Objects = new ObservableCollection<Visual3D>();
            Objects.Add(new DefaultLights());
            // Objects.Add(new GridLinesVisual3D { Length = xSize, Width = ySize, MinorDistance = 1, MajorDistance = 1, Thickness = 0.01 });

            NumberOfCells = world.Seed();

            OnTick(this, EventArgs.Empty);
        }


        public DelegateCommand StopCommand { get; set; }
        public DelegateCommand TickCommand { get; set; }
        public DelegateCommand SeedCommand { get; set; }


        public bool IsRunning
        {
            get => isRunning;
            private set
            {
                if (value == isRunning)
                {
                    return;
                }

                isRunning = value;
                OnPropertyChanged();
                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            }
        }


        public int Generation
        {
            get => generation;
            private set => SetField(ref generation, value);
        }

        public int NumberOfCells
        {
            get => numberOfCells;
            private set => SetField(ref numberOfCells, value);
        }

        public DelegateCommand StartCommand { get; set; }

        public ObservableCollection<Visual3D> Objects { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void DoSeed()
        {
            NumberOfCells = world.Seed();
        }

        private void OnTick(object sender, EventArgs e)
        {
            Generation++;
            NumberOfCells = world.CalculateNextGen();

            var boxVisual3Ds = Objects.OfType<BoxVisual3D>().ToList();
            foreach (var boxVisual3D in boxVisual3Ds)
            {
                Objects.Remove(boxVisual3D);
            }

            foreach (var index in world.LiveCells)
            {
                Objects.Add(world.CreateBox(index));
            }
        }

        private void Stop()
        {
            timer.Stop();
            IsRunning = false;
        }

        private void Start()
        {
            timer.Start();
            IsRunning = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}