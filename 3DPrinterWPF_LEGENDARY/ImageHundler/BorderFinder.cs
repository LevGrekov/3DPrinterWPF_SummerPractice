using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;

namespace _3DPrinterWPF_LEGENDARY
{
    public enum Algorith
    {
        ProcessPixelLikeOskarSaid,
        ProcessPixelLikeOskarSaidNewVersion,
        ProcessPixelLikeInPaint,
        TeoPavlidisAlgorithm
    }
    internal class BorderFinder
    {
        private byte[,] ImageGrayScale;
        public List<Vector2> ListPoints;
        public bool[,] BorderPoints;

        private byte controlColor = 230;

        public BorderFinder() { }
        public BorderFinder(BitmapSource image, Algorith algorithType)
        {
            ImageGrayScale = MyConverter.GetGrayScalePixels(image);
            bool[,] BorderBitMap = new bool[ImageGrayScale.GetLength(0), ImageGrayScale.GetLength(1)];
            

            switch (algorithType)
            {
                case Algorith.ProcessPixelLikeInPaint:
                    controlColor = 15;
                    ListPoints = ProcessPixelLikeInPaint(new(0, 0)).ToList();
                    break;
                case Algorith.ProcessPixelLikeOskarSaid:
                    controlColor = 230;
                    ListPoints = ProcessPixelLikeOskarSaid();
                    break;
                case Algorith.ProcessPixelLikeOskarSaidNewVersion:
                    controlColor = 230;
                    ListPoints = BorderWalkAroundStepByStep();
                    break;
                case Algorith.TeoPavlidisAlgorithm:
                    controlColor = 230;
                    ListPoints = TeoPavlidisAlgorithm();
                    break;
                default:
                    controlColor = 230;
                    ListPoints = BorderWalkAroundStepByStep();
                    break;
            }

            foreach (Vector2 point in ListPoints)
            {
                BorderBitMap[(int)point.X, (int)point.Y] =  true;
            }

            BorderPoints = BorderBitMap;
            CreateSvg(ListPoints);
        }

        private List<Vector2> BorderWalkAroundStepByStep()
        {
            List<Vector2> borderPoints = new List<Vector2>();

            bool FindNewPoint(Vector2 pixel)
            {
                for (int ni = -1; ni <= 1; ni++)
                {
                    for (int nj = -1; nj <= 1; nj++)
                    {
                        var neighbor = new Vector2(pixel.X + ni, pixel.Y + nj);

                        //Если это та же самая точка или за границами Карты, то не берем в расчет
                        if (IsPixelOutOfBounds(neighbor) || neighbor.Equals(pixel)) continue;

                        //Проверяем является ли точка черным пикселем и является ли она граничной.
                        if (!isWhite(neighbor) && NewCheckBorderAbleness(neighbor,true))
                        {
                            //Если мы находим такую точку, нам нужно вернуться в начало цикла 
                            borderPoints.Add(neighbor);
                            return true;
                        }
                    }
                }

                //Если мы не находим сосезднии клетки, то смотрим по вертикали 
                for (int ni = -1; ni <= 1; ni++)
                {
                    for (int nj = -1; nj <= 1; nj++)
                    {
                        var neighbor = new Vector2(pixel.X + ni, pixel.Y + nj);

                        //Если это та же самая точка или за границами Карты, то не берем в расчет
                        if (IsPixelOutOfBounds(neighbor) || neighbor.Equals(pixel)) continue;

                        //Проверяем является ли точка черным пикселем и является ли она граничной.
                        if (!isWhite(neighbor) && NewCheckBorderAbleness(neighbor,true))
                        {
                            //Если мы находим такую точку, нам нужно вернуться в начало цикла 
                            borderPoints.Add(neighbor);
                            return true;
                        }
                    }
                }
                //Если уж и это не помогает, то конец
                return false;
            }

            var currPoint = new Vector2(ImageGrayScale.GetLength(0) / 2, 0);
            while (isWhite(currPoint))
            {
                currPoint.Y++;
            }
            borderPoints.Add(currPoint);

            while (true)
            {
                //Работаем с последней точкой в списке
                var pixel = borderPoints.Last();
                ImageGrayScale[(int)pixel.X, (int)pixel.Y] = controlColor;

                //Проходим по соседям точки
                if (!FindNewPoint(pixel)) break;
            }
            return borderPoints;
        }

        

        // ******************************** Вспомогательные Функции *************************************************************
        /// <summary>
        /// Пробегается по соседям
        /// </summary>
        /// <param name="point"></param>
        /// <param name="Support">True для просмотра соседий по углам</param>
        /// <returns></returns>
        private bool NewCheckBorderAbleness(Vector2 point,bool Support)
        {
            Vector2[] NeiArr;

            if(Support)
            {
                var TopNeighboor = new Vector2(point.X + 1, point.Y + 1);
                var RightNeighboor = new Vector2(point.X + 1, point.Y - 1);
                var LeftNeighboor = new Vector2(point.X - 1, point.Y + 1);
                var BotNeighboor = new Vector2(point.X - 1, point.Y - 1);
                NeiArr = new Vector2[] { TopNeighboor, RightNeighboor, BotNeighboor, LeftNeighboor };
            }
            else
            {
                var TopNeighboor = new Vector2(point.X, point.Y - 1);
                var RightNeighboor = new Vector2(point.X + 1, point.Y);
                var LeftNeighboor = new Vector2(point.X, point.Y + 1);
                var BotNeighboor = new Vector2(point.X - 1, point.Y);
                NeiArr = new Vector2[] { TopNeighboor, RightNeighboor, BotNeighboor, LeftNeighboor };

            }


            foreach (var Neighboor in NeiArr)
            {
                if (IsPixelOutOfBounds(Neighboor) || Neighboor.Equals(point)) continue;
                if (isWhite(Neighboor) && !isControlColor(Neighboor))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверяет существует ли такая точка (не выходят ли координаты за границу) 
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        private bool IsPixelOutOfBounds(Vector2 pixel)
        {
            int width = ImageGrayScale.GetLength(0);
            int height = ImageGrayScale.GetLength(1);

            return pixel.X < 0 || pixel.X >= width || pixel.Y < 0 || pixel.Y >= height;
        }
        private bool isWhite(Vector2 pixel)
        {
            byte pixelColor = ImageGrayScale[(int)pixel.X, (int)pixel.Y];
            
            if (pixelColor >= 128)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool isControlColor(Vector2 pixel)
        {
            return ImageGrayScale[(int)pixel.X, (int)pixel.Y] == controlColor;
        }


        // ************************************** Прошлые Версии *******************************************************************

        /// <summary>
        /// Функция реализована путём заливки белых пиксилей и поиска граничных точек, т.к точки попаются из стека неравномерно, нарушается путь экструзии
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        private HashSet<Vector2> ProcessPixelLikeInPaint(Vector2 pixel)
        {
            Stack<Vector2> points = new Stack<Vector2>();
            HashSet<Vector2> border = new HashSet<Vector2>();

            void ProcessNeighbor(Vector2 neighbor)
            {
                if (IsPixelOutOfBounds(neighbor))
                {
                    return;
                }

                if (isWhite(neighbor))
                {
                    points.Push(neighbor);
                }
                else
                {
                    if (!isControlColor(neighbor))
                    {
                        border.Add(neighbor);
                    }
                }
            }

            points.Push(pixel);

            while (points.Count > 0)
            {
                pixel = points.Pop();
                ImageGrayScale[(int)pixel.X, (int)pixel.Y] = controlColor;

                var RightPoint = new Vector2(pixel.X + 1, pixel.Y);
                ProcessNeighbor(RightPoint);

                var ToppPoint = new Vector2(pixel.X, pixel.Y - 1);
                ProcessNeighbor(ToppPoint);

                var LeftPoint = new Vector2(pixel.X - 1, pixel.Y);
                ProcessNeighbor(LeftPoint);

                var BottomPoint = new Vector2(pixel.X, pixel.Y + 1);
                ProcessNeighbor(BottomPoint);

            }
            return border;
        }
        /// <summary>
        /// Функция генерирует ровную границу, но неупорядоченную, т.ч для сложных фигур(не круга) в GCode наблюдаются смещения
        /// </summary>
        /// <returns></returns>
        private List<Vector2> ProcessPixelLikeOskarSaid()
        {
            var borderPoints = new List<Vector2>();
            var points = new Stack<Vector2>();

            var currPoint = new Vector2(ImageGrayScale.GetLength(0) / 2, 0);
            while (isWhite(currPoint))
            {
                currPoint.Y++;
            }

            points.Push(currPoint);

            while (points.Count > 0)
            {
                var pixel = points.Pop();
                borderPoints.Add(pixel);
                ImageGrayScale[(int)pixel.X, (int)pixel.Y] = controlColor;


                for (int ni = -1; ni <= 1; ni++)
                {
                    for (int nj = -1; nj <= 1; nj++)
                    {
                        var neighbor = new Vector2(pixel.X + ni, pixel.Y + nj);
                        if (IsPixelOutOfBounds(neighbor) || neighbor.Equals(pixel)) continue;

                        if (!isWhite(neighbor) && CheckBorderableness(neighbor))
                        {
                            points.Push(neighbor);
                            borderPoints.Add(neighbor);
                            break;
                        }
                    }
                }
            }
            return borderPoints;
        }
        private bool CheckBorderableness(Vector2 point)
        {
            for (int ni = -1; ni <= 1; ni++)
            {
                for (int nj = -1; nj <= 1; nj++)
                {
                    var Neighboor = new Vector2(point.X + ni, point.Y + nj);

                    if (IsPixelOutOfBounds(Neighboor) || Neighboor.Equals(point)) continue;
                    if (isWhite(Neighboor) && !isControlColor(Neighboor))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        //***********************************Атавизмы, Рудименты и прочее***********************************************************

        //Функция для чтения точек из JSON 
        /*
        public static List<PointF> ReadJsonPoints(string filePath)
        {
            List<PointF> points = new List<PointF>();

            try
            {
                string json = File.ReadAllText(filePath);
                var jsonPoints = JsonConvert.DeserializeObject<List<List<float>>>(json);

                foreach (var jsonPoint in jsonPoints)
                {
                    float x = jsonPoint[0];
                    float y = jsonPoint[1];

                    points.Add(new PointF(x, y));
                }
                // Добавляем первую точку в конец, чтобы замкнуть контур
                if (points.Count > 0)
                {
                    points.Add(points[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the JSON file: {ex.Message}");
            }

            return points;
        }

        */
        //Функция создает SVG картинку по заданным точкам.

        /// <summary>
        /// Раньше функция нужна была для генерации SVG границы, ныне рудемент 
        /// </summary>
        /// <param name="points"></param>
        private static void CreateSvg(List<Vector2> points)
        {
            // Создаем объект XmlDocument
            XmlDocument doc = new XmlDocument();

            // Создаем корневой элемент svg
            XmlElement svgElement = doc.CreateElement("svg");
            svgElement.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
            svgElement.SetAttribute("version", "1.1");

            // Создаем элемент пути
            XmlElement pathElement = doc.CreateElement("path");

            // Формируем строку команд для рисования пути
            string pathData = "M"; // Начальная команда "M" (moveto)
            foreach (var point in points)
            {
                pathData += $" {point.X},{point.Y}"; // Команда "L" (lineto)
            }

            // Устанавливаем атрибут "d" для определения пути
            pathElement.SetAttribute("d", pathData);

            // Добавляем элемент пути в svg элемент
            svgElement.AppendChild(pathElement);

            // Добавляем svg элемент в документ
            doc.AppendChild(svgElement);

            using (StreamWriter writer = new StreamWriter("D:\\C#new\\New3DPrinterProgramAgainstMathLab\\New3DPrinterProgramAgainstMathLab\\image.svg", false))
            {
                writer.WriteLine(doc.OuterXml);
            }

        }



        //******************************************************Новые Попытки*******************************

        public List<Vector2> TeoPavlidisAlgorithm()
        {

        }
    }
}
