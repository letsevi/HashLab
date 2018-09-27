namespace HashLab.Stribog
{
    internal class EcPoint
    {
        public BigInteger A;
        public BigInteger B;
        public BigInteger X;
        public BigInteger Y;
        public BigInteger FieldChar;

        public EcPoint()
        {
            A = new BigInteger();
            B = new BigInteger();
            X = new BigInteger();
            Y = new BigInteger();
            FieldChar = new BigInteger();
        }

        public EcPoint(EcPoint p)
        {
            A = p.A;
            B = p.B;
            X = p.X;
            Y = p.Y;
            FieldChar = p.FieldChar;
        }

        //сложение пары точек.
        public static EcPoint operator +(EcPoint p1, EcPoint p2)
        {
            var res = new EcPoint
            {
                A = p1.A,
                B = p1.B,
                FieldChar = p1.FieldChar
            };

            BigInteger dx = p2.X - p1.X;
            BigInteger dy = p2.Y - p1.Y;

            if (dx < 0)
            {
                dx += p1.FieldChar;
            }
            if (dy < 0)
            {
                dy += p1.FieldChar;
            }

            BigInteger t = (dy * dx.modInverse(p1.FieldChar)) % p1.FieldChar;

            if (t < 0)
                t += p1.FieldChar;

            res.X = (t * t - p1.X - p2.X) % p1.FieldChar;
            res.Y = (t * (p1.X - res.X) - p1.Y) % p1.FieldChar;

            if (res.X < 0)
            {
                res.X += p1.FieldChar;
            }
            if (res.Y < 0)
            {
                res.Y += p1.FieldChar;
            }

            return res;
        }

        //удвоение точки.
        public static EcPoint Doubling(EcPoint p)
        {
            var res = new EcPoint
            {
                A = p.A,
                B = p.B,
                FieldChar = p.FieldChar
            };

            BigInteger dx = 2 * p.Y;
            BigInteger dy = 3 * p.X * p.X + p.A;

            if (dx < 0)
                dx += p.FieldChar;
            if (dy < 0)
                dy += p.FieldChar;

            BigInteger t = (dy * dx.modInverse(p.FieldChar)) % p.FieldChar;
            res.X = (t * t - p.X - p.X) % p.FieldChar;
            res.Y = (t * (p.X - res.X) - p.Y) % p.FieldChar;

            if (res.X < 0)
                res.X += p.FieldChar;
            if (res.Y < 0)
                res.Y += p.FieldChar;

            return res;
        }

        //умножение точки на число.
        public static EcPoint Multiply(EcPoint p, BigInteger c)
        {
            EcPoint res = p;
            c = c - 1;
            while (c != 0)
            {
                if (c % 2 != 0)
                {
                    res = (res.X == p.X || res.Y == p.Y) ? Doubling(res) : res + p;
                    c--;
                }

                c /= 2;
                p = Doubling(p);
            }

            return res;
        }
    }
}
