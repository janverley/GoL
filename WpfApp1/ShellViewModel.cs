using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace WpfApp1
{
    public class ShellViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer timer;

        private const int xSize = 300;
        private const int ySize = 300;
        private const int zSize = 1;

        private const int xyzSize = xSize * ySize * zSize;
        
        private byte[] cells2 = new byte[xyzSize];

        private int generation;
        private int numberOfCells;
        private bool isRunning;

        public ShellViewModel()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.01);
            timer.Tick += OnTick;

            StartCommand = new DelegateCommand(Start, CanStart);
            StopCommand = new DelegateCommand(Stop, CanStop);
            TickCommand = new DelegateCommand(() => OnTick(this, EventArgs.Empty));
            SeedCommand = new DelegateCommand(Seed);

            Objects = new ObservableCollection<Visual3D>();
            Objects.Add(new DefaultLights());
            // Objects.Add(new GridLinesVisual3D { Length = xSize, Width = ySize, MinorDistance = 1, MajorDistance = 1, Thickness = 0.01 });

            Seed();
        }

        private void Seed()
        {
            KillAllCells();

            var rand = new Random();
            var c = rand.Next(20, xyzSize / 10);
            for (var i = 0; i < c; i++)
            {
                var index = rand.Next(0, xyzSize);
                cells2[index] = 1;
            }

            NumberOfCells = c;

            OnTick(this, EventArgs.Empty);
        }

        private void KillAllCells()
        {
            for (var i = 0; i < xyzSize; i++)
            {
                cells2[i] = 0;
            }
        }


        private BoxVisual3D CreateBox(int index)
        {
            var z = index / (xSize * ySize);
            var zRest = index % (xSize * ySize);

            var y = zRest / xSize;

            var x = zRest % xSize;
            return new BoxVisual3D
            {
                Center = new Point3D(x - xSize / 2, y - ySize / 2, z - zSize / 2), Fill =
                    new SolidColorBrush(ColorByNeighbours(index))
            };
        }

        private Color ColorByNeighbours(int index)
        {
            var numberOfLiveNeighbours = NumberOfLiveNeighbours(index);

            return 
                numberOfLiveNeighbours < 2 ? Colors.LightBlue : 
                numberOfLiveNeighbours > 3 ? Colors.Red : 
                Colors.Blue;
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

        private void OnTick(object sender, EventArgs e)
        {
            CalculateNextGen();

            RebuildBlocksFromCells();
        }

        private void CalculateNextGen()
        {
            Generation++;
            var cellsNextGen2 = new byte[xyzSize];

            for (var index = 0; index < xyzSize; index++)
            {
                var numberOfLiveNeighbours = NumberOfLiveNeighbours(index);
                if (IsAlive(index))
                {
                    if (numberOfLiveNeighbours < 2)
                        cellsNextGen2[index] = 0;

                    else if (numberOfLiveNeighbours == 2 || numberOfLiveNeighbours == 3)
                        cellsNextGen2[index] = 1;

                    else if (numberOfLiveNeighbours > 3)
                        cellsNextGen2[index] = 0;
                }
                else if (numberOfLiveNeighbours == 3)
                    cellsNextGen2[index] = 1;
            }

            var count = 0;

            for (var index = 0; index < xyzSize; index++)
            {
                cells2[index] = cellsNextGen2[index];
                if (cells2[index] == 1)
                {
                    count++;
                }
            }
            NumberOfCells = count;
        }

        private void RebuildBlocksFromCells()
        {
            var boxVisual3Ds = Objects.OfType<BoxVisual3D>().ToList();
            foreach (var boxVisual3D in boxVisual3Ds)
            {
                Objects.Remove(boxVisual3D);
            }

            for (var index = 0; index < xyzSize; index++)
            {
                if (cells2[index] == 1)
                {
                    Objects.Add(CreateBox(index));
                }
            }
        }

        private bool IsAlive(int index)
        {
            if (index >= 0)
            {
                if (index < xyzSize)
                {
                    if (cells2[index] == 1)
                        return true;
                }
            }

            return false;
        }

        private int NumberOfLiveNeighbours(int index)
        {
            return NumberOfLiveNeighboursInThisPlane(index) +
                   NumberOfLiveNeighboursInThisPlane(Before(index)) +
                   (IsAlive(Before(index)) ? 1 : 0) +
                   NumberOfLiveNeighboursInThisPlane(Behind(index)) +
                   (IsAlive(Behind(index)) ? 1 : 0);
        }

        public static int Left(int index) => index - 1;
        public static int Right(int index) => index + 1;
        public static int Up(int index) => index + xSize;
        public static int Down(int index) => index - xSize;
        public static int Before(int index) => index - xSize * ySize;
        public static int Behind(int index) => index + xSize * ySize;

        private int NumberOfLiveNeighboursInThisPlane(int index)
        {
            return (IsAlive(Left(index)) ? 1 : 0) +
                   (IsAlive(Right(index)) ? 1 : 0) +
                   (IsAlive(Up(index)) ? 1 : 0) +
                   (IsAlive(Down(index)) ? 1 : 0) +
                   (IsAlive(Up(Left(index))) ? 1 : 0) +
                   (IsAlive(Down(Left(index))) ? 1 : 0) +
                   (IsAlive(Up(Right(index))) ? 1 : 0) +
                   (IsAlive(Down(Right(index))) ? 1 : 0);
        }

        private bool CanStop()
        {
            return IsRunning;
        }

        private void Stop()
        {
            timer.Stop();
            IsRunning = false;
        }

        private bool CanStart()
        {
            return !IsRunning;
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