namespace WpfApp1
{
    public struct Coordinate
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public Coordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

    }

    public static class CoordinateExtensions
    {        
        public static Coordinate Left(this Coordinate c) => new Coordinate(c.X - 1, c.Y, c.Z);
        public static Coordinate Right(this Coordinate c) => new Coordinate(c.X + 1, c.Y, c.Z);
        public static Coordinate Up(this Coordinate c) => new Coordinate(c.X, c.Y + 1, c.Z);
        public static Coordinate Down(this Coordinate c) => new Coordinate(c.X, c.Y - 1, c.Z);
        public static Coordinate Before(this Coordinate c) => new Coordinate(c.X, c.Y, c.Z+1);
        public static Coordinate Behind(this Coordinate c) => new Coordinate(c.X, c.Y, c.Z-1);

    }
}