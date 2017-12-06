using System.Numerics;

namespace ConsoleApp37
{
    public class EllipticCurve
    {
        //  Recommended parameters secp128r1. Reference: http://www.secg.org/SEC2-Ver-1.0.pdf

        // Field characteristic.
        public BigInteger p { get; } = BigInteger.Parse("340282366762482138434845932244680310783");

        // Curve coefficients.
        public BigInteger a { get; } = BigInteger.Parse("340282366762482138434845932244680310780");
        public BigInteger b { get; } = BigInteger.Parse("308990863222245658030922601041482374867");

        // Base point.
        public BigIntegerPoint G { get; } = new BigIntegerPoint(
            BigInteger.Parse("29408993404948928992877151431649155974"),
            BigInteger.Parse("275621562871047521857442314737465260675"));

        // Subgroup order.
        public BigInteger n { get; } = BigInteger.Parse("340282366762482138443322565580356624661");

        // Subgroup cofactor.
        public int h { get; } = 1;
    }
}