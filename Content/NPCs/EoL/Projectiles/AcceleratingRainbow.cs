using System;
using System.IO;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class AcceleratingRainbow : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<EmpressOfLight>
    {
        /// <summary>
        /// The general color for the rainbow.
        /// </summary>
        public Color GeneralColor => Main.hslToRgb(HueInterpolant, 1f, 0.5f, 0) * Projectile.Opacity;

        /// <summary>
        /// How long this rainbow has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.localAI[0];

        /// <summary>
        /// The hue interpolant of this rainbow.
        /// </summary>
        public ref float HueInterpolant => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 32;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.hide = true;

            // Increased so that the hitbox checks are more precise.
            Projectile.MaxUpdates = 2;

            Projectile.timeLeft = Projectile.MaxUpdates * 210;
            Projectile.Opacity = 0f;

            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Time);

        public override void ReceiveExtraAI(BinaryReader reader) => Time = reader.ReadSingle();

        public override void AI()
        {
            // Sharply fade in.
            Projectile.Opacity = Utilities.InverseLerp(0f, 12f, Time);
            Projectile.scale = Utilities.InverseLerp(0f, 6f, Time);

            // Decide rotation based on direction.
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.IsFinalExtraUpdate())
            {
                Time++;
                Projectile.velocity *= 1.029f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public float RainbowWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width;
            float tipCutFactor = MathF.Pow(Utilities.InverseLerp(0.04f, 0.3f, completionRatio), 0.6f);
            float slownessFactor = Utils.Remap(Projectile.velocity.Length(), 3f, 9f, 0.18f, 1f);
            return baseWidth * tipCutFactor * slownessFactor * Projectile.scale;
        }

        public Color RainbowColorFunction(float completionRatio)
        {
            return Projectile.GetAlpha(Color.White);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            ManagedShader trailShader = ShaderManager.GetShader("WoTE.RainbowTrailShader");
            trailShader.SetTexture(TextureAssets.Extra[ExtrasID.HallowBossGradient], 1, SamplerState.LinearWrap);
            trailShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 2, SamplerState.LinearWrap);
            trailShader.TrySetParameter("localTime", -Main.GlobalTimeWrappedHourly + Projectile.identity * 0.383f);
            trailShader.TrySetParameter("hueOffset", HueInterpolant);
            trailShader.Apply();

            PrimitiveSettings settings = new(RainbowWidthFunction, RainbowColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 26f, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 30);
        }
    }
}
