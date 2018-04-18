using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
namespace ConsoleApp4
{
    class Program
    {
        static float[] GetVs(int k)
        {
            int cnt = 10000;
            var tmp = new float[cnt];
            for (int i = 0; i < cnt; i++)
            {
                tmp[i] = k + i;
            }
            return tmp;
        }
        unsafe static void Main(string[] args)
        {
              
            AVXTest<float> a = new AVXTest<float>(GetVs(0));
            AVXTest<float> b = new AVXTest<float>(GetVs(1));
            AVXTest<float> c = new AVXTest<float>(GetVs(2));
            Vec aa = new Vec(GetVs(0));
            Vec ab = new Vec(GetVs(1));
            Vec ac = new Vec(GetVs(2));
           float rtn = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                AVXTest<float>.dot(ref a,ref b , ref rtn );
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
      //      Console.WriteLine(rtn);

            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
               rtn = Vec.dot(aa,ab);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
     //       Console.WriteLine(rtn);


        }
    }

    class Vec
    {
        float[] Array;
        public Vec(float[] Array)
        {
            this.Array = Array;
        }

        public static Vec mul(Vec v0, Vec v1)

        {
            if (v0.Array.Length != v1.Array.Length) throw new NotSupportedException();
            var rtn = new Vec(new float[v0.Array.Length]);
            for (int i = 0; i < v0.Array.Length; i++)
            {
                rtn.Array[i] = v0.Array[i] * v1.Array[i];
            }
            return rtn;
        }

        public static float dot(Vec v0, Vec v1)

        {
            if (v0.Array.Length != v1.Array.Length) throw new NotSupportedException();
            float rtn = 0;
            for (int i = 0; i < v0.Array.Length; i++)
            {
                rtn+= v0.Array[i] * v1.Array[i];
            }
            return rtn;
        }

        public static Vec Add(Vec v0, Vec v1)

        {
            if (v0.Array.Length != v1.Array.Length) throw new NotSupportedException();
            var rtn = new Vec(new float[v0.Array.Length]);
            for (int i = 0; i < v0.Array.Length; i++)
            {
                rtn.Array[i] = v0.Array[i] + v1.Array[i];
            }
            return rtn;
        }


        public static float Sum(Vec v0 )

        {
            float rtn = 0;
            for (int i = 0; i < v0.Array.Length; i++)
            {
                rtn+= v0.Array[i]  ;
            }
            return rtn;
        }
    }

    unsafe class AVXTest<T> where T : struct
    {
     public   int[] intArray;
     public   float[] floatArray;
        enum _Type
        {
            _int,
            _float,
        }
        _Type _T;
        public AVXTest(T[] Array)
        {
            if (typeof(T) == typeof(int))
            {
                this._T = _Type._int;
                this.intArray = (int[])(object)Array;
            }
            else if (typeof(T) == typeof(float))
            {
                this._T = _Type._float;
                this.floatArray = (float[])(object)Array;
            }
        }

        ~AVXTest()
        {

        }

        public int GetArrayLng()
        {
            if (this._T == _Type._int)
                return   intArray.Length;
            if (this._T == _Type._float)
                return  floatArray.Length;
            throw new Exception();
        }
        public static void mul(ref AVXTest<T> data0, ref AVXTest<T> data1, ref AVXTest<T> rtndata)
        {
            int ArrayLast = data0.GetArrayLng();
            if (data0._T != data1._T) throw new NotSupportedException();
            if (ArrayLast != data1.GetArrayLng()) throw new NotSupportedException();
            switch (data0._T)
            {
                case _Type._int:

                     throw new Exception();

                case _Type._float:

                    fixed (float* p0 = data0.floatArray)
                    fixed (float* p1 = data1.floatArray)
                    fixed (float* rtn = rtndata.floatArray)
                    {
                        float* pp0 = p0;
                        float* pp1 = p1;
                        float* ppr = rtn;
                        var last = ArrayLast - 8;
                        int i = 0;
                        for (; i < last; i += 8)
                        {
                            Vector256<float> vector0 = Avx.LoadVector256(pp0);
                            Vector256<float> vector1 = Avx.LoadVector256(pp1);
                            var ans = Avx.Multiply(vector0, vector1);

                            Avx.Store(ppr, ans);
                            pp0 += 8;
                            pp1 += 8;
                            ppr += 8;
                        }
                        for (; i < ArrayLast; i++)
                        {
                            *ppr = *pp0 * *pp1;
                            ppr++;
                            pp0++;
                            pp1++;
                        }

                    }

                    break;
                default:
                    throw new Exception();
            }

        }


        public static void  Add(ref AVXTest<T> data0,ref AVXTest<T> data1,ref AVXTest<T> rtndata)
        {
            int ArrayLast = data0.GetArrayLng();
            if (data0._T != data1._T) throw new NotSupportedException();
            if (ArrayLast != data1.GetArrayLng()) throw new NotSupportedException();
            switch (data0._T)
            {
                case _Type._int:

                    fixed (int* p0 = data0.intArray)
                    fixed (int* p1 = data1.intArray)
                    fixed (int* rtn = rtndata.intArray)
                    {
                        int* pp0 = p0;
                        int* pp1 = p1;
                        int* ppr = rtn;
                        var last = ArrayLast - 8;
                        int i = 0;
                        for (; i < last; i += 8)
                        {
                            Vector256<int> vector0 = Avx.LoadVector256(pp0);
                            Vector256<int> vector1 = Avx.LoadVector256(pp1);
                            var ans = Avx2.Add(vector0, vector1);
                            Avx.Store(ppr, ans);
                            pp0 += 8;
                            pp1 += 8;
                            ppr += 8;
                        }
                        for (; i < ArrayLast; i++)
                        {
                            *ppr = *pp0 + *pp1;
                            ppr++;
                            pp0++;
                            pp1++;
                        }
                    }
                    break;
                case _Type._float:
                    throw new Exception();

                default:
                    throw new Exception();
            }
            
        }

        public static void dot(ref AVXTest<T> data0, ref AVXTest<T> data1, ref T rtndata)
        {
            int ArrayLast = data0.GetArrayLng();
            if (data0._T != data1._T) throw new NotSupportedException();
            if (ArrayLast != data1.GetArrayLng()) throw new NotSupportedException();
            switch (data0._T)
            {
                case _Type._int:
                    throw new Exception();

                case _Type._float:
                    float[] tmpArray = new float[8];

                    fixed (float* p0 = data0.floatArray)
                    fixed (float* tmpArrayP = tmpArray)
                    fixed (float* p1 = data1.floatArray)
                    {
                        float* pp0 = p0;
                        float* pp1 = p1;
                        var last = ArrayLast - 8;
                        int i = 0;
                        Vector256<float> tmp = new Vector256<float>();
                        for (; i < last; i += 8)
                        {
                            tmp = Avx.Add(tmp, Avx.Multiply(Avx.LoadVector256(pp0), Avx.LoadVector256(pp1)));
                            pp0 += 8;
                            pp1 += 8;
                        }
                        float rtn = 0;
                        for (; i < ArrayLast; i++)
                        {
                            rtn += *pp0 * *pp1;
                            pp0++;
                            pp1++;
                        }
                        Avx.Store(tmpArrayP, tmp);
                        rtn += tmpArray[0] +
                            tmpArray[1] +
                            tmpArray[2] +
                            tmpArray[3] +
                            tmpArray[4] +
                            tmpArray[5] +
                            tmpArray[6] +
                    tmpArray[7];

                        rtndata = (T)(object)rtn;

                    }
                    break;

                default:
                    throw new Exception();
            }

        }


        public static void Sum(ref AVXTest<T> data0,ref T rtn)
        {
            int ArrayLast = data0.GetArrayLng();
            switch (data0._T)
            {
                case _Type._int:
                    {
                        int[] tmpArray = new int[8];
                        fixed (int* p0 = data0.intArray)
                        fixed (int* tp = tmpArray)
                        {
                            int* pp0 = p0;
                            var last = ArrayLast - 8;
                            int i = 0;
                            Vector256<int> tmp = new Vector256<int>();

                            for (; i < last; i += 8)
                            {
                                Vector256<int> vector0 = Avx.LoadVector256(pp0);
                                tmp = Avx2.Add(tmp, vector0);
                                pp0 += 8;
                            }
                            int trtn = 0;
                            for (; i < ArrayLast; i++)
                            {
                                trtn += *pp0;
                                pp0++;
                            }
                            Avx.Store(tp, tmp);
                            trtn +=
                            tmpArray[0] +
                           tmpArray[1] +
                           tmpArray[2] +
                           tmpArray[3] +
                           tmpArray[4] +
                           tmpArray[5] +
                           tmpArray[6] +
                           tmpArray[7];
                            rtn = (T)(object)trtn;
                        }
                    }
                    break;
                case _Type._float:
                    {
                        float[] tmpArray = new float[8];
                        fixed (float* p0 = data0.floatArray)
                        fixed (float* tp = tmpArray)
                        {
                            float* pp0 = p0;
                            var last = ArrayLast - 8;
                            int i = 0;
                            Vector256<float> tmp = new Vector256<float>();

                            for (; i < last; i += 8)
                            {
                                Vector256<float> vector0 = Avx.LoadVector256(pp0);
                                tmp = Avx.Add(tmp, vector0);
                                pp0 += 8;
                            }
                            float trtn = 0;
                            for (; i < ArrayLast; i++)
                            {
                                trtn += *pp0;
                                pp0++;
                            }
                            Avx.Store(tp, tmp);
                            trtn +=
                            tmpArray[0] +
                           tmpArray[1] +
                           tmpArray[2] +
                           tmpArray[3] +
                           tmpArray[4] +
                           tmpArray[5] +
                           tmpArray[6] +
                           tmpArray[7];
                            rtn = (T)(object)trtn;
                        }
                    }
                    break;
                default:
                    throw new Exception();

            }
            
        }

    }
}
