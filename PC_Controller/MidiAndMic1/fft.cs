using MidiAndMic1;
using System;
using System.Collections.Generic;
using System.Threading;

public class FFT
{
    public Queue<byte[]> queue = new Queue<byte[]>();
    public Queue<int> queue4 = new Queue<int>();
    public Queue<int> queue3 = new Queue<int>();
    public Queue<int> queue2 = new Queue<int>();
    public Queue<int> queue1 = new Queue<int>();
    public byte []number = new byte[5]{0x00,0x00,0x00,0x00,0x00};
    public int selectNumber = -1;
    public int FFT_N = 2048;
    public int FFT_n = 2048;
    public static float PI = 3.14159265358979323846f;
    public int threshold3 = 64;
    public int uintThreshold3 = 200;
    public int uintThreshold4 = 200;
    public int threshold4 = 64;
    public int pre_min1 = 0;
    public int pre_max1 = 11624;
    public int pre_min2 = 0;
    public int pre_max2 = 11624;
    public int a = 0;
    public int[] valueList2 = {0,10,18,24,44,72,118,192,314,514,840,1376};
    public int[] valueList1 = {0, 10, 18, 24, 44, 72, 118, 192, 314, 514, 840, 1376 };
    public double minInteral1 = 0.0;
    public double minInteral2 = 0.0;
    public bool numberEmpty ()
    {
        for(int i = 0;i < number.Length;i++)
        {
            if(number[i] != 0x00)
            {
                return false;
            }
        }
        return true;
    }
    public void update_valueList1(int min,int max)
    {
        pre_min1 = min;
        pre_max1 = max;
        double fmax = (double)max;
        double fmin = (double)min;
        double multiple = Math.Pow(fmax / fmin, 1.0/12);
        minInteral1 = 22.0 * 1024 / FFT_N;
        valueList1[0] = (int)(fmin / minInteral1);
        for(int i = 1;i < 12;i++)
        {
            fmin = fmin * multiple;
            valueList1[i] = (int)(fmin / minInteral1);
        }
        ;
    }
    public void update_valueList2(int min, int max)
    {
        pre_min2 = min;
        pre_max2 = max;
        double fmax = (double)max;
        double fmin = (double)min;
        double multiple = Math.Pow(fmax / fmin, 1.0 / 12);
        minInteral2 = 22.0 * 1024 / FFT_N;
        valueList2[0] = (int)(fmin / minInteral2);
        for (int i = 1; i < 12; i++)
        {
            fmin = fmin * multiple;
            valueList2[i] = (int)(fmin / minInteral2);
        }
        ;
    }
    public bool numberAdd(int number)
    {
        if(number >= 0 && number < this.number.Length)
        {
            this.number[number] = (byte)number;
            return true;
        }
        return false;
    }
    public void numberClear()
    {
        for(int i = 0;i<this.number.Length;i++)
        {
            number[i] = 0x00;
        }
    }
    private void swap(ref float a, ref float b)
    {
        float temp;
        temp = a;
        a = b;
        b = temp;
    }
    void bitrp(float[] xreal, float[] ximag, int n)
    {
        // 位反转置换 Bit-reversal Permutation
        int i, j, a, b, p;

        for (i = 1, p = 0; i < n; i *= 2)
        {
            p++;
        }
        for (i = 0; i < n; i++)
        {
            a = i;
            b = 0;
            for (j = 0; j < p; j++)
            {
                b = (b << 1) + (a & 1);    // b = b * 2 + a % 2;
                a >>= 1;        // a = a / 2;
            }
            if (b > i)
            {
                swap(ref xreal[i], ref xreal[b]);
                swap(ref ximag[i], ref ximag[b]);
            }
        }
    }
    public void calcu(byte[] dataByte)
    {
        float[] dataFloat = new float[512];
        for (int i = 0; i < 512; i++)
        {
            dataFloat[i] = 0;
        }
        for (int i = 0; i < dataByte.Length; i += 2)
        {
            int temp = ((dataByte[i + 1] << 8) | (dataByte[i])) << 48 >> 48;
            dataFloat[i >> 1] = (float)temp;
        }
        calculate(dataFloat);
    }
    public void calculate(float[] xreal)
    {
        // 快速傅立叶变换，将复数 x 变换后仍保存在 x 中，xreal, ximag 分别是 x 的实部和虚部
        float[] testData = new float[FFT_N];
        for (int i = 0; i < FFT_N; i++)
        {
            testData[i] = xreal[i];
        }
        float[] wreal = new float[FFT_N / 2];
        float[] wimag = new float[FFT_N / 2];
        float[] ximag = new float[FFT_N];
        for (int i = 0; i < ximag.Length; i++)
        {
            ximag[i] = 0.0f;
        }
        float treal, timag, ureal, uimag, arg;
        int m, k, j, t, index1, index2;

        bitrp(xreal, ximag, FFT_n);

        // 计算 1 的前 n / 2 个 n 次方根的共轭复数 W'j = wreal [j] + i * wimag [j] , j = 0, 1, ... , n / 2 - 1
        arg = -2 * PI / FFT_n;
        treal = (float)Math.Cos(arg);
        timag = (float)Math.Sin(arg);
        wreal[0] = 1.0f;
        wimag[0] = 0.0f;
        for (j = 1; j < FFT_n / 2; j++)
        {
            wreal[j] = wreal[j - 1] * treal - wimag[j - 1] * timag;
            wimag[j] = wreal[j - 1] * timag + wimag[j - 1] * treal;
        }

        for (m = 2; m <= FFT_n; m *= 2)
        {
            for (k = 0; k < FFT_n; k += m)
            {
                for (j = 0; j < m / 2; j++)
                {
                    index1 = k + j;
                    index2 = index1 + m / 2;
                    t = FFT_n * j / m;    // 旋转因子 w 的实部在 wreal [] 中的下标为 t
                    treal = wreal[t] * xreal[index2] - wimag[t] * ximag[index2];
                    timag = wreal[t] * ximag[index2] + wimag[t] * xreal[index2];
                    ureal = xreal[index1];
                    uimag = ximag[index1];
                    xreal[index1] = ureal + treal;
                    ximag[index1] = uimag + timag;
                    xreal[index2] = ureal - treal;
                    ximag[index2] = uimag - timag;
                }
            }
        }
        int max = FFT_N - 1;
        float maxTemp = 0;
        for (int i = FFT_N - 1; i > FFT_N / 2 - 1; i--)
        {
            if ((xreal[i] * xreal[i] + ximag[i] * ximag[i]) > maxTemp)
            {
                max = i;
                maxTemp = (xreal[i] * xreal[i] + ximag[i] * ximag[i]);
            }
        }

        byte[] shi1 = System.BitConverter.GetBytes((int)maxTemp);
        if (number[1] == 1)
        {
            int maxReal = FFT_N - max;
            int maxResult = 0;
            for (int i = 0; i < valueList1.Length; i++)
            {
                if (maxReal > valueList1[i])
                {
                    maxResult = i;
                }
                else
                {
                    break;
                }
            }
            byte[] shi = System.BitConverter.GetBytes(maxResult);
            byte[] fftresult = new byte[4];
            fftresult[0] = 0xA0;
            fftresult[1] = shi[0];
            if (maxTemp >= 127)
            {
                fftresult[2] = 127;
            }
            else
            {
                fftresult[2] = shi1[0];
            }
            fftresult[3] = number[1];
            Monitor.Enter(queue);
            queue.Enqueue(fftresult);
            Monitor.Exit(queue);
            Monitor.Enter(queue1);
            queue1.Enqueue(maxResult);
            Monitor.Exit(queue1);


        }
        if (number[2] == 2)
        {
            int maxReal = FFT_N - max;
            int maxResult = 0;
            for (int i = 0; i < valueList2.Length; i++)
            {
                if (maxReal > valueList2[i])
                {
                    maxResult = i;
                }
                else
                {
                    break;
                }
            }
            byte[] shi = System.BitConverter.GetBytes(maxResult);
            byte[] fftresult = new byte[4];
            fftresult[0] = 0xA0;
            fftresult[1] = shi[0];
            
            if (maxTemp >= 127)
            {
                fftresult[2] = 127;
            }
            else
            {
                fftresult[2] = shi1[0];
            }
            fftresult[3] = number[2];
            Monitor.Enter(queue);
            queue.Enqueue(fftresult);
            Monitor.Exit(queue);
            Monitor.Enter(queue2);
            queue2.Enqueue(maxResult);
            Monitor.Exit(queue2);
        }
        if (number[3] == 3)
        {
            byte[] fftresult = new byte[4];
            fftresult[0] = 0xA0;
            int value = (int)maxTemp;
            value /= uintThreshold3;
            if (value > 127)
                value = 127;
            Monitor.Enter(queue3);
            queue3.Enqueue(value);
            Monitor.Exit(queue3);
            fftresult[2] = (byte)value;
            if (fftresult[2] >= threshold3)
            {
                fftresult[1] = 0x01;
            }
            else
            {
                fftresult[1] = 0x00;
            }
            fftresult[3] = number[3];
            Monitor.Enter(queue);
            queue.Enqueue(fftresult);
            Monitor.Exit(queue);
        }
        if (number[4] == 4)
        {
            byte[] fftresult = new byte[4];
            fftresult[0] = 0xA0;
            int value = (int)maxTemp;
            value /= uintThreshold4;
            if (value > 127)
                value = 127;
            Monitor.Enter(queue4);
            queue4.Enqueue(value);
            Monitor.Exit(queue4);
            fftresult[2] = (byte)value;
            if (fftresult[2] >= threshold4)
            {
                fftresult[1] = 0x01;
            }
            else
            {
                fftresult[1] = 0x00;
            }
            fftresult[3] = number[4];
            Monitor.Enter(queue);
            queue.Enqueue(fftresult);
            Monitor.Exit(queue);
        }
    }
}
