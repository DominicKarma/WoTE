using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;

namespace WoTE.Content.NPCs.EoL
{
    public static class EmpressPalettes
    {
        private static readonly List<EmpressPaletteSet> paletteSets = [];

        /// <summary>
        /// The default Empress palette.
        /// </summary>
        public static readonly EmpressPaletteSet Default = RegisterNew(0, () => true).
            WithPalette(EmpressPaletteType.ButterflyAvatar, Color.White, Color.Wheat, Color.LightGoldenrodYellow, Color.White).
            WithPalette(EmpressPaletteType.Wings, Color.HotPink, Color.White, Color.Aqua, new(240, 243, 184)).
            WithPalette(EmpressPaletteType.Phase2Dress, Color.White, Color.Wheat, Color.HotPink, Color.MediumPurple, new(12, 104, 190)).
            WithPalette(EmpressPaletteType.PrismaticBolt, new Color(207, 0, 151), new(255, 220, 154), new(0, 255, 255), new(35, 175, 255), new(144, 61, 196)).
            WithPalette(EmpressPaletteType.StarBolt, new Color(255, 147, 176), new(255, 162, 252), new(177, 48, 209), new(255, 79, 196), new(255, 72, 75), new(255, 156, 67), new(255, 216, 184)).
            WithPalette(EmpressPaletteType.RainbowArrow, Main.hslToRgb(0f, 1f, 0.5f), Main.hslToRgb(0.125f, 1f, 0.5f), Main.hslToRgb(0.25f, 1f, 0.5f), Main.hslToRgb(0.375f, 1f, 0.5f),
                                                         Main.hslToRgb(0.5f, 1f, 0.5f), Main.hslToRgb(0.625f, 1f, 0.5f), Main.hslToRgb(0.75f, 1f, 0.5f), Main.hslToRgb(0.75f, 1f, 0.5f), Main.hslToRgb(0.875f, 1f, 0.5f)).
            WithPalette(EmpressPaletteType.LacewingTrail, new Color(189, 126, 255), new(255, 174, 240), new(255, 44, 196), new(148, 38, 187), new(10, 105, 187), new(109, 200, 252));

        /// <summary>
        /// The Empress palette used if the player is named "Dominic".
        /// </summary>
        public static readonly EmpressPaletteSet DominicPaletteSet = RegisterNew(1, () => Main.LocalPlayer.name.Equals("Dominic", StringComparison.OrdinalIgnoreCase)).
            WithPalette(EmpressPaletteType.ButterflyAvatar, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White).
            WithPalette(EmpressPaletteType.Wings, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72)).
            WithPalette(EmpressPaletteType.Phase2Dress, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72)).
            WithPalette(EmpressPaletteType.PrismaticBolt, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White).
            WithPalette(EmpressPaletteType.StarBolt, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White).
            WithPalette(EmpressPaletteType.RainbowArrow, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White).
            WithPalette(EmpressPaletteType.LacewingTrail, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White);

        /// <summary>
        /// Registers a new palette set that the Empress can use.
        /// </summary>
        /// <param name="priority">The priority of the palette.</param>
        /// <param name="usageCondition">The usage condition of the palette.</param>
        private static EmpressPaletteSet RegisterNew(int priority, Func<bool> usageCondition)
        {
            EmpressPaletteSet paletteSet = new(priority, usageCondition);
            paletteSets.Add(paletteSet);

            return paletteSet;
        }

        /// <summary>
        /// Chooses the first palette set available from the central registry, taking into account priority.
        /// </summary>
        ///
        /// <remarks>
        /// <see cref="Default"/> is returned if no valid palette could be found.
        /// </remarks>
        public static EmpressPaletteSet Choose()
        {
            var availableSets = paletteSets.Where(p => p.UsageCondition()).OrderByDescending(p => p.Priority);
            if (availableSets.Any())
                return availableSets.First();

            return Default;
        }
    }
}
