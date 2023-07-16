using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3DPrinterWPF_LEGENDARY
{
    internal static class BurrHandler
    {

        private static bool IsPointInsideBoundary(Vector2 point, List<Vector2> boundary)
        {
            int intersections = 0; // Счетчик пересечений
            for (int i = 0; i < boundary.Count; i++)
            {
                Vector2 BorderPointA = boundary[i]; // Начальная точка текущего отрезка границы
                Vector2 BorderPointB = boundary[(i + 1) % boundary.Count]; // Конечная точка текущего отрезка границы

                if ((BorderPointA.Y > point.Y) == (BorderPointB.Y > point.Y)) continue;
                // Проверяем, пересекает ли точка вертикальный луч, выходящий из нее
                if (point.X < (BorderPointB.X - BorderPointA.X) * (point.Y - BorderPointA.Y) / (BorderPointB.Y - BorderPointA.Y) + BorderPointA.X)
                {
                    intersections++;
                }
            }
            return intersections % 2 == 1; 
        }

        public static List<Vector2> RemoveOutsidePoints(List<Vector2> boundary, List<Vector2> insidePoints)
        {
            List<Vector2> insideBoundaryPoints = new List<Vector2>();
            foreach (Vector2 point in insidePoints)
            {
                if (IsPointInsideBoundary(point, boundary))
                {
                    insideBoundaryPoints.Add(point);
                }
            }
            return insideBoundaryPoints;
        }
    }
}
