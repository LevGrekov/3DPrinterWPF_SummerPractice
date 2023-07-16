using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using _3DPrinterWPF_LEGENDARY.Paint;
using Microsoft.Win32;

namespace _3DPrinterWPF_LEGENDARY
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private const byte cell_size = 5;
        private ImageSource mainImage;
        private List<List<Vector2>>? InnerSegments;
        private List<Vector2>? Border;

        private PaintCanvas paintCanvas;
        public MainWindow()
        {
            InitializeComponent();
            paintCanvas = new PaintCanvas(canvas);

        }

        //Для Получения Границы
        private void SetBorder(BitmapSource im)
        {
            string algorithm = AlgoList.Text; ;
            BorderFinder? BF;
            switch (algorithm)
            {
                case "Спуск к границе (8 клеточный)":
                    BF = new BorderFinder(im, Algorith.ProcessPixelLikeOskarSaid);
                    break;
                case "Спуск к границе (4 клеточный)":
                    BF = new BorderFinder(im, Algorith.ProcessPixelLikeOskarSaidNewVersion);
                    break;
                case "Закраска Белых":
                    BF = new BorderFinder(im, Algorith.ProcessPixelLikeInPaint);
                    break;
                default:
                    BF = new BorderFinder(im, Algorith.ProcessPixelLikeOskarSaidNewVersion);
                    break;
            }
            Border = BF.ListPoints;

            pictureBox5.Source = MyConverter.ConvertBoolArrayToImageSource(BF.BorderPoints);

        }

        //Для Получения Внутрянки
        private void SetInnerSpace(BitmapSource im)
        {
            mainImage = im;

            byte threshold = (byte)bitmapSlider.Value;
            InnerFiller innerFiller = new InnerFiller(im, cell_size, threshold);

            //Получение траектории движения для 3D принтера
            //InnerSegments = innerFiller.GetPrintingPath();
            if (BorderChoper.IsChecked == true)
            {
                InnerSegments = innerFiller.GetPrintingPathWithoutPointsOutOfBounds(Border);

            }
            else
            {
                InnerSegments = innerFiller.GetPrintingPath();
            }
            //Инфографика
            PictureBox1.Source = im;
            pictureBox2.Source = MyConverter.ConvertByteArrayToImageSource(innerFiller.ColorValuesMap, cell_size);
            pictureBox3.Source = MyConverter.ConvertBoolArrayToImageSource(innerFiller.Bitmap, cell_size);
            pictureBox6.Source = innerFiller.DrawTrajectoriesForCells();
            GetPointsSet();

        }

        private void GetPointsSet()
        {
            if (mainImage is BitmapSource bitmap)
            {
                byte[,] NewBitMap = new byte[bitmap.PixelWidth + 1, bitmap.PixelHeight + 1];
                for (int i = 0; i < NewBitMap.GetLength(0); i++)
                {
                    for (int j = 0; j < NewBitMap.GetLength(1); j++)
                    {
                        NewBitMap[i, j] = (byte)255;
                    }
                }

                foreach (var segment in InnerSegments)
                {
                    foreach (var point in segment)
                    {
                        NewBitMap[(int)point.X, (int)point.Y] = (byte)100;
                    }
                }
                foreach (var pixel in Border)
                {
                    NewBitMap[(int)pixel.X, (int)pixel.Y] = (byte)0;
                }
                pictureBox4.Source = MyConverter.ConvertByteArrayToImageSource(NewBitMap);

            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainImage != null)
            {
                SetInnerSpace(mainImage as BitmapSource);
                TextBoxBitmapValue.Text = ((int)(bitmapSlider.Value)).ToString();
            }
            else return;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;

                ImageSource imageSource = new BitmapImage(new Uri(imagePath));
                mainImage = imageSource;

                if (mainImage is BitmapSource bitmapSource)
                {
                    SetBorder(bitmapSource);
                    SetInnerSpace(bitmapSource);
                    bitmapSlider.IsEnabled = true;
                }
            }
        }

        private void GCodeGeneratorB_Click(object sender, RoutedEventArgs e)
        {
            if (Border != null && InnerSegments != null)
            {
                string message = "";
                if (mainImage is BitmapSource bitmapSource)
                {
                    int width = bitmapSource.PixelWidth;
                    int height = bitmapSource.PixelHeight;
                    var GCG = new NewGCodeGenerator(width, height, Border, InnerSegments);
                    GCG.Generate();
                    // Используйте значения width и height
                }

                message = "Успех";
                MessageBox.Show(message);
            }
            else
            {
                MessageBox.Show("Не загружены данные!");
            }
        }
        private void BorderChoper_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mainImage != null && mainImage is BitmapSource bms)
            {
                SetInnerSpace(bms);
            }
        }

        private void BorderChoper_Checked(object sender, RoutedEventArgs e)
        {
            if (mainImage != null)
            {
                SetInnerSpace(mainImage as BitmapSource);
                TextBoxBitmapValue.Text = ((int)(bitmapSlider.Value)).ToString();
            }
            else return;
        }

        //********************************2Tab*************************************************
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ShapeSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (paintCanvas != null)
            {
                paintCanvas.Shape = selectedItem ?? "Circle";
            }
        }

        //private void functionGeneratorBox_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    // Получаем текущий масштаб изображения
        //    double currentScale = functionGeneratorBox.LayoutTransform.Value.M11;

        //    // Определяем новый масштаб с учетом направления прокрутки колеса мыши
        //    double scaleChange = e.Delta > 0 ? 1.1 : 0.9;
        //    double newScale = currentScale * scaleChange;

        //    // Устанавливаем новый масштаб изображения
        //    ScaleTransform scaleTransform = new ScaleTransform(newScale, newScale);
        //    functionGeneratorBox.LayoutTransform = scaleTransform;

        //    e.Handled = true;
        //}

        //private void functionGeneratorBox_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        //{
        //    // Получаем текущий масштаб изображения
        //    double currentScale = functionGeneratorBox.LayoutTransform.Value.M11;

        //    // Определяем новый масштаб на основе изменения позиции при перемещении
        //    double scaleChange = e.DeltaManipulation.Scale.X;
        //    double newScale = currentScale * scaleChange;

        //    // Устанавливаем новый масштаб изображения
        //    ScaleTransform scaleTransform = new ScaleTransform(newScale, newScale);
        //    functionGeneratorBox.LayoutTransform = scaleTransform;

        //    e.Handled = true;
        //}

        private void GenerateFunctionB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string function = FunctionTextBox.Text;
                Brush brush;
                if (RadialCheckBox.IsChecked ?? false)
                {
                    brush = FunctionGenerator.CreateRadialBrush(function);
                }
                else
                {
                    brush = FunctionGenerator.CreateBrush(function);
                }
                if (paintCanvas != null)
                {
                    paintCanvas.Brush = brush;
                }
            }
            catch
            {
                MessageBox.Show("Функция задана неверно или не может быть отображена");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => paintCanvas.ClearCanvas();

        private void OutLoadImageFromImage(object sender, RoutedEventArgs e)
        {
            mainImage = paintCanvas.GetCanvasImageSource();
            if (mainImage is BitmapSource bitmapSource)
            {
                SetBorder(bitmapSource);
                SetInnerSpace(bitmapSource);
                bitmapSlider.IsEnabled = true;
            }
        }
    }

}
