using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace WoTE.Common.ShapeCurves
{
    public class ShapeCurveManager : ModSystem
    {
        private static readonly Dictionary<string, ShapeCurve> shapes = [];

        public override void OnModLoad()
        {
            // Load all shape points.
            // In binary they are simply stored as a list of paired, unordered X/Y floats. They are normalized such that their values never exceed a 0 to 1 range, and can thusly
            // be scaled up easily via the inbuilt ShapeCurve methods.
            foreach (string path in Mod.GetFileNames().Where(f => f.Contains("Common/ShapeCurves/") && Path.GetExtension(f) == ".vec"))
            {
                byte[] curveBytes = Mod.GetFileBytes(path);
                if (curveBytes.Length <= 0)
                    return;

                using MemoryStream byteStream = new(curveBytes);
                using BinaryReader reader = new(byteStream);

                shapes[Path.GetFileNameWithoutExtension(path)] = ReadFrom(reader);
            }
        }

        /// <summary>
        /// Reads the contents of a <see cref="ShapeCurve"/> instance from a given <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The binary reader to read contents from.</param>
        private static ShapeCurve ReadFrom(BinaryReader reader)
        {
            // Read points from the file's binary data and store them in the universal registry.
            int pointCount = reader.ReadInt32();
            List<Vector2> shapePoints = new(pointCount);
            for (int i = 0; i < pointCount; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                shapePoints.Add(new(x, y));
            }

            return new(shapePoints);
        }

        /// <summary>
        /// Safely attempts to find a given shape curve.
        /// </summary>
        /// <param name="name">The name of the shape curve.</param>
        /// <param name="curve">The resulting curve. <see langword="null"/> if the shape couldn't be found.</param>
        /// <returns>Whether the shape curve search was successful.</returns>
        public static bool TryFind(string name, out ShapeCurve curve)
        {
            // This usage of the ! operator is a bit improper, but really it should be on the developer to take care with the TryFind results.
            curve = default!;

            if (shapes.TryGetValue(name, out ShapeCurve? s))
            {
                curve = s;
                return true;
            }
            return false;
        }
    }
}
