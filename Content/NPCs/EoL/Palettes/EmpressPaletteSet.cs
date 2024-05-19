using System;
using System.Collections.Generic;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;

namespace WoTE.Content.NPCs.EoL
{
    public class EmpressPaletteSet
    {
        private readonly Dictionary<EmpressPaletteType, Vector4[]> palettes = [];

        /// <summary>
        /// The priority of this palette. Higher values take precedent over lower ones if two or more conditions are met.
        /// </summary>
        public int Priority
        {
            get;
            private set;
        }

        /// <summary>
        /// The condition necessary for this palette to be used.
        /// </summary>
        public Func<bool> UsageCondition
        {
            get;
            private set;
        }

        public EmpressPaletteSet(int priority, Func<bool> usageCondition)
        {
            Priority = priority;
            UsageCondition = usageCondition;
        }

        public EmpressPaletteSet WithPalette(EmpressPaletteType type, params Color[] palette)
        {
            Vector4[] covertedPalette = new Vector4[palette.Length];
            for (int i = 0; i < palette.Length; i++)
                covertedPalette[i] = palette[i].ToVector4();

            return WithPalette(type, covertedPalette);
        }

        public EmpressPaletteSet WithPalette(EmpressPaletteType type, params Vector4[] palette)
        {
            palettes[type] = palette;
            return this;
        }

        /// <summary>
        /// Returns a given <see cref="EmpressPaletteType"/> from the set, returning an empty array of <see cref="Vector4"/>s if it couldn't be found.
        /// </summary>
        /// <param name="type">The type of palette to get.</param>
        public Vector4[] Get(EmpressPaletteType type)
        {
            if (palettes.TryGetValue(type, out Vector4[]? palette) && palette is not null)
                return palette;

            return [];
        }

        /// <summary>
        /// Performs a multi-color lerp on a given palette.
        /// </summary>
        /// <param name="type">The type of palette to use.</param>
        /// <param name="colorInterpolant">The color interpolant.</param>
        public Color MulticolorLerp(EmpressPaletteType type, float colorInterpolant)
        {
            Vector4[] palette = Get(type);

            colorInterpolant = colorInterpolant.Modulo(0.999f);

            int gradientStartingIndex = (int)(colorInterpolant * palette.Length);
            float currentColorInterpolant = colorInterpolant * palette.Length % 1f;
            Color gradientSubdivisionA = new(palette[gradientStartingIndex]);
            Color gradientSubdivisionB = new(palette[(gradientStartingIndex + 1) % palette.Length]);
            return Color.Lerp(gradientSubdivisionA, gradientSubdivisionB, currentColorInterpolant);
        }
    }
}
