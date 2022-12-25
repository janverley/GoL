using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace WpfApp1
{
    internal class World
    {
        private const int xSize = 300;
        private const int ySize = 300;
        private const int zSize = 1;

        private const int xyzSize = xSize * ySize * zSize;

        private readonly byte[] cells = new byte[xyzSize];

        public IEnumerable<int> LiveCells
        {
            get
            {
                for (var i = 0; i < xyzSize; i++)
                {
                    if (cells[i] == 1)
                    {
                        yield return i;
                    }
                }
            }
        }


        public int Seed()
        {
            KillAllCells();

            var rand = new Random();
            var c = rand.Next(20, xyzSize / 10);
            for (var i = 0; i < c; i++)
            {
                var index = rand.Next(0, xyzSize);
                cells[index] = 1;
            }


            return c;
        }

        private void KillAllCells()
        {
            for (var i = 0; i < xyzSize; i++)
            {
                cells[i] = 0;
            }
        }

        public BoxVisual3D CreateBox(int index)
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

        public int CalculateNextGen()
        {
            var nextGenCells = new byte[xyzSize];

            for (var index = 0; index < xyzSize; index++)
            {
                var numberOfLiveNeighbours = NumberOfLiveNeighbours(index);
                if (cells[index] == 1)
                {
                    if (numberOfLiveNeighbours < 2)
                    {
                        nextGenCells[index] = 0;
                    }

                    else if (numberOfLiveNeighbours == 2 || numberOfLiveNeighbours == 3)
                    {
                        nextGenCells[index] = 1;
                    }

                    else if (numberOfLiveNeighbours > 3)
                    {
                        nextGenCells[index] = 0;
                    }
                }
                else if (numberOfLiveNeighbours == 3)
                {
                    nextGenCells[index] = 1;
                }
            }

            var count = 0;

            for (var index = 0; index < xyzSize; index++)
            {
                cells[index] = nextGenCells[index];
                if (cells[index] == 1)
                {
                    count++;
                }
            }

            return count;
        }


        private int NumberOfLiveNeighbours(int index)
        {
            return NumberOfLiveNeighboursInThisPlane(index) +
                   NumberOfLiveNeighboursInThisPlane(Before(index)) +
                   (IsAlive(Before(index)) ? 1 : 0) +
                   NumberOfLiveNeighboursInThisPlane(Behind(index)) +
                   (IsAlive(Behind(index)) ? 1 : 0);
        }

        private static int Left(int index)
        {
            return index - 1;
        }

        private static int Right(int index)
        {
            return index + 1;
        }

        private static int Up(int index)
        {
            return index + xSize;
        }

        private static int Down(int index)
        {
            return index - xSize;
        }

        private static int Before(int index)
        {
            return index - xSize * ySize;
        }

        private static int Behind(int index)
        {
            return index + xSize * ySize;
        }

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

        private bool IsAlive(int index)
        {
            if (index >= 0)
            {
                if (index < xyzSize)
                {
                    if (cells[index] == 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}