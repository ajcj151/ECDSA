using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp37
{
    public class EllipticCurveDSA
    {
        public EllipticCurve Curve { get; }

        public EllipticCurveDSA(EllipticCurve curve)
        {
            Curve = curve;
        }

        /// <summary>
        /// Generates a random private-public key pair.
        /// </summary>
        /// <returns>The tuple consists of a private and public key</returns>
        public (BigInteger privateKey, BigIntegerPoint publicKey) GenerateKeyPair()
        {
            BigInteger privateKey = RandomIntegerBelow(Curve.n);
            BigIntegerPoint publicKey = ScalarMult(privateKey, Curve.G);
            return (privateKey, publicKey);
        }

        /// <summary>
        /// Generates the truncated SHA521 hash of the <paramref name="message"/>.
        /// </summary>
        /// <param name="message"></param>
        public BigInteger HashMessage(string message)
        {
            byte[] messageBytes = Encoding.Default.GetBytes(message);
            byte[] messageHash;

            using (SHA512Managed sha512M = new SHA512Managed())
            {
                messageHash = sha512M.ComputeHash(messageBytes);
            }

            BigInteger e = new BigInteger(messageHash);
            BigInteger z = e >> (e.BitLenght() - Curve.n.BitLenght());

            if (!(z.BitLenght() <= Curve.n.BitLenght()))
            {
                throw new Exception("!(z.BitLenght() <= Curve.n.BitLenght())");
            }

            return z;
        }

        /// <summary>
        /// Generates signature.
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public (BigInteger r, BigInteger s) SignMessage(BigInteger privateKey, string message)
        {
            BigInteger z = HashMessage(message);

            BigInteger r = 0;
            BigInteger s = 0;

            while (r == 0 || s == 0)
            {
                BigInteger k = RandomIntegerBelow(Curve.n);
                BigIntegerPoint point = ScalarMult(k, Curve.G);

                r = MathMod(point.X, Curve.n);
                s = MathMod((z + r * privateKey) * InverseMod(k, Curve.n), Curve.n);
            }

            return (r, s);
        }

        public bool VerifySignature(BigIntegerPoint publicKey, string message,
            (BigInteger r, BigInteger s) signature)
        {
            BigInteger z = HashMessage(message);

            BigInteger w = InverseMod(signature.s, Curve.n);
            BigInteger u1 = MathMod((z * w), Curve.n);
            BigInteger u2 = MathMod((signature.r * w), Curve.n);

            BigIntegerPoint point = PointAdd(ScalarMult(u1, Curve.G), ScalarMult(u2, publicKey));
            return MathMod(signature.r, Curve.n) == MathMod(point.X, Curve.n);
        }

        /// <summary>
        /// Returns k * point computed using the double and <see cref="PointAdd"/> algorithm.
        /// </summary>
        /// <param name="k"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public BigIntegerPoint ScalarMult(BigInteger k, BigIntegerPoint point)
        {
            if (MathMod(k, Curve.n) == 0 || point.IsEmpty())
            {
                return point;
            }

            if (k < 0)
            {
                return ScalarMult(-k, NegatePoint(point));
            }

            BigIntegerPoint result = BigIntegerPoint.GetEmpty();
            BigIntegerPoint addend = point;

            while (k > 0)
            {
                if ((k & 1) > 0)
                {
                    result = PointAdd(result, addend);
                }

                addend = PointAdd(addend, addend);

                k >>= 1;
            }

            return result;
        }

        /// <summary>
        /// Returns the result of point1 + point2 according to the group law.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public BigIntegerPoint PointAdd(BigIntegerPoint point1, BigIntegerPoint point2)
        {
            if (point1.IsEmpty())
            {
                return point2;
            }

            if (point2.IsEmpty())
            {
                return point1;
            }

            BigInteger x1 = point1.X, y1 = point1.Y;
            BigInteger x2 = point2.X, y2 = point2.Y;

            if (x1 == x2 && y1 != y2)
            {
                return BigIntegerPoint.GetEmpty();
            }

            BigInteger m = BigInteger.Zero;
            if (x1 == x2)
            {
                m = (3 * x1 * x1 + Curve.a) * InverseMod(2 * y1, Curve.p);
            }
            else
            {
                m = (y1 - y2) * InverseMod(x1 - x2, Curve.p);
            }

            BigInteger x3 = m * m - x1 - x2;
            BigInteger y3 = y1 + m * (x3 - x1);
            BigIntegerPoint result = new BigIntegerPoint(MathMod(x3, Curve.p), MathMod(-y3, Curve.p));

            return result;
        }

        /// <summary>
        /// Returns the inverse of k modulo p.
        /// </summary>
        /// <param name="k">Must be non-zero</param>
        /// <param name="p">Must be a prime</param>
        /// <returns></returns>
        public BigInteger InverseMod(BigInteger k, BigInteger p)
        {
            if (k == 0)
            {
                throw new DivideByZeroException(nameof(k));
            }

            if (k < 0)
            {
                return p - InverseMod(-k, p);
            }

            (BigInteger gcd, BigInteger x, BigInteger y) result = ExtendedGcd(p, k);

            if (result.gcd != 1)
            {
                throw new Exception("Gcd is not 1");
            }

            if (MathMod(k * result.x, p) != 1)
            {
                throw new Exception("(k * result.x) % p != 1");
            }

            return MathMod(result.x, p);
        }

        public static BigInteger MathMod(BigInteger a, BigInteger b)
        {
            return (BigInteger.Abs(a * b) + a) % b;
        }

        /// <summary>
        /// Extended Euclidean algorithm.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b)
        {
            BigInteger s = 0, old_s = 1;
            BigInteger t = 1, old_t = 0;
            BigInteger r = a, old_r = b;

            while (r != 0)
            {
                BigInteger q = old_r / r;
                var temp = r;
                r = old_r - q * r;
                old_r = temp;

                temp = s;
                s = old_s - q * s;
                old_s = temp;

                temp = t;
                t = old_t - q * t;
                old_t = temp;
            }

            return (old_r, old_s, old_t);
        }

        /// <summary>
        /// Returns -point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public BigIntegerPoint NegatePoint(BigIntegerPoint point)
        {
            if (point.IsEmpty())
            {
                return point;
            }

            BigInteger x = point.X;
            BigInteger y = point.Y;
            BigIntegerPoint result = new BigIntegerPoint(x, MathMod(-y, Curve.p));

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if the given point lies on the elliptic curve.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsOnCurve(BigIntegerPoint point)
        {
            if (point.IsEmpty())
            {
                return true;
            }

            BigInteger x = point.X;
            BigInteger y = point.Y;

            return MathMod(y * y - x * x * x - Curve.a * x - Curve.b, Curve.p) == 0;
        }

        public static BigInteger RandomIntegerBelow(BigInteger N)
        {
            byte[] bytes = N.ToByteArray();
            BigInteger R;
            Random random = new Random();
            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= (byte) 0x7F;
                R = new BigInteger(bytes);
            } while (R >= N && R >= 1);

            return R;
        }
    }
}