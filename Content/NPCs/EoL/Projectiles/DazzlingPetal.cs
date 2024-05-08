using System;
using System.Collections.Generic;
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
using WoTE.Content.NPCs.EoL.Projectiles;
using static WoTE.Content.NPCs.EoL.EmpressOfLight;

namespace WoTE.Content.NPCs.EoL
{
    public class DazzlingPetal : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<EmpressOfLight>
    {
        public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.AfterNPCs;

        public float FlareInterpolant
        {
            get;
            set;
        }

        public float VanishInterpolant
        {
            get;
            set;
        }

        /// <summary>
        /// The directional offset angle of this petal.
        /// </summary>
        public ref float DirectionOffsetAngle => ref Projectile.ai[0];

        /// <summary>
        /// The length of this petal
        /// </summary>
        public ref float PetalLength => ref Projectile.ai[1];

        /// <summary>
        /// How long this petal has existed, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[2];

        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FairyQueenSunDance}";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
        }

        public override void SetDefaults()
        {
            Projectile.width = 94;
            Projectile.height = 94;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = 9999999;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            if (Myself is null)
            {
                Projectile.Kill();
                return;
            }

            if (Time <= 3f)
            {
                for (int i = 0; i < Projectile.oldRot.Length; i++)
                    Projectile.oldRot[i] = Projectile.rotation;
            }

            Projectile.velocity = DirectionOffsetAngle.ToRotationVector2();
            Projectile.Center = Myself.Center - Projectile.velocity - Projectile.Size * 0.5f;
            Projectile.rotation = DirectionOffsetAngle;

            FlareInterpolant = MathF.Pow(Utilities.InverseLerp(0f, TwirlingPetalSun_FlareTransformTime, Time - TwirlingPetalSun_TwirlTime), 0.85f);

            float spinSpeed = Utilities.InverseLerp(0f, TwirlingPetalSun_TwirlTime, Time) * 0.033f;
            DirectionOffsetAngle += MathF.Sqrt(1f - FlareInterpolant) * spinSpeed;

            float idealPetalLength = Utilities.InverseLerp(0f, 50f, Time).Squared() * 1000f;
            idealPetalLength -= Utilities.InverseLerp(0f, TwirlingPetalSun_FlareRetractTime, Time - TwirlingPetalSun_TwirlTime - TwirlingPetalSun_FlareTransformTime).Squared() * 500f;
            idealPetalLength += Utilities.InverseLerp(0f, TwirlingPetalSun_BurstTime, Time - TwirlingPetalSun_TwirlTime - TwirlingPetalSun_FlareTransformTime - TwirlingPetalSun_FlareRetractTime).Squared() * 4000f;

            PetalLength = MathHelper.Lerp(PetalLength, idealPetalLength, 0.2f);

            VanishInterpolant = Utilities.InverseLerp(0f, 24f, Time - TwirlingPetalSun_TwirlTime - TwirlingPetalSun_FlareTransformTime - TwirlingPetalSun_FlareRetractTime - TwirlingPetalSun_BurstTime).Squared();

            if (Time == TwirlingPetalSun_TwirlTime + TwirlingPetalSun_FlareTransformTime + TwirlingPetalSun_FlareRetractTime + TwirlingPetalSun_BurstTime)
            {
                ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 7f);
                for (int i = 0; i < 3; i++)
                {
                    Vector2 boltVelocity = Projectile.velocity.RotatedBy(MathHelper.Lerp(-0.09f, 0.09f, i / 2f)) * 4f;
                    Utilities.NewProjectileBetter(Projectile.GetSource_FromThis(), Projectile.Center + boltVelocity * 3f, boltVelocity, ModContent.ProjectileType<AcceleratingRainbow>(), 180, 0f, -1, -Main.rand.NextFloat(0.12f));
                }
            }

            Time++;

            if (VanishInterpolant >= 1f)
                Projectile.Kill();
        }

        public float PetalWidthFunction(float completionRatio)
        {
            float tipWidthFactor = MathF.Pow(Utilities.InverseLerp(0.95f, 0f, completionRatio), 0.65f) * MathF.Pow(Utilities.InverseLerp(0f, 0.54f, completionRatio), 0.4f);

            float baseWidth = Projectile.width * MathHelper.Lerp(1f, 2f, FlareInterpolant);
            return baseWidth * tipWidthFactor;
        }

        public Color PetalColorFunction(float completionRatio)
        {
            float fadeStart = 0f + VanishInterpolant;
            float fadeEnd = 1f;
            float edgeFade = Utilities.InverseLerpBump(fadeStart - 0.1f, fadeStart, fadeEnd, fadeEnd + 0.1f, completionRatio);

            float hue = (Main.GlobalTimeWrappedHourly * 0.13f + MathF.Sin(DirectionOffsetAngle) * 0.08f).Modulo(1f);
            Color baseColor = Main.hslToRgb(hue, 1f, 0.75f);
            return Projectile.GetAlpha(baseColor) * Utilities.InverseLerp(1f, 0.54f, completionRatio) * edgeFade * (1f - VanishInterpolant);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            ManagedShader trailShader = ShaderManager.GetShader("WoTE.DazzlingPetalShader");
            trailShader.TrySetParameter("fireColorInterpolant", FlareInterpolant);
            trailShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(TextureAssets.Extra[ExtrasID.FlameLashTrailShape], 2, SamplerState.LinearWrap);
            trailShader.Apply();

            float petalLengthFactor = MathHelper.Lerp(1f, 0.4f, FlareInterpolant.Squared());
            List<Vector2> controlPoints = Projectile.GetLaserControlPoints(Projectile.oldPos.Length, PetalLength * petalLengthFactor);
            for (int i = 0; i < controlPoints.Count; i++)
            {
                float angularOffset = MathHelper.WrapAngle(Projectile.oldRot[i] - Projectile.rotation);
                Vector2 offsetFromCenter = controlPoints[i] - Projectile.Center;
                Vector2 twirledControlPoint = Projectile.Center + offsetFromCenter.RotatedBy(angularOffset);
                controlPoints[i] = Vector2.Lerp(controlPoints[i], twirledControlPoint, FlareInterpolant);
            }

            PrimitiveSettings settings = new(PetalWidthFunction, PetalColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(controlPoints, settings, 25);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
