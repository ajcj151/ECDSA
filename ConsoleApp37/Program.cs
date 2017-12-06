using System;

namespace ConsoleApp37
{
    class Program
    {
        static void Main(string[] args)
        {
            EllipticCurve ellipticCurve = new EllipticCurve();
            EllipticCurveDSA ellipticCurveDsa = new EllipticCurveDSA(ellipticCurve);
            var keyPair = ellipticCurveDsa.GenerateKeyPair();
            Console.WriteLine($"Private key: {keyPair.privateKey}");
            Console.WriteLine($"Public key: {keyPair.publicKey.X}, {keyPair.publicKey.Y}");

            string message = "Hello!";
            var signature = ellipticCurveDsa.SignMessage(keyPair.privateKey, message);

            Console.WriteLine();

            Console.WriteLine($"Message: {message}");
            Console.WriteLine($"Signature: {signature.r}, {signature.s}");
            string verification = ellipticCurveDsa.VerifySignature(keyPair.publicKey, message, signature)
                ? "Signature matches"
                : "Invalid signature";
            Console.WriteLine(verification);

            message = "Hi there!";
            Console.WriteLine();
            Console.WriteLine($"Message: {message}");
            verification = ellipticCurveDsa.VerifySignature(keyPair.publicKey, message, signature)
                ? "Signature matches"
                : "Invalid signature";
            Console.WriteLine(verification);

            message = "Hello";
            Console.WriteLine();
            Console.WriteLine($"Message: {message}");
            Console.WriteLine($"Public key: {keyPair.publicKey.X}, {keyPair.publicKey.Y}");
            verification = ellipticCurveDsa.VerifySignature(keyPair.publicKey, message, signature)
                ? "Signature matches"
                : "Invalid signature";
            Console.WriteLine(verification);
        }
    }
}