using System;
using System.Numerics;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Utilities
{
    internal static class CalcUtilities
    {
        public static int ToFixed(float value) => (int)(value * (1 << 10));
        public static float ToFloat(int fixedValue) => fixedValue * (1.0f / (1 << 10));

        public static int ToFixed(int value) => value << 10;
        public static int ToInt32(int fixedValue) => fixedValue >> 10;

        public static float Bezier(float p0, float p1, float p2, float p3, float amount)
        {
            // De-Casteljau Algorithm
            float c10 = Mix(p0, p1, amount);
            float c11 = Mix(p1, p2, amount);
            float c12 = Mix(p2, p3, amount);

            float c20 = Mix(c10, c11, amount);
            float c21 = Mix(c11, c12, amount);

            return Mix(c20, c21, amount); // c30
        }

        public static float Mix(float a, float b, float amount)
        {
            return a + (b - a) * amount;
        }

        public static void ValidateFCurve(Vector2 p0, ref Vector2 p1, ref Vector2 p2, Vector2 p3)
        {
            // validate the bezier curve
            p1.X = Math.Clamp(p1.X, p0.X, p3.X);
            p2.X = Math.Clamp(p2.X, p0.X, p3.X);
        }

        public static double CubicRoot(double x)
        {
            if (x == 0.0)
                return 0.0;
            else if (x < 0.0)
                return -Math.Exp(Math.Log(-x) / 3.0);
            else
                return Math.Exp(Math.Log(x) / 3.0);
        }

        public static float SolveBezier(float x, float p0, float p1, float p2, float p3)
        {
            // check for valid f-curve
            // we only take care of monotonic bezier curves, so there has to be exactly 1 real solution

            //tl_assert(p0 <= x && x <= p3);
            //tl_assert((p0 <= p1 && p1 <= p3) && (p0 <= p2 && p2 <= p3));

            double a, b, c, t;
            double x3 = -p0 + 3 * p1 - 3 * p2 + p3;
            double x2 = 3 * p0 - 6 * p1 + 3 * p2;
            double x1 = -3 * p0 + 3 * p1;
            double x0 = p0 - x;

            if (x3 == 0.0 && x2 == 0.0)
            {
                // linear
                // a*t + b = 0
                a = x1;
                b = x0;

                if (a == 0.0)
                    return 0.0f;
                else
                    return (float)(-b / a);
            }
            else if (x3 == 0.0)
            {
                // quadratic
                // t*t + b*t +c = 0
                b = x1 / x2;
                c = x0 / x2;

                if (c == 0.0)
                    return 0.0f;

                double D = b * b - 4 * c;

                t = (-b + Math.Sqrt(D)) / 2;

                if (0.0 <= t && t <= 1.0001f)
                    return (float)t;
                else
                    return (float)((-b - Math.Sqrt(D)) / 2);
            }
            else
            {
                // cubic
                // t*t*t + a*t*t + b*t*t + c = 0
                a = x2 / x3;
                b = x1 / x3;
                c = x0 / x3;

                // substitute t = y - a/3
                double sub = a / 3.0;

                // depressed form x^3 + px + q = 0
                // cardano's method
                double p = b / 3 - a * a / 9;
                double q = (2 * a * a * a / 27 - a * b / 3 + c) / 2;

                double D = q * q + p * p * p;

                if (D > 0.0)
                {
                    // only one 'real' solution
                    double s = Math.Sqrt(D);
                    return (float)(CubicRoot(s - q) - CubicRoot(s + q) - sub);
                }
                else if (D == 0.0)
                {
                    // one single, one double solution or triple solution
                    double s = CubicRoot(-q);
                    t = 2 * s - sub;

                    if (0.0 <= t && t <= 1.0001f)
                        return (float)t;
                    else
                        return (float)(-s - sub);

                }
                else
                {
                    // Casus irreductibilis ... ,_,
                    double phi = Math.Acos(-q / Math.Sqrt(-(p * p * p))) / 3;
                    double s = 2 * Math.Sqrt(-p);

                    t = s * Math.Cos(phi) - sub;

                    if (0.0 <= t && t <= 1.0001f)
                        return (float)t;

                    t = -s * Math.Cos(phi + Math.PI / 3) - sub;

                    if (0.0 <= t && t <= 1.0001f)
                        return (float)t;
                    else
                        return (float)(-s * Math.Cos(phi - Math.PI / 3) - sub);
                }
            }
        }
    }
}
