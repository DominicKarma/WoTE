using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;

namespace WoTE.Common.ShapeCurves
{
    public class ShapeCurve
    {
        public Vector2 Center;

        public List<Vector2> ShapePoints;

        public ShapeCurve()
        {
            ShapePoints = [];
            Center = Vector2.Zero;
        }

        public ShapeCurve(List<Vector2> shapePoints)
        {
            ShapePoints = shapePoints;

            Center = Vector2.Zero;
            for (int i = 0; i < ShapePoints.Count; i++)
                Center += ShapePoints[i];
            Center /= ShapePoints.Count;
        }

        public ShapeCurve Upscale(float upscaleFactor)
        {
            List<Vector2> upscaledPoints = [];
            float maxX = ShapePoints.Max(p => p.X);
            for (int i = 0; i < ShapePoints.Count; i++)
                upscaledPoints.Add((ShapePoints[i] - Vector2.UnitY * 0.5f) * upscaleFactor + Vector2.UnitY * 0.5f + Vector2.UnitX * -maxX * upscaleFactor * 0.5f);

            return new(upscaledPoints);
        }

        public ShapeCurve Rotate(float angle)
        {
            List<Vector2> rotatedPoints = [];
            for (int i = 0; i < ShapePoints.Count; i++)
                rotatedPoints.Add(ShapePoints[i].RotatedBy(angle, Center));

            return new(rotatedPoints);
        }

        public ShapeCurve LinearlyTransform(Matrix transformation)
        {
            List<Vector2> transformedPoints = [];
            for (int i = 0; i < ShapePoints.Count; i++)
                transformedPoints.Add(Vector2.Transform(ShapePoints[i], transformation));

            return new(transformedPoints);
        }

        public ShapeCurve VerticalFlip()
        {
            List<Vector2> rotatedPoints = [];
            float maxY = ShapePoints.Max(p => p.Y);

            for (int i = 0; i < ShapePoints.Count; i++)
                rotatedPoints.Add(new(ShapePoints[i].X, maxY - ShapePoints[i].Y));

            return new(rotatedPoints);
        }
    }
}
