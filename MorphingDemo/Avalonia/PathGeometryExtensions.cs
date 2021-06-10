﻿using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;

namespace MorphingDemo.Avalonia
{
    public enum FlattenOutput
    {
        Lines,
        PolyLines
    }

    public static class PathGeometryExtensions
    {
        private static Point[] Interpolate(Point pt0, Point pt1)
        {
            var count = (int)Math.Max(1, Length(pt0, pt1));
            var points = new Point[count];

            for (var i = 0; i < count; i++)
            {
                var t = (i + 1d) / count;
                var x = (1 - t) * pt0.X + t * pt1.X;
                var y = (1 - t) * pt0.Y + t * pt1.Y;
                points[i] = new Point(x, y);
            }

            return points;
        }

        private static Point[] FlattenCubic(Point pt0, Point pt1, Point pt2, Point pt3)
        {
            var count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2) + Length(pt2, pt3));
            var points = new Point[count];

            for (int i = 0; i < count; i++)
            {
                var t = (i + 1d) / count;
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

        private static Point[] FlattenQuadratic(Point pt0, Point pt1, Point pt2)
        {
            var count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2));
            var points = new Point[count];

            for (var i = 0; i < count; i++)
            {
                var t = (i + 1d) / count;
                var x = (1 - t) * (1 - t) * pt0.X + 2 * t * (1 - t) * pt1.X + t * t * pt2.X;
                var y = (1 - t) * (1 - t) * pt0.Y + 2 * t * (1 - t) * pt1.Y + t * t * pt2.Y;
                points[i] = new Point(x, y);
            }

            return points;
        }

        private static Point[] FlattenConic(Point pt0, Point pt1, Point pt2, float weight)
        {
            var count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2));
            var points = new Point[count];

            for (int i = 0; i < count; i++)
            {
                var t = (i + 1d) / count;
                var denominator = (1 - t) * (1 - t) + 2 * weight * t * (1 - t) + t * t;
                var x = (1 - t) * (1 - t) * pt0.X + 2 * weight * t * (1 - t) * pt1.X + t * t * pt2.X;
                var y = (1 - t) * (1 - t) * pt0.Y + 2 * weight * t * (1 - t) * pt1.Y + t * t * pt2.Y;
                x /= denominator;
                y /= denominator;
                points[i] = new Point(x, y);
            }

            return points;
        }

        private static double Length(Point pt0, Point pt1)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt0.X, 2) + Math.Pow(pt1.Y - pt0.Y, 2));
        }

        public static PathGeometry Flatten(this PathGeometry pathIn, FlattenOutput flattenOutput)
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
                    StartPoint = figureIn.StartPoint
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
                                    for (var i = 0; i < points.Length; i++)
                                    {
                                        var lineSegmentOut = new LineSegment {Point = points[i]};
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
                                    for (var i = 0; i < points.Length; i++)
                                    {
                                        polyLineSegmentOut.Points.Add(points[i]);
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
                                    for (var i = 0; i < points.Length; i++)
                                    {
                                        var lineSegmentOut = new LineSegment {Point = points[i]};
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
                                    for (var i = 0; i < points.Length; i++)
                                    {
                                        polyLineSegmentOut.Points.Add(points[i]);
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
                                    for (var i = 0; i < points.Length; i++)
                                    {
                                        var lineSegmentOut = new LineSegment {Point = points[i]};
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
                                    for (var i = 0; i < points.Length; i++)
                                    {
                                        polyLineSegmentOut.Points.Add(points[i]);
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
                                            for (var j = 0; j < points.Length; j++)
                                            {
                                                var lineSegmentOut = new LineSegment {Point = points[j]};
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
                                            for (var j = 0; j < points.Length; j++)
                                            {
                                                polyLineSegmentOut.Points.Add(points[j]);
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

#if true
                if (figureIn.IsClosed)
                {
                    var points = Interpolate(lastPoint, firstPoint);

                    switch (flattenOutput)
                    {
                        case FlattenOutput.Lines:
                        {
                            for (var i = 0; i < points.Length; i++)
                            {
                                var lineSegmentOut = new LineSegment {Point = points[i]};
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
                            for (var i = 0; i < points.Length; i++)
                            {
                                polyLineSegmentOut.Points.Add(points[i]);
                            }

                            figureOut.Segments?.Add(polyLineSegmentOut);
                        }
                            break;
                    }

                    firstPoint = lastPoint = new Point(0, 0);
                }
#endif
            }
            return pathOut;
        }

        public static PathGeometry ClonePathGeometry(this PathGeometry pathIn)
        {
            var pathOut = new PathGeometry()
            {
                FillRule = pathIn.FillRule
            };

            foreach (var figureIn in pathIn.Figures)
            {
                var figureOut = figureIn.ClonePathFigure();
                pathOut.Figures.Add(figureOut);
            }

            return pathOut;
        }

        public static PathFigure ClonePathFigure(this PathFigure figureIn)
        {
           var figureOut = new PathFigure()
            {
                IsClosed = figureIn.IsClosed,
                IsFilled = figureIn.IsClosed,
                StartPoint = figureIn.StartPoint
            };

            if (figureIn.Segments is null)
            {
                return figureOut;
            }

            foreach (var pathSegmentIn in figureIn.Segments)
            {
                switch (pathSegmentIn)
                {
                    case ArcSegment arcSegmentIn:
                    {
                        var arcSegmentOut = new ArcSegment()
                        {
                            IsLargeArc = arcSegmentIn.IsLargeArc,
                            Point = arcSegmentIn.Point,
                            RotationAngle = arcSegmentIn.RotationAngle,
                            Size = arcSegmentIn.Size,
                            SweepDirection = arcSegmentIn.SweepDirection
                        };
                        figureOut.Segments?.Add(arcSegmentOut);
                    }
                        break;
                    case BezierSegment bezierSegmentIn:
                    {
                        var bezierSegmentOut = new BezierSegment()
                        {
                            Point1 = bezierSegmentIn.Point1,
                            Point2 = bezierSegmentIn.Point2,
                            Point3 = bezierSegmentIn.Point3
                        };
                        figureOut.Segments?.Add(bezierSegmentOut);
                    }
                        break;
                    case LineSegment lineSegmentIn:
                    {
                        var lineSegmentOut = new LineSegment()
                        {
                            Point = lineSegmentIn.Point
                        };
                        figureOut.Segments?.Add(lineSegmentOut);
                    }
                        break;
                    case QuadraticBezierSegment quadraticBezierSegmentIn:
                    {
                        var quadraticBezierSegmentOut = new QuadraticBezierSegment()
                        {
                            Point1 = quadraticBezierSegmentIn.Point1,
                            Point2 = quadraticBezierSegmentIn.Point2,
                        };
                        figureOut.Segments?.Add(quadraticBezierSegmentOut);
                    }
                        break;
                    case PolyLineSegment polyLineSegmentIn:
                    {
                        var polyLineSegmentOut = new PolyLineSegment()
                        {
                            Points = new AvaloniaList<Point>()
                        };
                        foreach (var pt in polyLineSegmentIn.Points)
                        {
                            polyLineSegmentOut.Points.Add(pt);
                        }
                        figureOut.Segments?.Add(polyLineSegmentOut);
                    }
                        break;
                    default:
                    {
                        // TODO:
                    }
                        break;
                }
            }

            return figureOut;
        }
    }
}