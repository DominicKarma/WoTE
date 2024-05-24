using System;
using System.IO;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class GlowingAurora : ModProjectile, IProjOwnedByBoss<EmpressOfLight>
    {
        /// <summary>
        /// How long this aurora has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.localAI[0];

        public static int Lifetime => Utilities.SecondsToFrames(3.5f);

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.HallowBossDeathAurora}";

        public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 720;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = Lifetime;

            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Time);

        public override void ReceiveExtraAI(BinaryReader reader) => Time = reader.ReadSingle();

        public override bool PreDraw(ref Color lightColor)
        {
            if (EmpressOfLight.Myself is null)
                return false;

            EmpressPaletteSet palette = EmpressOfLight.Myself.As<EmpressOfLight>().Palette;
            Texture2D value = TextureAssets.Projectile[Type].Value;
            Vector2 origin = value.Size() * 0.5f;
            float timeInterpolant = Main.GlobalTimeWrappedHourly % 10f / 10f;
            Vector2 baseDrawPosition = Projectile.Center - Main.screenPosition;

            int auroraCount = 25;
            Vector2[] auroraDrawOffsets = new Vector2[auroraCount];
            float[] auroraHues = new float[auroraCount];
            float[] auroraScales = new float[auroraCount];
            float opacityA = Utilities.InverseLerp(0f, 60f, Projectile.timeLeft) * Utilities.InverseLerp(Lifetime, Lifetime - 60, Projectile.timeLeft);
            float opacityB = MathHelper.Lerp(0.2f, 0.5f, Utilities.InverseLerp(0f, 60f, Projectile.timeLeft) * Utilities.InverseLerp(Lifetime, 90f, Projectile.timeLeft));
            float area = 800f / value.Width;
            float scaleFactorPerIncrement = area / auroraCount * 0.2f;
            for (int i = 0; i < auroraCount; i++)
            {
                float sinusoidalOffset = MathF.Sin(timeInterpolant * MathHelper.TwoPi + MathHelper.PiOver2 + i / 2f);
                auroraDrawOffsets[i] = new(sinusoidalOffset * (300f - i * 3f), MathF.Sin(timeInterpolant * MathHelper.TwoPi * 2f + MathHelper.Pi / 3f + i) * 30f - i * 3f);
                auroraHues[i] = (sinusoidalOffset * 0.5f + 0.5f) * 0.6f + timeInterpolant;
                auroraScales[i] = (area * 0.8f + (i + 1) * scaleFactorPerIncrement) * 0.3f;

                Color color = palette.MulticolorLerp(EmpressPaletteType.RainbowArrow, auroraHues[i] % 1f) * opacityA * opacityB;
                color.A /= 4;

                float rotation = MathHelper.PiOver2 + sinusoidalOffset * MathHelper.PiOver4 * -0.3f + MathHelper.Pi * i;
                Main.EntitySpriteDraw(value, baseDrawPosition + auroraDrawOffsets[i], null, color, rotation, origin, new Vector2(3f, 6f) * auroraScales[i], 0);
            }
            return false;
        }
    }
}
