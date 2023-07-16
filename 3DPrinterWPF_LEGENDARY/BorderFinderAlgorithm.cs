using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3DPrinterWPF_LEGENDARY
{
    internal class BorderFinderAlgorithm
    {
        byte[,] bitmap;
        public BorderFinderAlgorithm(byte[,] Image, byte threshold)
        {
            this.bitmap = Image;
        }


        //Алгоритм Павлидиса 
        public static List<(int, int)> FindBoundaryPixels(bool[,] bitmap)
        {
            List<(int, int)> Border = new List<(int, int)>();
            int n = bitmap.GetLength(0);
            int m = bitmap.GetLength(1);

            // Находим начальный пиксель s
            bool found = false;
            int startX = 0;
            int startY = 0;

            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = 0; j < m; j++)
                {
                    if (bitmap[i, j])
                    {
                        found = true;
                        startX = i;
                        startY = j;
                        break;
                    }
                }
                if (found)
                    break;
            }
            if (!found)
            {
                Console.WriteLine("Нет чёрных пикселей во входной тесселяции.");
                return Border;
            }

            Border.Add((startX, startY));
            int pX = startX;
            int pY = startY;
            int direction = 0; // 0 - вверх, 1 - вправо, 2 - вниз, 3 - влево

            while (true)
            {
                int p1X, p1Y, p2X, p2Y, p3X, p3Y;

                switch (direction)
                {
                    case 0: // Вверх
                        p1X = pX - 1;
                        p1Y = pY;
                        p2X = pX;
                        p2Y = pY - 1;
                        p3X = pX;
                        p3Y = pY + 1;
                        break;

                    case 1: // Вправо
                        p1X = pX;
                        p1Y = pY - 1;
                        p2X = pX + 1;
                        p2Y = pY;
                        p3X = pX - 1;
                        p3Y = pY;
                        break;

                    case 2: // Вниз
                        p1X = pX + 1;
                        p1Y = pY;
                        p2X = pX;
                        p2Y = pY + 1;
                        p3X = pX;
                        p3Y = pY - 1;
                        break;

                    case 3: // Влево
                        p1X = pX;
                        p1Y = pY + 1;
                        p2X = pX - 1;
                        p2Y = pY;
                        p3X = pX + 1;
                        p3Y = pY;
                        break;

                    default:
                        throw new InvalidOperationException("Некорректное направление.");
                }

                if (IsValidPixel(p1X, p1Y, n, m) && bitmap[p1X, p1Y])
                {
                    Border.Add((p1X, p1Y));
                    pX = p1X;
                    pY = p1Y;
                    direction = (direction + 3) % 4; // Поворот на 90 градусов против часовой стрелки
                }
                else if (IsValidPixel(p2X, p2Y, n, m) && bitmap[p2X, p2Y])
                {
                    Border.Add((p2X, p2Y));
                    pX = p2X;
                    pY = p2Y;
                }
                else if (IsValidPixel(p3X, p3Y, n, m) && bitmap[p3X, p3Y])
                {
                    Border.Add((p3X, p3Y));
                    pX = p3X;
                    pY = p3Y;
                    direction = (direction + 1) % 4; // Поворот на 90 градусов по часовой стрелке
                }
                else if (pX == startX && pY == startY)
                {
                    break; // Завершение цикла повтора
                }
                else
                {
                    direction = (direction + 1) % 4; // Поворот на 90 градусов по часовой стрелке
                }
            }

            return Border;
        }

        private static bool IsValidPixel(int x, int y, int n, int m)
        {
            return x >= 0 && x < n && y >= 0 && y < m;
        }
        //Конец Алгоритма Павлидиса
    }
}
