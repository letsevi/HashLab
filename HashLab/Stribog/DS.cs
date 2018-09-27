using System;

namespace HashLab.Stribog
{
    internal class Ds
    {
        private readonly BigInteger _a;
        private readonly BigInteger _b;
        private readonly BigInteger _n;
        private readonly BigInteger _p;
        private readonly byte[] _xG;
        private EcPoint _g = new EcPoint();

        public Ds(BigInteger p, BigInteger a, BigInteger b, BigInteger n, byte[] xG)
        {
            _a = a;
            _b = b;
            _n = n;
            _p = p;
            _xG = xG;
        }

        //генерация секретного ключа заданной длины.
        public BigInteger GenPrivateKey(int bitSize)
        {
            var result = new BigInteger();
            do
            {
                result.genRandomBits(bitSize, new Random());
            } while (result < 0 || result > _n);

            return result;
        }

        //генерация публичного ключа (с помощью секретного).
        public EcPoint GenPublicKey(BigInteger d)
        {
            return EcPoint.Multiply(GDecompression(), d);
        }

        //восстановление координат Y из координаты X и бита четности Y.
        private EcPoint GDecompression()
        {
            byte y = _xG[0];
            byte[] x = new byte[_xG.Length - 1];
            Array.Copy(_xG, 1, x, 0, _xG.Length - 1);
            BigInteger xcord = new BigInteger(x);
            BigInteger temp = (xcord * xcord * xcord + _a * xcord + _b) % _p;
            BigInteger beta = ModSqrt(temp, _p);
            BigInteger ycord = (beta % 2 == y % 2) ? beta : _p - beta;

            EcPoint g = new EcPoint
            {
                A = _a,
                B = _b,
                FieldChar = _p,
                X = xcord,
                Y = ycord
            };
            _g = g;
            return g;
        }

        //вычисление квадратоного корня по модулю простого числа q.
        public BigInteger ModSqrt(BigInteger a, BigInteger q)
        {
            var b = new BigInteger();
            do
            {
                b.genRandomBits(255, new Random());
            }
            while (Legendre(b, q) == 1);

            BigInteger s = 0;
            BigInteger t = q - 1;
            while ((t & 1) != 1)
            {
                s++;
                t = t >> 1;
            }

            BigInteger invA = a.modInverse(q);
            BigInteger c = b.modPow(t, q);
            BigInteger r = a.modPow(((t + 1) / 2), q);
            BigInteger d;
            for (int i = 1; i < s; i++)
            {
                BigInteger temp = 2;
                temp = temp.modPow(s - i - 1, q);
                d = (r.modPow(2, q) * invA).modPow(temp, q);
                if (d == q - 1)
                {
                    r = (r * c) % q;
                }
                c = c.modPow(2, q);
            }

            return r;
        }

        //вычисление символа Лежандра.
        public BigInteger Legendre(BigInteger a, BigInteger q)
        {
            return a.modPow((q - 1) / 2, q); // по формуле Эйлера
        }

        //формирование цифровой подписи.
        public string GenDs(byte[] h, BigInteger d)
        {
            var a = new BigInteger(h);
            BigInteger e = a % _n;
            if (e == 0)
            {
                e = 1;
            }
            var k = new BigInteger();
            BigInteger r;
            BigInteger s;
            do
            {
                do
                {
                    k.genRandomBits(_n.bitCount(), new Random());
                }
                while (k < 0 || k > _n);

                var c = EcPoint.Multiply(_g, k);
                r = c.X % _n;
                s = (r * d + k * e) % _n;
            }
            while (r == 0 || s == 0);

            string rvector = Padding(r.ToHexString(), _n.bitCount() / 4);
            string svector = Padding(s.ToHexString(), _n.bitCount() / 4);
            return rvector + svector;
        }

        //проверка цифровой подписи.
        public bool VerifDs(byte[] h, string sign, EcPoint q)
        {
            string rvector = sign.Substring(0, sign.Length / 2);
            string svector = sign.Substring(sign.Length / 2, sign.Length / 2);
            var r = new BigInteger(rvector, 16);
            var s = new BigInteger(svector, 16);

            if ((r < 1) || (r > (_n - 1)) || (s < 1) || (s > (_n - 1)))
                return (false);

            var a = new BigInteger(h);
            BigInteger e = a % _n;
            if (e == 0)
                e = 1;

            BigInteger v = e.modInverse(_n);
            BigInteger z1 = (s * v) % _n;
            BigInteger z2 = _n + ((-(r * v)) % _n);
            _g = GDecompression();
            EcPoint A = EcPoint.Multiply(_g, z1);
            EcPoint b = EcPoint.Multiply(q, z2);
            EcPoint c = A + b;
            BigInteger R = c.X % _n;
            return R == r;
        }

        //дополнить подпись нулями слева до длины n, 
        // где n - длина модуля в битах.
        private string Padding(string input, int size)
        {
            if (input.Length < size)
            {
                do
                {
                    input = "0" + input;
                }
                while (input.Length < size);
            }

            return input;
        }
    }
}
