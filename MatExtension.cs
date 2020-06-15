
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;


namespace 相机标定
{



    /// <summary>

    /// 通过指针操作内存，修改or获取Mat元素值

    /// </summary>

    public static class MatExtension

    {

        /// <summary>

        /// 获取Mat的元素值

        /// </summary>

        /// <param name="mat">需操作的Mat</param>

        /// <param name="row">元素行</param>

        /// <param name="col">元素列</param>

        /// <returns></returns>

        public static dynamic GetValue(this Mat mat, int row, int col)

        {

            var value = CreateElement(mat.Depth);

            Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);

            return value[0];

        }

        /// <summary>

        /// 修改Mat的元素值

        /// </summary>

        /// <param name="mat">需操作的Mat</param>

        /// <param name="row">元素行</param>

        /// <param name="col">元素列</param>

        /// <param name="value">修改值</param>

        public static void SetValue(this Mat mat, int row, int col, dynamic value)

        {

            var target = CreateElement(mat.Depth, value);

            Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);

        }

        /// <summary>

        /// 根据Mat的类型，动态解析传入数据的类型

        /// </summary>

        /// <param name="depthType"></param>

        /// <param name="value"></param>

        /// <returns></returns>

        private static dynamic CreateElement(DepthType depthType, dynamic value)

        {

            var element = CreateElement(depthType);

            element[0] = value;

            return element;

        }

        /// <summary>

        /// 获取Mat元素的类型

        /// </summary>

        /// <param name="depthType"></param>

        /// <returns></returns>

        private static dynamic CreateElement(DepthType depthType)

        {

            if (depthType == DepthType.Cv8S)

            {

                return new sbyte[1];

            }

            if (depthType == DepthType.Cv8U)

            {

                return new byte[1];

            }

            if (depthType == DepthType.Cv16S)

            {

                return new short[1];

            }

            if (depthType == DepthType.Cv16U)

            {

                return new ushort[1];

            }

            if (depthType == DepthType.Cv32S)

            {

                return new int[1];

            }

            if (depthType == DepthType.Cv32F)

            {

                return new float[1];

            }

            if (depthType == DepthType.Cv64F)

            {

                return new double[1];

            }

            return new float[1];

        }

    }



}
