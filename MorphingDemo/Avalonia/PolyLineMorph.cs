using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace Avalonia
{
    public static class PolyLineMorph
    {
        public static List<PathGeometry> ToCache(PathGeometry source, PathGeometry target, double speed, IEasing easing)
        {
            int steps = (int) (1 / speed);
            double p = speed;
            var cache = new List<PathGeometry>(steps);

            //cache.Add(source.ClonePathGeometry());

            for (int i = 0; i < steps; i++)
            {
                var clone = source.ClonePathGeometry();
                var easeP = easing.Ease(p);

                To(clone, target, easeP);

                p += speed;

                cache.Add(clone);
            }

            //cache.Add(target.ClonePathGeometry());

            return cache;
        }

        public static void To(PathGeometry source, PathGeometry target, double progress)
        {
            var sourceFigure = source.Figures[0];
            var sourceSegment = sourceFigure.Segments[0] as PolyLineSegment;

            var targetFigure = target.Figures[0];
            var targetSegment = targetFigure.Segments[0] as PolyLineSegment;

            if (sourceSegment.Points.Count < targetSegment.Points.Count)
            {
                var toAdd = targetSegment.Points.Count - sourceSegment.Points.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    sourceSegment.Points.Add(sourceFigure.StartPoint);
                }
            }
            else if (sourceSegment.Points.Count > targetSegment.Points.Count)
            {
                var toAdd = sourceSegment.Points.Count - targetSegment.Points.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    targetSegment.Points.Add(targetFigure.StartPoint);
                }
            }
            
            sourceFigure.StartPoint = Interpolate(sourceFigure.StartPoint, targetFigure.StartPoint, progress);

            for (int j = 0; j < sourceSegment.Points.Count; j++)
            {
                sourceSegment.Points[j] = Interpolate(sourceSegment.Points[j], targetSegment.Points[j], progress);
            }
        }

        public static double Interpolate(double from, double to, double progress)
        {
            return from + (to - from) * progress;
        }

        public static Point Interpolate(Point from, Point to, double progress)
        {
            var x = Interpolate(from.X,  to.X, progress);
            var y = Interpolate(from.Y, to.Y, progress);
            return new Point(x, y);
        }

    }
}