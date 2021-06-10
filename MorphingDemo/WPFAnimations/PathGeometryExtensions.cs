using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using SkiaSharp;
using WPFAnimations;

namespace WPFAnimations
{
    enum FlattenOutput
    {
        Lines,
        PolyLines
    }

    static class PathGeometryExtensions
    {
        public static PathGeometry FlattenWithTransform(this PathGeometry pathIn, Func<Point, Point> transform, FlattenOutput flattenOutput)
        {
            var pathOut = new PathGeometry()
            {
                FillRule = pathIn.FillRule
            };

            foreach (var figureIn in pathIn.Figures)
            {
                var firstPoint = new Point();
                var lastPoint = new Point();
                var figureOut = new PathFigure()
                {
                    IsClosed = figureIn.IsClosed,
                    IsFilled = figureIn.IsFilled,
                    StartPoint = transform(figureIn.StartPoint)
                };
                firstPoint = lastPoint = figureIn.StartPoint;

                pathOut.Figures.Add(figureOut);

                if (figureIn.Segments is null)
                {
                    continue;
                }

                foreach (var pathSegmentIn in figureIn.Segments)
                {
                    switch (pathSegmentIn)
                    {
                        case ArcSegment arcSegmentIn:
                        {
                            // TODO:
                        }
                            break;
                        case BezierSegment bezierSegmentIn:
                        {
                            var points = FlattenCubic(lastPoint, bezierSegmentIn.Point1, bezierSegmentIn.Point2, bezierSegmentIn.Point3);

                            switch (flattenOutput)
                            {
                                case FlattenOutput.Lines:
                                {
                                    foreach (var pt in points)
                                    {
                                        var lineSegmentOut = new LineSegment {Point = transform(pt)};
                                        figureOut.Segments?.Add(lineSegmentOut);
                                    }
                                }
                                    break;
                                case FlattenOutput.PolyLines:
                                {
                                    var polyLineSegmentOut = new PolyLineSegment()
                                    {
                                        Points = new AvaloniaList<Point>()
                                    };
                                    foreach (var pt in points)
                                    {
                                        polyLineSegmentOut.Points.Add(transform(pt));
                                    }
                                    figureOut.Segments?.Add(polyLineSegmentOut);
                                }
                                    break;
                            }

                            lastPoint = bezierSegmentIn.Point3;
                        }
                            break;
                        case LineSegment lineSegmentIn:
                        {
                            var points = Interpolate(lastPoint, lineSegmentIn.Point);

                            switch (flattenOutput)
                            {
                                case FlattenOutput.Lines:
                                {
                                    foreach (var pt in points)
                                    {
                                        var lineSegmentOut = new LineSegment {Point = transform(pt)};
                                        figureOut.Segments?.Add(lineSegmentOut);
                                    }
                                }
                                    break;
                                case FlattenOutput.PolyLines:
                                {
                                    var polyLineSegmentOut = new PolyLineSegment()
                                    {
                                        Points = new AvaloniaList<Point>()
                                    };
                                    foreach (var pt in points)
                                    {
                                        polyLineSegmentOut.Points.Add(transform(pt));
                                    }
                                    figureOut.Segments?.Add(polyLineSegmentOut);
                                }
                                    break;
                            }

                            lastPoint = lineSegmentIn.Point;
                        }
                            break;
                        case QuadraticBezierSegment quadraticBezierSegmentIn:
                        {
                            var points = FlattenQuadratic(lastPoint, quadraticBezierSegmentIn.Point1, quadraticBezierSegmentIn.Point2);

                            switch (flattenOutput)
                            {
                                case FlattenOutput.Lines:
                                {
                                    foreach (var pt in points)
                                    {
                                        var lineSegmentOut = new LineSegment {Point = transform(pt)};
                                        figureOut.Segments?.Add(lineSegmentOut);
                                    }
                                }
                                    break;
                                case FlattenOutput.PolyLines:
                                {
                                    var polyLineSegmentOut = new PolyLineSegment()
                                    {
                                        Points = new AvaloniaList<Point>()
                                    };
                                    foreach (var pt in points)
                                    {
                                        polyLineSegmentOut.Points.Add(transform(pt));
                                    }
                                    figureOut.Segments?.Add(polyLineSegmentOut);
                                }
                                    break;
                            }

                            lastPoint = quadraticBezierSegmentIn.Point2;
                        }
                            break;
                        case PolyLineSegment polyLineSegmentIn:
                        {
                            if (polyLineSegmentIn.Points.Count > 0)
                            {
                                for (var i = 0; i < polyLineSegmentIn.Points.Count; i++)
                                {
                                    var points = Interpolate(lastPoint, polyLineSegmentIn.Points[i]);

                                    switch (flattenOutput)
                                    {
                                        case FlattenOutput.Lines:
                                        {
                                            foreach (var pt in points)
                                            {
                                                var lineSegmentOut = new LineSegment {Point = transform(pt)};
                                                figureOut.Segments?.Add(lineSegmentOut);
                                            }
                                        }
                                            break;
                                        case FlattenOutput.PolyLines:
                                        {
                                            var polyLineSegmentOut = new PolyLineSegment()
                                            {
                                                Points = new AvaloniaList<Point>()
                                            };
                                            foreach (var pt in points)
                                            {
                                                polyLineSegmentOut.Points.Add(transform(pt));
                                            }
                                            figureOut.Segments?.Add(polyLineSegmentOut);
                                        }
                                            break;
                                    }

                                    lastPoint = polyLineSegmentIn.Points[i];
                                }
                            }
                        }
                            break;
                        default:
                        {
                            // TODO:
                        }
                            break;
                    }
                }

                if (figureIn.IsClosed)
                {
                    var points = Interpolate(lastPoint, firstPoint);

                    switch (flattenOutput)
                    {
                        case FlattenOutput.Lines:
                        {
                            foreach (var pt in points)
                            {
                                var lineSegmentOut = new LineSegment {Point = transform(pt)};
                                figureOut.Segments?.Add(lineSegmentOut);
                            }
                        }
                            break;
                        case FlattenOutput.PolyLines:
                        {
                            var polyLineSegmentOut = new PolyLineSegment()
                            {
                                Points = new AvaloniaList<Point>()
                            };
                            foreach (var pt in points)
                            {
                                polyLineSegmentOut.Points.Add(transform(pt));
                            }
                            figureOut.Segments?.Add(polyLineSegmentOut);
                        }
                            break;
                    }

                    firstPoint = lastPoint = new Point(0, 0);
                }
            }

            return pathOut;
        }

        static Point[] Interpolate(Point pt0, Point pt1)
        {
            var count = (int)Math.Max(1, Length(pt0, pt1));
            var points = new Point[count];

            for (var i = 0; i < count; i++)
            {
                var t = (i + 1f) / count;
                var x = (1 - t) * pt0.X + t * pt1.X;
                var y = (1 - t) * pt0.Y + t * pt1.Y;
                points[i] = new Point(x, y);
            }

            return points;
        }

        static Point[] FlattenCubic(Point pt0, Point pt1, Point pt2, Point pt3)
        {
            var count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2) + Length(pt2, pt3));
            var points = new Point[count];

            for (int i = 0; i < count; i++)
            {
                var t = (i + 1f) / count;
                var x = (1 - t) * (1 - t) * (1 - t) * pt0.X +
                        3 * t * (1 - t) * (1 - t) * pt1.X +
                        3 * t * t * (1 - t) * pt2.X +
                        t * t * t * pt3.X;
                var y = (1 - t) * (1 - t) * (1 - t) * pt0.Y +
                        3 * t * (1 - t) * (1 - t) * pt1.Y +
                        3 * t * t * (1 - t) * pt2.Y +
                        t * t * t * pt3.Y;
                points[i] = new Point(x, y);
            }

            return points;
        }

        static Point[] FlattenQuadratic(Point pt0, Point pt1, Point pt2)
        {
            var count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2));
            var points = new Point[count];

            for (var i = 0; i < count; i++)
            {
                var t = (i + 1f) / count;
                var x = (1 - t) * (1 - t) * pt0.X + 2 * t * (1 - t) * pt1.X + t * t * pt2.X;
                var y = (1 - t) * (1 - t) * pt0.Y + 2 * t * (1 - t) * pt1.Y + t * t * pt2.Y;
                points[i] = new Point(x, y);
            }

            return points;
        }

        static Point[] FlattenConic(Point pt0, Point pt1, Point pt2, float weight)
        {
            var count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2));
            var points = new Point[count];

            for (int i = 0; i < count; i++)
            {
                var t = (i + 1f) / count;
                var denominator = (1 - t) * (1 - t) + 2 * weight * t * (1 - t) + t * t;
                var x = (1 - t) * (1 - t) * pt0.X + 2 * weight * t * (1 - t) * pt1.X + t * t * pt2.X;
                var y = (1 - t) * (1 - t) * pt0.Y + 2 * weight * t * (1 - t) * pt1.Y + t * t * pt2.Y;
                x /= denominator;
                y /= denominator;
                points[i] = new Point(x, y);
            }

            return points;
        }

        static double Length(Point pt0, Point pt1)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt0.X, 2) + Math.Pow(pt1.Y - pt0.Y, 2));
        }
    }
}