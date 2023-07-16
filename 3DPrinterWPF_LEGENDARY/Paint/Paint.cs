using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Security.RightsManagement;

namespace _3DPrinterWPF_LEGENDARY.Paint
{
    public class PaintCanvas
    {
        private Point startPoint;
        private Point endPoint;
        private bool isDrawing;
        private Brush theBrush;
        private Canvas canvas;
        private string selectedShape = "Circle";
        private Shape currentShape = new Ellipse();

        public string Shape
        {
            get => selectedShape;
            set => selectedShape = value;
        }
        public Brush Brush
        {
            get => theBrush;
            set
            {
                theBrush = value;
                if (currentShape != null)
                {
                    currentShape.Fill = theBrush;
                }
            }
        }
        public void ClearCanvas() => canvas.Children.Clear();
        public PaintCanvas(Canvas targetCanvas, Brush brush)
        {
            canvas = targetCanvas;
            theBrush = brush;
            isDrawing = false;

            WireUpEvents();
        }

        public PaintCanvas(Canvas targetCanvas) : this(targetCanvas, null)
        {
            theBrush = new SolidColorBrush(Colors.Black);
        }

        private void WireUpEvents()
        {
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            startPoint = e.GetPosition(canvas);
            currentShape = CreateShape();

            if (currentShape != null)
            {
                canvas.Children.Add(currentShape);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && currentShape != null)
            {
                endPoint = e.GetPosition(canvas);
                UpdateShape();
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            endPoint = e.GetPosition(canvas);
            currentShape = null;
        }

        private Shape CreateShape()
        {
            Shape shape;

            switch (selectedShape)
            {
                case "Круг":
                    shape = new Ellipse
                    {
                        Fill = theBrush
                    };
                    break;
                case "Прямоугольник":
                    shape = new Rectangle
                    {
                        Fill = theBrush
                    };
                    break;
                case "Треугольник":
                    shape = new Polygon
                    {
                        Fill = theBrush
                    };
                    break;
                case "Линия":
                    shape = new Line
                    {
                        Stroke = theBrush,
                        StrokeThickness = 2
                    };
                    break;
                case "Овал":
                    shape = new Ellipse
                    {
                        Fill = theBrush,
                        Stretch = Stretch.Fill
                    };
                    break;
                case "Ромб":
                    shape = new Polygon
                    {
                        Fill = theBrush
                    };
                    break;
                case "Пятиугольник":
                    shape = new Polygon
                    {
                        Fill = theBrush
                    };
                    break;
                default:
                    shape = new Ellipse
                    {
                        Fill = theBrush
                    };
                    break;
            }

            return shape;
        }

        private void UpdateShape()
        {
            if (currentShape != null)
            {
                double x = Math.Min(startPoint.X, endPoint.X);
                double y = Math.Min(startPoint.Y, endPoint.Y);
                double width = Math.Abs(startPoint.X - endPoint.X);
                double height = Math.Abs(startPoint.Y - endPoint.Y);

                if (currentShape is Ellipse ellipse)
                {
                    ellipse.Width = width;
                    ellipse.Height = height;
                    Canvas.SetLeft(ellipse, x);
                    Canvas.SetTop(ellipse, y);
                }
                else if (currentShape is Rectangle rectangle)
                {
                    rectangle.Width = width;
                    rectangle.Height = height;
                    Canvas.SetLeft(rectangle, x);
                    Canvas.SetTop(rectangle, y);
                }
                else if (currentShape is Polygon polygon)
                {
                    UpdatePolygonPoints(polygon, x, y, width, height);
                }
                else if (currentShape is Line line)
                {
                    line.X1 = startPoint.X;
                    line.Y1 = startPoint.Y;
                    line.X2 = endPoint.X;
                    line.Y2 = endPoint.Y;
                }
                // Нужно добавить обработку других типов фигур 
            }
        }

        private void UpdatePolygonPoints(Polygon polygon, double x, double y, double width, double height)
        {
            PointCollection points = new PointCollection();

            switch (selectedShape)
            {
                case "Треугольник":
                    points.Add(new Point(x + width / 2, y));
                    points.Add(new Point(x, y + height));
                    points.Add(new Point(x + width, y + height));
                    break;
                case "Ромб":
                    points.Add(new Point(x + width / 2, y));
                    points.Add(new Point(x + width, y + height / 2));
                    points.Add(new Point(x + width / 2, y + height));
                    points.Add(new Point(x, y + height / 2));
                    break;
                case "Пятиугольник":
                    double ratio = 0.2;
                    points.Add(new Point(x + width / 2, y));
                    points.Add(new Point(x + width, y + ratio * height));
                    points.Add(new Point(x + width * (1 - ratio), y + height));
                    points.Add(new Point(x + width * ratio, y + height));
                    points.Add(new Point(x, y + ratio * height));
                    break;
            }

            polygon.Points = points;
        }

        public ImageSource GetCanvasImageSource()
        {

            // Получаем размеры содержимого Canvas
            double contentWidth = canvas.ActualWidth;
            double contentHeight = canvas.ActualHeight;
            int targetWidth = (int)Math.Ceiling(contentWidth) + 2;
            int targetHeight = (int)Math.Ceiling(contentHeight) + 2;
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(targetWidth, targetHeight, 96d, 96d, PixelFormats.Default);

            // Рендерим Canvas на RenderTargetBitmap с смещением в 1 пиксель вправо и вниз
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var brush = new VisualBrush(canvas);
                context.DrawRectangle(brush, null, new Rect(new Point(1, 1), new Point(targetWidth - 1, targetHeight - 1)));
            }
            renderBitmap.Render(visual);

            // Создаем новый CroppedBitmap, обрезанный по одному пикселю с каждой стороны
            var croppedBitmap = new CroppedBitmap(renderBitmap, new Int32Rect(1, 1, targetWidth - 2, targetHeight - 2));

            // Создаем новый WriteableBitmap с размерами, соответствующими обрезанному изображению
            WriteableBitmap outputBitmap = new WriteableBitmap(croppedBitmap.PixelWidth, croppedBitmap.PixelHeight, 96d, 96d, PixelFormats.Pbgra32, null);

            // Копируем пиксели из обрезанного изображения
            croppedBitmap.CopyPixels(new Int32Rect(0, 0, croppedBitmap.PixelWidth, croppedBitmap.PixelHeight), outputBitmap.BackBuffer, outputBitmap.BackBufferStride * croppedBitmap.PixelHeight, outputBitmap.BackBufferStride);


            return croppedBitmap;
        }
    }
}
