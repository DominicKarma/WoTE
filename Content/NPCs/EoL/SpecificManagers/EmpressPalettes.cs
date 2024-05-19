using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;

namespace WoTE.Content.NPCs.EoL
{
    // TODO -- Make these static-readonly where possible.
    public static class EmpressPalettes
    {
        /// <summary>
        /// The palette the Empress uses for her butterfly avatar form.
        /// </summary>
        public static Vector4[] ButterflyAvatarPalette => new Vector4[]
        {
            Color.White.ToVector4(),
            Color.Wheat.ToVector4(),
            Color.LightGoldenrodYellow.ToVector4(),
            Color.White.ToVector4(),
        };

        /// <summary>
        /// The palette the Empress uses for her wings.
        /// </summary>
        public static Vector4[] WingsPalette => new Vector4[]
        {
            Color.HotPink.ToVector4(),
            Color.White.ToVector4(),
            Color.Aqua.ToVector4(),
            new Color(240, 243, 184).ToVector4(),
        };

        /// <summary>
        /// The palette the Empress uses for her dress in her second phase.
        /// </summary>
        public static Vector4[] DressP2Palette
        {
            get
            {
                float hueShift = -Utilities.Sin01(MathHelper.Pi * Main.GlobalTimeWrappedHourly) * 0.12f;
                return new Vector4[]
                {
                    new Color(74, 18, 46).HueShift(hueShift).ToVector4(),
                    new Color(124, 38, 96).HueShift(hueShift * 1.15f).ToVector4(),
                    new Color(188, 88, 172).HueShift(hueShift * 1.3f).ToVector4(),
                    new Color(255, 150, 255).HueShift(hueShift * 1.45f).ToVector4(),
                    new Color(255, 228, 224).HueShift(hueShift * 1.6f).ToVector4(),
                    new Color(255, 79, 139).HueShift(hueShift * 1.75f).ToVector4(),
                    new Color(255, 79, 139).HueShift(hueShift * 2.2f).ToVector4(),
                };
            }
        }

        /// <summary>
        /// The palette the Empress uses for her prismatic bolts.
        /// </summary>
        public static Vector4[] PrismaticBoltPalette => new Vector4[]
        {
            new Color(207, 0, 151).ToVector4(),
            new Color(255, 220, 154).ToVector4(),
            new Color(0, 255, 255).ToVector4(),
            new Color(35, 178, 255).ToVector4(),
            new Color(144, 61, 196).ToVector4()
        };

        /// <summary>
        /// The palette the Empress uses for her star bolts.
        /// </summary>
        public static Vector4[] StarBoltPalette => new Vector4[]
        {
            new Color(255, 147, 176).ToVector4(),
            new Color(255, 162, 252).ToVector4(),
            new Color(177, 48, 209).ToVector4(),
            new Color(255, 79, 196).ToVector4(),
            new Color(255, 72, 75).ToVector4(),
            new Color(255, 156, 67).ToVector4(),
            new Color(255, 216, 184).ToVector4(),
        };

        /// <summary>
        /// The palette the Empress uses for her star bolts.
        /// </summary>
        public static Vector4[] RainbowPalette => new Vector4[]
        {
            Main.hslToRgb(0f, 1f, 0.5f).ToVector4(),
            Main.hslToRgb(0.125f, 1f, 0.5f).ToVector4(),
            Main.hslToRgb(0.25f, 1f, 0.5f).ToVector4(),
            Main.hslToRgb(0.375f, 1f, 0.5f).ToVector4(),
            Main.hslToRgb(0.5f, 1f, 0.5f).ToVector4(),
            Main.hslToRgb(0.625f, 1f, 0.5f).ToVector4(),
            Main.hslToRgb(0.75f, 1f, 0.5f).ToVector4(),
            Main.hslToRgb(0.875f, 1f, 0.5f).ToVector4(),
        };

        /// <summary>
        /// The palette the Empress' split form lacewings use for their afterimage trails.
        /// </summary>
        public static Vector4[] LacewingTrailPalette => new Vector4[]
        {
            new Color(189, 126, 255).ToVector4(),
            new Color(255, 174, 240).ToVector4(),
            new Color(255, 44, 196).ToVector4(),
            new Color(148, 38, 187).ToVector4(),
            new Color(10, 105, 187).ToVector4(),
            new Color(109, 200, 252).ToVector4(),
        };
    }
}
