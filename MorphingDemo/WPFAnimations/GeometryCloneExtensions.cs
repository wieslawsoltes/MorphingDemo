using System.Linq;
using Avalonia.Media;

namespace WPFAnimations
{
    internal static class GeometryCloneExtensions
    {
        public static PathGeometry ClonePathGeometry(this PathGeometry source)
        {
            return PathGeometry.Parse(source.ToString());
        }
        public static PathFigure ClonePathFigure(this PathFigure source)
        {
            return PathGeometry.Parse(source.ToString()).Figures.First();
        }
    }
}