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

        private const int xSize = 100;
        private const int ySize = 100;
        private const int zSize = 100;
        
        private readonly bool[,,] cells = new bool[xSize, ySize, zSize];
        private int generation;
        private bool isRunning;

        public ShellViewModel()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += OnTick;

            StartCommand = new DelegateCommand(Start, CanStart);
            StopCommand = new DelegateCommand(Stop, CanStop);

            Objects = new ObservableCollection<Visual3D>();
            Objects.Add(new DefaultLights());
            Objects.Add(new GridLinesVisual3D { Length = 20, Width = 20, MinorDistance = 1, MajorDistance = 1, Thickness = 0.01 });

            cells[50, 50, 50] = true;
            cells[49, 50, 50] = true;
            cells[50, 51, 50] = true;
            cells[47, 52, 50] = true;
            cells[47, 53, 50] = true;
            cells[48, 53, 50] = true;
            
            OnTick(this,EventArgs.Empty);
        }

        private static BoxVisual3D FromCoordinate(Coordinate c)
        {
            return new BoxVisual3D { Center = new Point3D(c.X - (xSize / 2), c.Y - (ySize / 2), c.Z - (zSize / 2)) };
        }

        public DelegateCommand StopCommand { get; set; }


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

        public DelegateCommand StartCommand { get; set; }

        public ObservableCollection<Visual3D> Objects { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnTick(object sender, EventArgs e)
        {
            Generation++;

            foreach (var cell in Objects.OfType<BoxVisual3D>().ToList())
            {
                Objects.Remove(cell);
            }

            for (int ix = 0; ix < 100; ix++)
            {
                for (int iy = 0; iy < 100; iy++)
                {
                    for (int iz = 0; iz < 100; iz++)
                    {
                        var c = new Coordinate(ix, iy, iz);
                        if (IsAlive(c))
                        {
                            if (NumberOfLiveNeighbours(c) < 2)
                            {
                                cells[c.X, c.Y, c.Z] = false;
                            }

                            if (NumberOfLiveNeighbours(c) == 2 || NumberOfLiveNeighbours(c) == 3)
                            {
                                cells[c.X, c.Y, c.Z] = true;
                            }

                            if (NumberOfLiveNeighbours(c) > 3)
                            {
                                cells[c.X, c.Y, c.Z] = false;
                            }
                        }
                        else
                        {
                            if (NumberOfLiveNeighbours(c) == 3)
                            {
                                cells[c.X, c.Y, c.Z] = true;
                            }
                        }
                    }
                }
            }

            for (int ix = 0; ix < 100; ix++)
            {
                for (int iy = 0; iy < 100; iy++)
                {
                    for (int iz = 0; iz < 100; iz++)
                    {
                        var c = new Coordinate(ix, iy, iz);
                        if (IsAlive(c))
                        {
                            Objects.Add(FromCoordinate(c));
                        }
                    }
                }
            }
        }

        private bool IsAlive(Coordinate p)
        {
            if (p.X < 0 || p.X > xSize -1)
            {
                return false;
            }
            if (p.Y < 0 || p.Y > ySize -1)
            {
                return false;
            }
            
            if (p.Z < 0 || p.Z > zSize-1)
            {
                return false;
            }

            return cells[p.X, p.Y, p.Z];
        }

        private int NumberOfLiveNeighbours(Coordinate p)
        {
            return NumberOfLiveNeighboursInThisPlane(p) +
                   NumberOfLiveNeighboursInThisPlane(p.Before()) +
                   (IsAlive(p.Before()) ? 1 : 0) +
                   NumberOfLiveNeighboursInThisPlane(p.Behind()) +
                   (IsAlive(p.Behind()) ? 1 : 0);
        }
        

        private int NumberOfLiveNeighboursInThisPlane(Coordinate p)
        {
            return (IsAlive(p.Left()) ? 1 : 0) +
                   (IsAlive(p.Right()) ? 1 : 0) +
                   (IsAlive(p.Up()) ? 1 : 0) +
                   (IsAlive(p.Down()) ? 1 : 0) +
                   (IsAlive(p.Left().Up()) ? 1 : 0) +
                   (IsAlive(p.Left().Down()) ? 1 : 0) +
                   (IsAlive(p.Right().Up()) ? 1 : 0) +
                   (IsAlive(p.Right().Down()) ? 1 : 0);
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