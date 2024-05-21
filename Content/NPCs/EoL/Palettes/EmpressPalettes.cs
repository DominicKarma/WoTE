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
            WithPalette(EmpressPaletteType.LacewingTrail, new Color(189, 126, 255), new(255, 174, 240), new(255, 44, 196), new(148, 38, 187), new(10, 105, 187), new(109, 200, 252)).
            WithPalette(EmpressPaletteType.DazzlingPetal, new Color(51, 137, 255), new(122, 107, 160)).
            WithCloudColor(new(17, 172, 209, 128)).
            WithMistColor(new(185, 170, 237, 128)).
            WithMoonColors(new(200, 238, 235, 75), new(17, 172, 209, 0)).
            WithBackgroundTint(Color.White * 0.4f);

        /// <summary>
        /// The Empress palette used during the daytime.
        /// </summary>
        public static readonly EmpressPaletteSet DaytimePaletteSet = RegisterNew(1, () => Main.dayTime).
            WithPalette(EmpressPaletteType.ButterflyAvatar, new Color(248, 49, 1), new(254, 147, 8), new(255, 239, 195), new(52, 7, 79), new(142, 28, 0)).
            WithPalette(EmpressPaletteType.Wings, new Color(248, 49, 1), new(254, 147, 8), new(255, 239, 195), new(52, 7, 79), new(142, 28, 0)).
            WithPalette(EmpressPaletteType.Phase2Dress, new Color(248, 49, 1), new(254, 147, 8), new(255, 239, 195), new(52, 7, 79)).
            WithPalette(EmpressPaletteType.PrismaticBolt, new Color(248, 49, 1), new(254, 147, 8), new(255, 239, 195), new(52, 7, 79)).
            WithPalette(EmpressPaletteType.StarBolt, new Color(248, 49, 1), new(254, 147, 8), new(255, 239, 195), new(52, 7, 79), new(142, 28, 0), Color.White).
            WithPalette(EmpressPaletteType.RainbowArrow, new Color(248, 49, 1), new(254, 147, 8), new(255, 239, 195), new(52, 7, 79)).
            WithPalette(EmpressPaletteType.LacewingTrail, new Color(248, 49, 1), new(254, 147, 8), new(255, 239, 195), new(52, 7, 79), new(142, 28, 0), Color.White).
            WithPalette(EmpressPaletteType.DazzlingPetal, new Color(248, 49, 1), new(255, 239, 195)).
            WithCloudColor(new(255, 74, 11, 165)).
            WithMistColor(new(197, 25, 22, 190)).
            WithMoonColors(Color.Transparent, Color.Transparent).
            WithBackgroundTint(new Color(213, 24, 25) * 0.75f);

        /// <summary>
        /// The Empress palette used during the eclipse.
        /// </summary>
        public static readonly EmpressPaletteSet EclipsePaletteSet = RegisterNew(2, () => Main.dayTime && Main.eclipse).
            WithPalette(EmpressPaletteType.ButterflyAvatar, new Color(7, 1, 0), new(158, 39, 0), new(188, 67, 0), new(255, 228, 83)).
            WithPalette(EmpressPaletteType.Wings, new Color(7, 1, 0), new(158, 39, 0), new(188, 67, 0), new(255, 228, 83)).
            WithPalette(EmpressPaletteType.Phase2Dress, new Color(7, 1, 0), new(158, 39, 0), new(188, 67, 0), new(255, 228, 83)).
            WithPalette(EmpressPaletteType.PrismaticBolt, new Color(7, 1, 0), new(158, 39, 0), new(188, 67, 0), new(255, 228, 83)).
            WithPalette(EmpressPaletteType.StarBolt, new Color(7, 1, 0), new(158, 39, 0), new(188, 67, 0), new(255, 228, 83)).
            WithPalette(EmpressPaletteType.RainbowArrow, new Color(7, 1, 0), new(158, 39, 0), new(188, 67, 0), new(255, 228, 83)).
            WithPalette(EmpressPaletteType.LacewingTrail, new Color(7, 1, 0), new(158, 39, 0), new(188, 67, 0), new(255, 228, 83)).
            WithPalette(EmpressPaletteType.DazzlingPetal, new Color(7, 1, 0), new(158, 39, 0), new(188, 67, 0), new(255, 228, 83)).
            WithCloudColor(Color.Transparent).
            WithMistColor(Color.Transparent).
            WithMoonColors(Color.Transparent, Color.Transparent).
            WithBackgroundTint(Color.Transparent);

        /// <summary>
        /// The Empress palette used if the player is named "Lynel". This name check does not care about casing.
        /// This palette is also used during blood moons.
        /// </summary>
        public static readonly EmpressPaletteSet BloodMoonPaletteSet = RegisterNew(5, () => Main.LocalPlayer.name.Equals("Lynel", StringComparison.OrdinalIgnoreCase) || Main.bloodMoon).
            WithPalette(EmpressPaletteType.ButterflyAvatar, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White).
            WithPalette(EmpressPaletteType.Wings, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72)).
            WithPalette(EmpressPaletteType.Phase2Dress, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72)).
            WithPalette(EmpressPaletteType.PrismaticBolt, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White).
            WithPalette(EmpressPaletteType.StarBolt, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White).
            WithPalette(EmpressPaletteType.RainbowArrow, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White).
            WithPalette(EmpressPaletteType.LacewingTrail, Color.Black, Color.Black, new(33, 0, 62), new(250, 24, 72), Color.White).
            WithPalette(EmpressPaletteType.DazzlingPetal, new Color(255, 47, 11), new(255, 12, 102)).
            WithCloudColor(new(255, 74, 11, 165)).
            WithMistColor(new(244, 39, 72, 128)).
            WithMoonColors(new(255, 39, 4, 148), new(237, 168, 179, 0)).
            WithBackgroundTint(new Color(255, 40, 2) * 0.4f);

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
