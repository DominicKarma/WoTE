using System;
using System.IO;
using Luminance.Assets;
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
    public class LightLance : ModProjectile, IProjOwnedByBoss<EmpressOfLight>
    {
        /// <summary>
        /// Whether this lance should accelerate or not.
        /// </summary>
        public bool Accelerate
        {
            get => Projectile.ai[2] == 1f;
            set => Projectile.ai[2] = value.ToInt();
        }

        /// <summary>
        /// The appearance interpolant for the lance.
        /// </summary>
        public float AppearInterpolant => Utilities.InverseLerp(TelegraphTime - 16f, TelegraphTime - 3f, Time);

        /// <summary>
        /// The general color for the lance.
        /// </summary>
        public Color GeneralColor
        {
            get
            {
                Color baseColor = Color.White;
                if (EmpressOfLight.Myself is not null)
                    baseColor = EmpressOfLight.Myself.As<EmpressOfLight>().Palette.MulticolorLerp(EmpressPaletteType.LacewingTrail, (HueInterpolant - Time * 0.01f).Modulo(1f));
                baseColor.A = 0;

                return baseColor * Projectile.Opacity;
            }
        }

        /// <summary>
        /// How long this lance has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.localAI[0];

        /// <summary>
        /// How long this lance should telegraph for, in frames.
        /// </summary>
        public ref float TelegraphTime => ref Projectile.ai[0];

        /// <summary>
        /// The hue interpolant of this lance.
        /// </summary>
        public ref float HueInterpolant => ref Projectile.ai[1];

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FairyQueenLance}";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;

            // Increased so that the hitbox checks are more precise.
            Projectile.MaxUpdates = 2;

            Projectile.timeLeft = Projectile.MaxUpdates * 90;
            Projectile.Opacity = 0f;
            if (EmpressOfLight.Myself is not null && EmpressOfLight.Myself.As<EmpressOfLight>().CurrentState == EmpressAIType.PrismaticOverload)
                Projectile.scale = Main.rand?.NextFloat(0.67f, 1.85f) ?? 1f;

            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Time);

        public override void ReceiveExtraAI(BinaryReader reader) => Time = reader.ReadSingle();

        public override void AI()
        {
            // Sharply fade in.
            Projectile.Opacity = Utilities.InverseLerp(0f, 6f, Time) * Utilities.InverseLerp(0f, Projectile.MaxUpdates * 12f, Projectile.timeLeft);

            // Decide rotation based on direction.
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Accelerate after the telegraph dissipates.
            if (Time >= TelegraphTime && Accelerate)
            {
                float newSpeed = MathHelper.Clamp(Projectile.velocity.Length() + 2f / Projectile.MaxUpdates, 14f, 100f / Projectile.MaxUpdates);
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * newSpeed;
            }

            if (Projectile.IsFinalExtraUpdate())
                Time++;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            if (Time <= TelegraphTime)
                DrawTelegraph();

            // Draw bloom underneath the dagger. This is strongest when the blade itself has not yet fully faded in.
            float bloomOpacity = MathHelper.Lerp(0.75f, 0.51f, AppearInterpolant) * Projectile.Opacity;

            Color c1 = GeneralColor.HueShift(0.05f);
            Color c2 = Color.White * 0.51f;
            c1.A = 0;
            c2.A = 0;

            Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
            Vector2 bloomScale = new Vector2(2f, 1f) * Projectile.scale;
            Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null, c2 * bloomOpacity, Projectile.rotation, bloom.Size() * 0.5f, bloomScale * 1.01f, 0, 0);
            Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null, c1 * bloomOpacity, Projectile.rotation, bloom.Size() * 0.5f, bloomScale * 0.6f, 0, 0);

            // Make the dagger appear near the end of the telegraph fade-in.
            float daggerOffsetFactor = Projectile.velocity.Length() * 0.2f;
            Texture2D daggerTexture = TextureAssets.Projectile[Type].Value;
            for (int i = 0; i < 30; i++)
            {
                float daggerScale = MathHelper.Lerp(1f, 0.48f, i / 29f) * Projectile.scale;
                Vector2 daggerDrawOffset = Projectile.velocity.SafeNormalize(Vector2.UnitY) * AppearInterpolant * i * daggerScale * -daggerOffsetFactor;
                Color daggerDrawColor = c1 * AppearInterpolant * MathF.Pow(1f - i / 10f, 1.6f) * Projectile.Opacity * 1.8f;
                Main.EntitySpriteDraw(daggerTexture, Projectile.Center + daggerDrawOffset - Main.screenPosition, null, daggerDrawColor, Projectile.rotation, daggerTexture.Size() * 0.5f, daggerScale, 0, 0);
            }

            Color mainDaggerColor = Color.Wheat * Projectile.Opacity * AppearInterpolant;
            mainDaggerColor.A /= 9;
            Main.EntitySpriteDraw(daggerTexture, Projectile.Center - Main.screenPosition, null, mainDaggerColor, Projectile.rotation, daggerTexture.Size() * 0.5f, Projectile.scale, 0, 0);
            return false;
        }

        public void DrawTelegraph()
        {
            Vector2 start = Projectile.Center;
            Vector2 end = start + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 2100f;
            Main.spriteBatch.DrawBloomLine(start, end, GeneralColor * MathF.Sqrt(1f - AppearInterpolant), Projectile.Opacity * 30f);
        }

        public override bool? CanDamage() => Time >= TelegraphTime;

        public override bool ShouldUpdatePosition() => Time >= TelegraphTime;
    }
}
