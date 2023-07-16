using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Runtime.InteropServices;

namespace _3DPrinterWPF_LEGENDARY
{
    internal class FunctionGenerator
    {
        public static ImageSource CreateMandelbrotSet(int width, int height, int maxIterations, double zoom, double offsetX, double offsetY)
        {
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            double aspectRatio = (double)width / height;
            double minX = -2.5 * zoom;
            double maxX = 1.5 * zoom;
            double minY = -1.5 * zoom / aspectRatio;
            double maxY = 1.5 * zoom / aspectRatio;

            byte[] pixels = new byte[width * height * 4];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double a = minX + (maxX - minX) * x / width + offsetX;
                    double b = minY + (maxY - minY) * y / height + offsetY;

                    double ca = a;
                    double cb = b;

                    int iterations = 0;

                    while (iterations < maxIterations && (a * a + b * b) < 4)
                    {
                        double tempA = a * a - b * b + ca;
                        b = 2 * a * b + cb;
                        a = tempA;

                        iterations++;
                    }

                    byte colorValue = (byte)(iterations * 255 / maxIterations);
                    int position = 4 * (y * width + x);

                    pixels[position] = colorValue; // Blue
                    pixels[position + 1] = colorValue; // Green
                    pixels[position + 2] = colorValue; // Red
                    pixels[position + 3] = 255; // Alpha
                }
            }

            bitmap.Lock();
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
            bitmap.Unlock();

            return bitmap;
        }
        public static Brush CreateBrush(string function)
        {
            int width = 400;
            int height = 400;
            string[] postfixFunction = MathParser.TransformInfixToPostfixNotation(function);

            // Создаем новый WriteableBitmap с заданными размерами
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            // Вычисляем и устанавливаем цвет каждого пикселя в изображении
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    decimal value = MathParser.Evaluate(postfixFunction, y);

                    // Преобразуем значение в цвет пикселя
                    byte colorValue = (byte)((value * 255) % 255);
                    byte[] colorBytes = { colorValue, colorValue, colorValue, 255 };

                    // Вычисляем позицию пикселя в буфере изображения
                    int position = 4 * (y * width + x);

                    // Устанавливаем цвет пикселя в буфере изображения
                    Marshal.Copy(colorBytes, 0, bitmap.BackBuffer + position, 4);
                }
            }

            // Обновляем изображение
            bitmap.Lock();
            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bitmap.Unlock();

            // Создаем и возвращаем Brush на основе полученного WriteableBitmap
            ImageBrush imageBrush = new ImageBrush(bitmap);
            return imageBrush;
        }

        public static Brush CreateRadialBrush(string function)
        {
            int width = 400;
            int height = 400;
            string[] postfixFunction = MathParser.TransformInfixToPostfixNotation(function);

            // Создаем новый WriteableBitmap с заданными размерами и форматом пикселей Gray8
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);

            // Вычисляем радиальные координаты
            float centerX = width / 2.0f;
            float centerY = height / 2.0f;
            float[,] radii = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float radius = (float)Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    radii[x, y] = radius;
                }
            }

            // Вычисляем и устанавливаем цвет каждого пикселя в изображении
            byte[] pixelData = new byte[width * height];

            for (int i = 0; i < width; i++)
            {
                for (int y = 0; y < height; y++)
                {
                    decimal value = MathParser.Evaluate(postfixFunction, (decimal)radii[i, y]);

                    // Преобразуем значение в цвет пикселя
                    byte colorValue = (byte)(value * 255);
                    pixelData[y * width + i] = colorValue;
                }
            }

            // Заполняем буфер изображения пиксельными данными
            Int32Rect imageRect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(imageRect, pixelData, width, 0);

            // Создаем и возвращаем Brush на основе полученного WriteableBitmap
            ImageBrush imageBrush = new ImageBrush(bitmap);
            return imageBrush;
        }
    }
}
