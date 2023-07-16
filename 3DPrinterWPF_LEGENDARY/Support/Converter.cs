using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Numerics;


namespace _3DPrinterWPF_LEGENDARY
{
    public static class MyConverter
    {
        /// <summary>
        /// Функция , для получения из изображения массива пиксилей в серых оттенках
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[,] GetGrayScalePixels(BitmapSource bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            byte[] pixels = new byte[width * height * bitmap.Format.BitsPerPixel / 8];
            bitmap.CopyPixels(pixels, width * bitmap.Format.BitsPerPixel / 8, 0);

            FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Gray8, null, 0);

            byte[] grayPixels = new byte[width * height];
            convertedBitmap.CopyPixels(new Int32Rect(0, 0, width, height), grayPixels, width, 0);

            byte[,] grayPixels2D = new byte[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixelValue = grayPixels[y * width + x];
                    grayPixels2D[y, x] = pixelValue;
                }
            }

            return grayPixels2D;
        }
        /// <summary>
        /// Функция для полуения Картинки из массива байтов
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static ImageSource ConvertByteArrayToImageSource(byte[,] byteArray)
        {
            int width = byteArray.GetLength(1);
            int height = byteArray.GetLength(0);

            // Создание массива пикселей для черно-белого изображения
            byte[] pixels = new byte[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixelValue = byteArray[y, x];
                    pixels[y * width + x] = pixelValue;
                }
            }

            // Создание черно-белого формата пикселей
            PixelFormat format = PixelFormats.Gray8;
            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, width);
        }
        /// <summary>
        /// Для получения картинки из булевого массива
        /// </summary>
        /// <param name="boolArray"></param>
        /// <returns></returns>
        public static ImageSource ConvertBoolArrayToImageSource(bool[,] boolArray)
        {
            int width = boolArray.GetLength(0);
            int height = boolArray.GetLength(1);

            // Создание массива пикселей для черно-белого изображения
            byte[] pixels = new byte[width * height];

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    bool pixelValue = boolArray[y, x];
                    pixels[y * width + x] = pixelValue == true ? (byte)0 : (byte)255 ;
                }
            }

            PixelFormat format = PixelFormats.Gray8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, width) as ImageSource; ;
        }
        /// <summary>
        /// Для маштабирования Картинки
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static ImageSource ConvertByteArrayToImageSource(byte[,] byteArray, byte ScaelCoef)
        {
            int width = byteArray.GetLength(1);
            int height = byteArray.GetLength(0);

            int pixelWidth = width * ScaelCoef;
            int pixelHeight = height * ScaelCoef;

            byte[] pixels = new byte[pixelWidth * pixelHeight];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixelValue = byteArray[y, x];

                    for (int dy = 0; dy < ScaelCoef; dy++)
                    {
                        for (int dx = 0; dx < ScaelCoef; dx++)
                        {
                            int pixelIndex = ((y * ScaelCoef + dy) * pixelWidth) + (x * ScaelCoef + dx);
                            pixels[pixelIndex] = pixelValue;
                        }
                    }
                }
            }

            PixelFormat format = PixelFormats.Gray8;


            return BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, format, null, pixels, pixelWidth) as ImageSource;
        }
        public static ImageSource ConvertBoolArrayToImageSource(bool[,] byteArray, byte ScaelCoef)
        {
            int width = byteArray.GetLength(1);
            int height = byteArray.GetLength(0);

            int pixelWidth = width * ScaelCoef;
            int pixelHeight = height * ScaelCoef;

            byte[] pixels = new byte[pixelWidth * pixelHeight];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixelValue = byteArray[y, x] == true ? (byte)0 : (byte)255;

                    for (int dy = 0; dy < ScaelCoef; dy++)
                    {
                        for (int dx = 0; dx < ScaelCoef; dx++)
                        {
                            int pixelIndex = ((y * ScaelCoef + dy) * pixelWidth) + (x * ScaelCoef + dx);
                            pixels[pixelIndex] = pixelValue;
                        }
                    }
                }
            }

            PixelFormat format = PixelFormats.Gray8;


            return BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, format, null, pixels, pixelWidth) as ImageSource;
        }









        public static Vector2 ConverPixelPointToCartesian(Vector2 pixelPoint, int width, int height, float offset)
        {
            int x = (int)pixelPoint.X;
            int y = (int)pixelPoint.Y;

            float pointX = XConvertToCartesian(x, 0, width, width) / 2 + offset;
            float pointY = YConvertToCartesian(y, 0, height, height) / 2 + offset;

            return new Vector2(pointX, pointY);
        }

        private static float XConvertToCartesian(int x, int xMin, int xMax, int width)
        {
            return xMin + ((xMax - xMin) * x / (float)width);
        }

        private static float YConvertToCartesian(int y, int yMin, int yMax, int height)
        {
            return yMax - ((yMax - yMin) * y / (float)height);
        }


        // Дальше идут функции для Получения дополнительных Границ 


        /// <summary>
        /// Функция для получения отрезков (в виде координат начала и конца отрезка), формирующих границу объекта: 
        /// </summary>
        /// <param name="borderPoints"></param>
        /// <returns></returns>
        public static List<(Vector2, Vector2)> GetBorderSides(List<Vector2> borderPoints)
        {
            List<(Vector2, Vector2)> borderSides = new List<(Vector2, Vector2)>();

            for (int i = 0; i < borderPoints.Count - 1; i++)
            {
                var point1 = borderPoints[i];
                var point2 = borderPoints[i + 1];

                borderSides.Add((point1, point2));
            }

            return borderSides;
        }

        /// <summary>
        /// Функция для получения координат векторов границы (функция не является обязательной, но позволяет удобнее вычислять в дальнейшем  векторы нормали): 
        /// </summary>
        /// <param name="borderSides"></param>
        /// <returns></returns>
        public static List<Vector2> GetBorderSidesVectors(List<(Vector2, Vector2)> borderSides)
        {

            List<Vector2> borderSidesVectors = new List<Vector2>();

            foreach ((var point1, var point2) in borderSides)
            {
                float deltaX = point2.X - point1.X;
                float deltaY = point2.Y - point1.Y;

                borderSidesVectors.Add(new Vector2(deltaX, deltaY));
            }

            return borderSidesVectors;
        }

        /// <summary>
        /// Функция для вычисления векторов нормали к векторам границы:
        /// </summary>
        /// <param name="borderSidesVectors"></param>
        /// <returns></returns>
        public static List<Vector2> GetBorderSidesNormals(List<Vector2> borderSidesVectors)
        {
            List<Vector2> normals = new List<Vector2>();

            foreach (Vector2 borderSideVector in borderSidesVectors)
            {
                float x = borderSideVector.X;
                float y = borderSideVector.Y;
                var normal = new Vector2(y, -x);
                normals.Add(normal);
            }

            return normals;
        }
        /// <summary>
        /// Для смещения точки (𝑥0 , 𝑦0) вдоль вектора нормали на расстояние 𝑘, воспользуемся следующими формулами:
        /// </summary>
        /// <param name="borderSides"></param>
        /// <param name="borderSidesVectors"></param>
        /// <param name="shiftCoefficient"></param>
        /// <returns></returns>
        public static List<(Vector2, Vector2)> ShiftBorderSidesByNormal(List<(Vector2, Vector2)> borderSides, List<Vector2> borderSidesVectors, float shiftCoefficient)
        {
            List<(Vector2, Vector2)> shiftedBorderSides = new List<(Vector2, Vector2)>();
            List<Vector2> normals = GetBorderSidesNormals(borderSidesVectors);

            for (int i = 0; i < borderSides.Count; i++)
            {
                (Vector2 point1, Vector2 point2) = borderSides[i];
                Vector2 normal = normals[i];

                float vectorSize = GetVectorSize(normal);

                Vector2 shiftedPoint1 = new Vector2(point1.X + shiftCoefficient * normal.X / vectorSize, point1.Y + shiftCoefficient * normal.Y / vectorSize);
                Vector2 shiftedPoint2 = new Vector2(point2.X + shiftCoefficient * normal.X / vectorSize, point2.Y + shiftCoefficient * normal.Y / vectorSize);

                shiftedBorderSides.Add((shiftedPoint1, shiftedPoint2));
            }

            return shiftedBorderSides;
        }

        public static float GetVectorSize(Vector2 vector) =>
            (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);

        /// <summary>
        /// итоговая функция для смещения точек границы: 
        /// </summary>
        /// <param name="borderPoints">Список точек границы.</param>
        /// <param name="shiftCoefficient">Коэффициент сдвига.</param>
        /// <returns>Список сдвинутых точек границы.</returns>
        public static List<Vector2> ShiftBorderPointsByNormal(List<Vector2> borderPoints, float shiftCoefficient)
        {

            List<Vector2> shiftedBorderPoints = new List<Vector2>();

            List<(Vector2, Vector2)> borderSides = GetBorderSides(borderPoints);
            List<Vector2> borderSidesVectors = GetBorderSidesVectors(borderSides);

            float crossProductSum = 0;

            for (int i = 0; i < borderSidesVectors.Count - 1; i++)
            {
                Vector2 sideVector = borderSidesVectors[i];
                Vector2 nextSideVector = borderSidesVectors[i + 1];

                crossProductSum += sideVector.X * nextSideVector.Y - sideVector.Y * nextSideVector.X;
            }

            int orientation = crossProductSum >= 0 ? 1 : -1;
            float shiftedShiftCoefficient = shiftCoefficient * orientation;

            List<(Vector2, Vector2)> shiftedBorderSides = ShiftBorderSidesByNormal(borderSides, borderSidesVectors, shiftedShiftCoefficient);

            foreach ((Vector2 point1, Vector2 point2) in shiftedBorderSides)
            {
                shiftedBorderPoints.Add(point1);
                shiftedBorderPoints.Add(point2);
            }

            return shiftedBorderPoints;
        }


    }
}
