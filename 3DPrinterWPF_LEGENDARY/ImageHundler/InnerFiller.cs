using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Numerics;

namespace _3DPrinterWPF_LEGENDARY
{
    internal class InnerFiller
    {
        private readonly byte threshold;
        private readonly byte cellSize;
        private byte[,] colorValuesMap;
        private bool[,] bitmap;

        public byte[,] ColorValuesMap => colorValuesMap;
        public bool[,] Bitmap => bitmap;

        private static Dictionary<int, Vector2[]> paths = new Dictionary<int, Vector2[]>()
        {
            { 5, new Vector2[] { new (3, 1), new (3, 5) } },
            { 6, new Vector2[] { new (3, 1), new (4, 1), new (3, 2), new (3, 5) } },
            { 7, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (3, 2), new (3, 5) } },
            { 8, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (2, 2), new (2, 4), new (3, 5) } },
            { 9, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (2, 2), new (2, 5), new (3, 5) } },
            { 10, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (2, 2), new (2, 4), new (4, 4), new (3, 5) } },
            { 11, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (2, 2), new (2, 4), new (4, 4), new (4, 5), new (3, 5) } },
            { 12, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (2, 2), new (2, 4), new (5, 4), new (4, 5), new (3, 5) } },
            { 13, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (2, 2), new (2, 3), new (4, 3), new (4, 4), new (2, 4), new (2, 5), new (3, 5) } },
            { 14, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (2, 2), new (2, 3), new (4, 3), new (4, 4), new (1, 4), new (2, 5), new (3, 5) } },
            { 15, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (1, 2), new (1, 3), new (4, 3), new (4, 4), new (2, 4), new (2, 5), new (3, 5) } },
            { 16, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (1, 2), new (1, 3), new (4, 3), new (4, 4), new (1, 4), new (2, 5), new (3, 5) } },
            { 17, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (1, 2), new (1, 3), new (5, 3), new (5, 4), new (2, 4), new (2, 5), new (3, 5) } },
            { 18, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (1, 2), new (1, 3), new (5, 3), new (5, 4), new (1, 4), new (2, 5), new (3, 5) } },
            { 19, new Vector2[] { new (3, 1), new (4, 1), new (4, 2), new (1, 2), new (1, 3), new (5, 3), new (5, 4), new (1, 4), new (1, 5), new (3, 5) } },
            { 20, new Vector2[] { new (3, 1), new (4, 1), new (5, 2), new (1, 2), new (1, 3), new (5, 3), new (5, 4), new (1, 4), new (1, 5), new (3, 5) } },
            { 21, new Vector2[] { new (3, 1), new (5, 1), new (5, 2), new (1, 2), new (1, 3), new (5, 3), new (5, 4), new (1, 4), new (1, 5), new (3, 5) } },
            { 22, new Vector2[] { new (2, 1), new (5, 1), new (5, 2), new (1, 2), new (1, 3), new (5, 3), new (5, 4), new (1, 4), new (1, 5), new (3, 5) } },
            { 23, new Vector2[] { new (1, 1), new (5, 1), new (5, 2), new (1, 2), new (1, 3), new (5, 3), new (5, 4), new (1, 4), new (1, 5), new (3, 5) } },
            { 24, new Vector2[] { new (1, 1), new (5, 1), new (5, 2), new (1, 2), new (1, 3), new (5, 3), new (5, 4), new (1, 4), new (1, 5), new (4, 5) } },
            { 25, new Vector2[] { new (1, 1), new (5, 1), new (5, 2), new (1, 2), new (1, 3), new (5, 3), new (5, 4), new (1, 4), new (1, 5), new (5, 5) } }
        };

        public InnerFiller(BitmapSource image, byte cellSize, byte threshold)
        {
            
            this.cellSize = cellSize;
            this.threshold = threshold;

            colorValuesMap = QuantizeGrayScale(MyConverter.GetGrayScalePixels(image));
            bitmap = GetBitmap();
        }

        //public byte[,] DrawTrajectoriesForCells()
        //{

        //    byte[,] OutPutImage = new byte[image.Width, image.Height];

        //    using (Graphics graphics = Graphics.FromImage(OutPutImage))
        //    {
        //        for (int i = 0; i < bitmap.GetLength(0); i++)
        //        {
        //            for (int j = 0; j < bitmap.GetLength(1); j++)
        //            {
        //                if (bitmap[i, j] == 1)
        //                {
        //                    var stdPathKey = getKey(i, j);

        //                    Point[] stdPath = paths[stdPathKey];
        //                    int smallCellSize = cellSize / 5;
        //                    int coefficient = 0;
        //                    int xShift = i * cellSize;
        //                    int yShift = j * cellSize;

        //                    for (int k = 0; k < stdPath.Length - 1; k++)
        //                    {
        //                        Point from = new Point(stdPath[k].Y * smallCellSize - coefficient + xShift, stdPath[k].X * smallCellSize - coefficient + yShift);
        //                        Point to = new Point(stdPath[k + 1].Y * smallCellSize - coefficient + xShift, stdPath[k + 1].X * smallCellSize - coefficient + yShift);
        //                        Color color = Color.Black;
        //                        graphics.DrawLine(new Pen(color), from, to);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return OutPutImage;
        //}

        //Futures 
        public ImageSource DrawTrajectoriesForCells()
        {
            int width = bitmap.GetLength(1) * cellSize;
            int height = bitmap.GetLength(0) * cellSize;

            DrawingVisual drawingVisual = new DrawingVisual();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                for (int i = 0; i < bitmap.GetLength(0); i++)
                {
                    for (int j = 0; j < bitmap.GetLength(1); j++)
                    {
                        if (bitmap[i, j] == true)
                        {
                            var stdPathKey = getKey(i, j);

                            Vector2[] stdPath = paths[stdPathKey];
                            int smallCellSize = cellSize / 5;
                            int coefficient = 0;
                            int xShift = j * cellSize;
                            int yShift = i * cellSize;

                            for (int k = 0; k < stdPath.Length - 1; k++)
                            {
                                Vector2 from = new Vector2(stdPath[k].Y * smallCellSize - coefficient + xShift, stdPath[k].X * smallCellSize - coefficient + yShift);
                                Vector2 to = new Vector2(stdPath[k + 1].Y * smallCellSize - coefficient + xShift, stdPath[k + 1].X * smallCellSize - coefficient + yShift);

                                SolidColorBrush brush = Brushes.Black;
                                Pen pen = new Pen(brush, 1);

                                drawingContext.DrawLine(pen, new Point(from.X, from.Y), new Point(to.X, to.Y));
                            }
                        }
                    }
                }
            }

            RenderTargetBitmap renderedBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            renderedBitmap.Render(drawingVisual);

            return renderedBitmap;
        }

        private byte[,] QuantizeGrayScale(byte[,] grayPixels)
        {
            int width = grayPixels.GetLength(0);
            int height = grayPixels.GetLength(1);

            int quantizedWidth = width / cellSize;
            int quantizedHeight = height / cellSize;

            byte[,] quantizedPixels = new byte[quantizedWidth, quantizedHeight];

            for (int x = 0; x < quantizedWidth; x++)
            {
                for (int y = 0; y < quantizedHeight; y++)
                {
                    int startX = x * cellSize;
                    int startY = y * cellSize;

                    int averageValue = 0;

                    for (int i = startX; i < startX + cellSize; i++)
                    {
                        for (int j = startY; j < startY + cellSize; j++)
                        {
                            averageValue += grayPixels[i, j];
                        }
                    }
                    
                    averageValue /= cellSize * cellSize;
                    quantizedPixels[x, y] = (byte)averageValue;
                }
            }

            return quantizedPixels;
        }


        private bool[,] GetBitmap()
        {
            int bitmapWidth = colorValuesMap.GetLength(0);
            int bitmapHeight = colorValuesMap.GetLength(1);
            bool[,] bitmap = new bool[bitmapWidth, bitmapHeight];

            for (int i = 0; i < bitmapWidth; i++)
            {
                for (int j = 0; j < bitmapHeight; j++)
                {
                    if (colorValuesMap[i, j] < threshold)
                    {
                        bitmap[i, j] = true;
                    }
                }
            }

            return bitmap;
        }
        /// <summary>
        /// Получение нужного пути из библиотеки путей для Сектора 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int getKey(int x, int y)
        {
            byte meanColor = colorValuesMap[x, y];
            double density = 1 - (meanColor / 255.0);
            int stdPathKey = paths.Keys.ToArray()[0];

            foreach (int key in paths.Keys)
            {
                if ((Math.Abs((key / 25.0) - density) < Math.Abs((stdPathKey / 25.0) - density)))
                {
                    stdPathKey = key;
                }
            }
            return stdPathKey;
        }

        //private static int GetAverageValue(Image img, int startX, int startY, int cellSize)
        //{
        //    double sum = 0;
        //    int count = 0;

        //    for (int x = startX; x < startX + cellSize; x++)
        //    {
        //        for (int y = startY; y < startY + cellSize; y++)
        //        {
        //            Color pixelColor = ((Bitmap)img).GetPixel(x, y);
        //            sum += (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0;
        //            count++;
        //        }
        //    }

        //    return (int)(sum / count);
        //}





        //Оптимизация обработки изображения (требует доработки)


        public List<Segment> GetSegmentsList()
        {
            List<Segment> segmentsList = new List<Segment>();
            int i = 0;

            while (i < bitmap.GetLength(0))
            {
                int j = 0;

                while (j < bitmap.GetLength(1))
                {
                    if (bitmap[i, j])
                    {
                        int start = j;
                        int k = j + 1;

                        while (k < bitmap.GetLength(1) && bitmap[i, k])
                        {
                            k++;
                        }

                        int stop = k - 1;
                        segmentsList.Add(new Segment(i, start, stop));
                        j = k + 1;
                    }
                    else
                    {
                        j++;
                    }
                }

                i++;
            }

            return segmentsList;
        }
        public static Tuple<Segment, float> NextSegmentWithDistanceBetweenSegments(Segment firstSegment, Segment secondSegment)
        {
            if (firstSegment.Row != secondSegment.Row)
            {
                if (Math.Abs(secondSegment.Stop - firstSegment.Stop) < Math.Abs(secondSegment.Stop - firstSegment.Start))
                {
                    secondSegment = new Segment(secondSegment.Row, secondSegment.Stop, secondSegment.Start);
                }
            }

            float distance = (float)Math.Pow(firstSegment.Row - secondSegment.Row, 2) +
                           (float)Math.Pow(firstSegment.Stop - secondSegment.Start, 2);

            return Tuple.Create(secondSegment, distance);
        }
        public static Segment? GetClosestSegment(Segment currentSegment, List<Segment> segmentsList)
        {
            if (segmentsList.Count > 0)
            {
                Segment closestSegment = segmentsList[0];
                float closestDist;
                Tuple<Segment, float> result = NextSegmentWithDistanceBetweenSegments(currentSegment, closestSegment);
                closestSegment = result.Item1;
                closestDist = result.Item2;

                for (int idx = 1; idx < segmentsList.Count; idx++)
                {
                    Segment segment = segmentsList[idx];
                    result = NextSegmentWithDistanceBetweenSegments(currentSegment, segment);
                    segment = result.Item1;
                    float dist = result.Item2;

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestSegment = segment;
                    }
                }

                return closestSegment;
            }
            else return null;
        }
        public List<Segment> GetPathForTraversingGrid()
        {
            List<Segment> path = new List<Segment>();
            List<Segment> segmentsList = GetSegmentsList();
            List<Segment> segmentsListCopy = new List<Segment>(segmentsList);
            Segment segment = segmentsListCopy[0];

            while (segmentsListCopy.Count > 0)
            {
                path.Add(segment);
                segmentsListCopy.Remove(segment);

                if (segmentsListCopy.Count == 0)
                    break;

                Segment? newSegmentToAdd = GetClosestSegment(segment, segmentsListCopy);

                if (newSegmentToAdd != null)
                {
                    segment = newSegmentToAdd;

                    // Удаляем дублирующиеся сегменты
                    segmentsListCopy.RemoveAll(s => s.Row == segment.Row && s.Start == segment.Stop && s.Stop == segment.Start);
                }
                else
                {
                    break;
                }
            }

            return path;
        }
        public List<List<Vector2>> GetPrintingPath()
        {
            var printingPath = new List<List<Vector2>>();
            List<Segment> pathForTraversingGrid = GetPathForTraversingGrid();

            foreach (Segment continuousPathSegment in pathForTraversingGrid)
            {
                int direction = continuousPathSegment.Start < continuousPathSegment.Stop ? 1 : -1;
                var printingPathForCps = new List<Vector2>();

                for (int i = continuousPathSegment.Start; i != continuousPathSegment.Stop + direction; i += direction)
                {
                    int x = continuousPathSegment.Row;
                    int y = i;

                    Vector2[] stdPath;
                    if (colorValuesMap != null)
                    {
                        byte meanColor = colorValuesMap[x, y];
                        double density = 1 - meanColor / 255.0;
                        int stdPathKey = paths.Keys.First();

                        foreach (int key in paths.Keys)
                        {
                            if (Math.Abs(key / 25.0 - density) < Math.Abs(stdPathKey / 25.0 - density))
                            {
                                stdPathKey = key;
                            }
                        }

                        stdPath = paths[stdPathKey];
                    }
                    else
                    {
                        stdPath = paths[25];
                    }

                    int small_cell_size = cellSize / 5;
                    int coefficient = 0;
                    float x_shift = x * cellSize;
                    float y_shift = y * cellSize;

                    var cellPoints = new List<Vector2>();
                    for(int l = 0; l < stdPath.Length; l++)
                    {
                        var pointToMoveX = stdPath[l].X * small_cell_size - coefficient + x_shift;
                        var pointToMoveY = stdPath[l].Y * small_cell_size - coefficient + y_shift;
                        cellPoints.Add(new Vector2((int)pointToMoveX,(int)pointToMoveY));
                    }

                    if(direction == -1)
                    {
                        cellPoints.Reverse();
                    }
                    printingPathForCps.AddRange(cellPoints);
                }

                printingPath.Add(printingPathForCps);
            }

            return printingPath;
        }

        public List<List<Vector2>> GetPrintingPathWithoutPointsOutOfBounds(List<Vector2> bounds)
        {
            var pathes = GetPrintingPath();
            var newPathes = new List<List<Vector2>>();
            foreach(var path in pathes)
            {
                newPathes.Add(BurrHandler.RemoveOutsidePoints(bounds,path));
            }
            return newPathes;
        }

        //******************************************************************

        public List<List<Vector2>> SimpleWalkAround()
        {
            var thePath = new List<Vector2>();


            for (int i = 0; i < bitmap.GetLength(0); i++)
            {
                var linePath = new List<Vector2>();
                for (int j = 0; j < bitmap.GetLength(1); j++)
                {
                    if (bitmap[i, j] == true)
                    {
                        var stdPathKey = getKey(i, j);
                        Vector2[] stdPath = paths[stdPathKey];

                        int smallCellSize = cellSize / 5;
                        int coefficient = 0;
                        int xShift = i * cellSize;
                        int yShift = j * cellSize;

                        var newPoints = new List<Vector2>();
                        foreach (var point in stdPath)
                        {
                            var pointToMoveX = point.X * smallCellSize - coefficient + xShift;
                            var pointToMoveY = point.Y * smallCellSize - coefficient + yShift;
                            newPoints.Add(new Vector2(pointToMoveX, pointToMoveY));
                        }

                        linePath.AddRange(newPoints);
                    }
                }
                if (i % 2 != 0) linePath.Reverse();

                thePath.AddRange(linePath);
            }

            return new List<List<Vector2>> { thePath };

        }


    }

    public class Segment
    {
        public int Row { get; set; }
        public int Start { get; set; }
        public int Stop { get; set; }

        public Segment(int row, int start, int stop)
        {
            Row = row;
            Start = start;
            Stop = stop;
        }
    }
}