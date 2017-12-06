using System.Numerics;

namespace ConsoleApp37
{
    public struct BigIntegerPoint
    {
        public BigInteger X { get; set; }
        public BigInteger Y { get; set; }

        public BigIntegerPoint(BigInteger x, BigInteger y)
        {
            X = x;
            Y = y;
        }

        public static BigIntegerPoint GetEmpty()
        {
            return new BigIntegerPoint(0, 0);
        }

        public bool IsEmpty() => X == BigInteger.Zero && Y == BigInteger.Zero;
    }
}
