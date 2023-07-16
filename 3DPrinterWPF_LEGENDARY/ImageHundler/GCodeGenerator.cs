using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;


namespace _3DPrinterWPF_LEGENDARY
{

    enum actionType
    {
        move,
        extrude
    }
    internal class NewGCodeGenerator
    {
        //layer_height – высота слоя печати в миллиметрах(рекомендуемое значение 0.2 мм);
        private const float layerHeight = 0.2f;

        //flow_modifier – модификатор потока экструзии;
        private const float flowModifier = 1f;

        //nozzle_diameter – диаметр сопла экструдера в миллиметрах;
        private const float nozzleDiameter = 0.4f;

        //filament_diameter – диаметр нити пластика в миллиметрах;
        private const float filamentDiameter = 1.75f;

        //offset – смещение в миллиметрах относительно нуля стола печати.
        private const int offset = 20;

        //layers_count – количество слоев печати;
        private const int layersCount = 10;

        //width – максимальное значение координаты X в пиксельной системе Координат
        private int width;

        //height – максимальное значение координаты Y в пиксельной системе координат
        private int height;

        private CultureInfo culture;

        private static string HelpFilePath = "D:\\C#new\\New3DPrinterProgramAgainstMathLab\\New3DPrinterProgramAgainstMathLab\\HelpFile.txt";

        private List<string> HelpFileStrings;

        private static string filePath = "D:\\C#new\\New3DPrinterProgramAgainstMathLab\\New3DPrinterProgramAgainstMathLab\\output.gcode";
        private StreamWriter writer;

        private List<Vector2> borderPoints;
        private List<Vector2> ShiftedBorderPoints;
        private List<(Vector2, actionType, bool)> infillPoints;

        public NewGCodeGenerator(int width, int height, List<Vector2> borderPoints, List<List<Vector2>> infillPoints)
        {
            this.width = width;
            this.height = height;


            this.borderPoints = new List<Vector2>();

            foreach (var borderPoint in borderPoints)
            {
                this.borderPoints.Add(MyConverter.ConverPixelPointToCartesian(borderPoint, width, height, offset));
            }

            this.ShiftedBorderPoints = MyConverter.ShiftBorderPointsByNormal(this.borderPoints, nozzleDiameter);
            this.infillPoints = GetInfillPointsForPrinting(infillPoints);


            culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;

            writer = new StreamWriter(filePath, false);

            using (StreamReader reader = new StreamReader(HelpFilePath))
            {
                HelpFileStrings = new List<string>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    HelpFileStrings.Add(line);
                }
            }

        }

        //Вспомогательные Функции

        /// <summary>
        /// Вспомогательная функция, нужная для перевода Координат Внутренности в Декартовы, а так же для определения типа движения (с экструзией и без) и определения является ли точка граничной
        /// </summary>
        /// <param name="infillPrintingPath"></param>
        /// <returns></returns>
        private List<(Vector2, actionType, bool)> GetInfillPointsForPrinting(List<List<Vector2>> infillPrintingPath)
        {
            var infillPoints = new List<(Vector2, actionType, bool)>();

            foreach (var continuousPath in infillPrintingPath)
            {
                foreach (var point in continuousPath)
                {

                    var newPoint = MyConverter.ConverPixelPointToCartesian(point, width, height, offset);

                    var actiontype = actionType.extrude;
                    bool isPointNearBorder = false;


                    if (continuousPath.IndexOf(point) == 0)
                    {
                        actiontype = actionType.move;
                    }

                    //if (continuousPath.IndexOf(point) == 0 || continuousPath.IndexOf(point) == continuousPath.Count - 1)
                    //{
                    //    isPointNearBorder = true;
                    //}

                    infillPoints.Add((newPoint, actiontype, isPointNearBorder));
                }
            }

            return infillPoints;
        }


        /// <summary>
        /// Формула Для получения значения Экструзии для Пути 
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        private float getExtrusion(float dist) =>
            (4 * layerHeight * flowModifier * nozzleDiameter * dist) /
            ((float)Math.PI * filamentDiameter * filamentDiameter);

        /// <summary>
        /// Функция для получения значения Экструзии для точки, в зависимости от расстояния для предыдущей. Перегруз для удобстава
        /// </summary>
        /// <param name="currPoint"></param>
        /// <param name="prevPoint"></param>
        /// <returns></returns>
        private float getExtrusion(Vector2 currPoint, Vector2 prevPoint)
        {
            float dx = prevPoint.X - currPoint.X;
            float dy = prevPoint.Y - currPoint.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            return ((4 * layerHeight * flowModifier * nozzleDiameter * dist) / ((float)Math.PI * filamentDiameter * filamentDiameter));
        }

        private float Distance(Vector2 p1, Vector2 p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        //Основные функции

        private void GenerateBorderGcode(List<Vector2> Border, bool Outer)
        {
            if (Outer) writer.WriteLine(";TYPE:External perimeter");
            else writer.WriteLine(";TYPE:Perimeter");

            for (int idx = 0; idx < Border.Count; idx++)
            {
                Vector2 point = Border[idx];

                if (idx == 0)
                {
                    writer.WriteLine($"G1 X{point.X.ToString(culture)} Y{point.Y.ToString(culture)} F9000");
                    writer.WriteLine("G1 F1200");
                }
                else
                {
                    Vector2 prevPoint = Border[idx - 1];

                    float E = getExtrusion(point, prevPoint);

                    writer.WriteLine($"G1 X{point.X.ToString(culture)} Y{point.Y.ToString(culture)} E{E.ToString(culture)}");
                }
            }
            //Для Замыкания Контура
            var firstPoint = Border[0];
            var lastPoint = Border[^1];
            float Eclose = getExtrusion(firstPoint, lastPoint);
            writer.WriteLine($"G1 X{firstPoint.X.ToString(culture)} Y{firstPoint.Y.ToString(culture)} E{Eclose.ToString(culture)}");

        }

        private void GenerateInfillGcode()
        {
            writer.WriteLine(";TYPE:Internal infill");
            for (int i = 0; i < infillPoints.Count; i++)
            {
                var point = infillPoints[i].Item1;
                var actionType = infillPoints[i].Item2;
                var isNearBorder = infillPoints[i].Item3;

                if (isNearBorder)
                {
                    var newPoint = borderPoints[0];
                    var dist = Distance(point, newPoint);
                    foreach (var borderPoint in borderPoints)
                    {
                        var tempDist = Distance(borderPoint, newPoint);
                        if (tempDist < dist)
                        {
                            dist = tempDist;
                            newPoint = borderPoint;
                        }
                    }
                    point = newPoint;
                }

                if (actionType == actionType.move)
                {
                    writer.WriteLine($"G1 X{point.X.ToString(culture)} Y{point.Y.ToString(culture)} F9000");
                    writer.WriteLine("G1 F1200");
                }
                else
                {
                    var prevPoint = infillPoints[i - 1].Item1;
                    float E = getExtrusion(point, prevPoint);
                    writer.WriteLine($"G1 X{point.X.ToString(culture)} Y{point.Y.ToString(culture)} E{E.ToString(culture)}");
                }
            }
        }

        public void Generate()
        {
            using (writer)
            {

                int my_code_start_line_index = HelpFileStrings.IndexOf("; my code start");
                int my_code_end_line_index = my_code_start_line_index + 1;

                for (int i = 0; i <= my_code_start_line_index; i++)
                {
                    writer.WriteLine(HelpFileStrings[i]);
                }

                // Начало Генерации Кода
                writer.WriteLine("G1 F1200");

                for (int j = 1; j <= layersCount; j++)
                {
                    float z = layerHeight * j;
                    writer.WriteLine($"G0 Z{z.ToString(culture)}");
                    writer.WriteLine(";LAYER_CHANGE");
                    writer.WriteLine($";{z}");


                    GenerateBorderGcode(ShiftedBorderPoints, true);
                    GenerateBorderGcode(borderPoints, false);
                    GenerateInfillGcode();
                }

                for (int i = my_code_end_line_index; i < HelpFileStrings.Count; i++)
                {
                    writer.WriteLine(HelpFileStrings[i]);
                }
            }
        }
    }
}
