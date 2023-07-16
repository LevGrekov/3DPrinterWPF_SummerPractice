using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _3DPrinterWPF_LEGENDARY
{
    public interface INode
    {
        public string ID { get; }
        public string Name { get; }
    }
    public interface IShapeNode : INode
    {
        public string CreateGeometry();
    }
    public class CircleNode : IShapeNode
    {
        public string ID => "Circle";

        public string Name => "Circle";

        public Image Icon => throw new NotImplementedException();

        public string CreateGeometry()
        {
            return "M 366 241 A 87.5 89 0 0 1 278.5 330 87.5 89 0 0 1 191 241 87.5 89 0 0 1 278.5 152 87.5 89 0 0 1 366 241 Z";
        }
    }
    public class DiamondNode : IShapeNode
    {
        public string ID => "Diamond";

        public string Name => "Diamond";

        public Image Icon => throw new NotImplementedException();

        public string CreateGeometry()
        {
            return "m297.50999 255.24999l88 -105l88 105l-88 105l-88 -105z";
        }
    }
    public class DonutNode : IShapeNode
    {
        public string ID => "Donut";

        public string Name => "Donut";

        public Image Icon => throw new NotImplementedException();

        public string CreateGeometry()
        {
            return "m293.53002 271.16501l0 0c0 -57.43761 54.62127 -104.00001 122 -104.00001l0 0c32.3564 0 63.38759 10.95711 86.26701 30.46089c22.87945 19.50379 35.73299 45.95659 35.73299 73.53912l0 0c0 57.43761 -54.62127 104.00001 -122 104.00001l0 0c-67.37873 0 -122 -46.5624 -122 -104.00001zm61 0l0 0c0 28.7188 27.31065 52 61 52c33.68937 0 61 -23.2812 61 -52c0 -28.7188 -27.31064 -52 -61 -52l0 0c-33.68935 0 -61 23.28121 -61 52z";
        }
    }
    public class HeartNode : IShapeNode
    {
        public string ID => "Heart";

        public string Name => "Heart";

        public Image Icon => throw new NotImplementedException();

        public string CreateGeometry()
        {
            return "M 366 241 A 87.5 89 0 0 1 278.5 330 87.5 89 0 0 1 191 241 87.5 89 0 0 1 278.5 152 87.5 89 0 0 1 366 241 Z";
        }
    }
    public class HexagonNode : IShapeNode
    {
        public string ID => "Hexagon";

        public string Name => "Hexagon";

        public Image Icon => throw new NotImplementedException();

        public string CreateGeometry()
        {
            return "M69.5 0L103.685 21.8212L138.349 42.75L137.87 85.5L138.349 128.25L103.685 149.179L69.5 171L35.315 149.179L0.650978 128.25L1.13 85.5L0.650978 42.75L35.315 21.8212L69.5 0Z";
        }
    }
    public class RectangleNode : IShapeNode
    {
        public string ID => "Rectangle";

        public string Name => "Rectangle";

        public string CreateGeometry()
        {
            return "M 1 1 H 90 V 90 H 1 L 1 1";
        }
    }
}
