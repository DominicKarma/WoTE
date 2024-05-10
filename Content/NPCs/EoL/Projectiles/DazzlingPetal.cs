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
            Projectile.hide = true;
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
            Projectile.Opacity = Utilities.InverseLerp(0f, 90f, Time).Cubed();
            Projectile.rotation = DirectionOffsetAngle;

            FlareInterpolant = MathF.Pow(Utilities.InverseLerp(0f, TwirlingPetalSun_FlareTransformTime, Time - TwirlingPetalSun_TwirlTime), 0.85f);

            // SPIN
            // 2
            // WIN
            float spinSpeed = MathF.Sqrt(1f - FlareInterpolant) * Utilities.InverseLerp(0f, TwirlingPetalSun_TwirlTime, Time) * EmpressOfLight.TwirlingPetalSun_PetalSpinSpeed;
            DirectionOffsetAngle += spinSpeed;

            // Extend outward.
            float idealPetalLength = Projectile.Opacity * 1000f;
            idealPetalLength -= Utilities.InverseLerp(0f, TwirlingPetalSun_FlareRetractTime, Time - TwirlingPetalSun_TwirlTime - TwirlingPetalSun_FlareTransformTime).Squared() * 500f;
            idealPetalLength += Utilities.InverseLerp(0f, TwirlingPetalSun_BurstTime, Time - TwirlingPetalSun_TwirlTime - TwirlingPetalSun_FlareTransformTime - TwirlingPetalSun_FlareRetractTime).Squared() * 4000f;
            PetalLength = MathHelper.Lerp(PetalLength, idealPetalLength, 0.2f);

            VanishInterpolant = Utilities.InverseLerp(0f, 24f, Time - TwirlingPetalSun_TwirlTime - TwirlingPetalSun_FlareTransformTime - TwirlingPetalSun_FlareRetractTime - TwirlingPetalSun_BurstTime).Squared();

            if (Time == TwirlingPetalSun_TwirlTime + TwirlingPetalSun_FlareTransformTime + TwirlingPetalSun_FlareRetractTime + TwirlingPetalSun_BurstTime)
                CreateRainbowBurst();

            Time++;

            if (VanishInterpolant >= 1f)
                Projectile.Kill();
        }

        public void CreateRainbowBurst()
        {
            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 7f);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 boltVelocity = Projectile.velocity.RotatedBy(MathHelper.Lerp(-0.09f, 0.09f, i / 2f)) * 4f;
                Utilities.NewProjectileBetter(Projectile.GetSource_FromThis(), Projectile.Center + boltVelocity * 3f, boltVelocity, ModContent.ProjectileType<AcceleratingRainbow>(), 180, 0f, -1, -Main.rand.NextFloat(0.12f));
            }
        }

        public float PetalWidthFunction(float completionRatio)
        {
            float fadeInWidthFactor = Utilities.InverseLerp(0f, 75f, Time);
            float tipWidthFactor = MathF.Pow(Utilities.InverseLerp(0.95f, 0f, completionRatio), 0.65f) * MathF.Pow(Utilities.InverseLerp(0f, 0.54f, completionRatio), 0.4f);
            float flareWidthFactor = MathHelper.Lerp(1f, 2f, FlareInterpolant);
            float baseWidth = Projectile.width;
            return baseWidth * tipWidthFactor * flareWidthFactor * fadeInWidthFactor;
        }

        public Color PetalColorFunction(float completionRatio)
        {
            float fadeStart = VanishInterpolant;
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

        public override bool? CanDamage() => Projectile.Opacity >= 0.7f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Measure how far along the petal's length the target is.
            // If the signed distance is negative (a.k.a. they're behind the petal) or above the petal length (a.k.a. they're beyond the petal), terminate this
            // method immediately.
            Vector2 direction = Projectile.velocity.RotatedBy(MathHelper.PiOver2);
            float signedDistanceAlongPetal = Utilities.SignedDistanceToLine(targetHitbox.Center(), Projectile.Center, direction);
            if (signedDistanceAlongPetal < VanishInterpolant * PetalLength || signedDistanceAlongPetal >= PetalLength * 0.9f)
                return false;

            // Now that the point on the petal is known from the distance, evaluate the exact width of the petal at said point for use with a AABB/line collision check.
            // The petal width is reduced somewhat based on the flare interpolant, since most of the edge of that isn't super hot.
            float fadePositionWidthFade = MathHelper.Lerp(1f, 0.61f, FlareInterpolant);
            float petalWidth = PetalWidthFunction(signedDistanceAlongPetal / PetalLength) * fadePositionWidthFade * 0.45f;
            Vector2 perpendicular = new(-direction.Y, direction.X);
            Vector2 petalPoint = Projectile.Center + direction * signedDistanceAlongPetal;
            Vector2 left = petalPoint - perpendicular * petalWidth;
            Vector2 right = petalPoint + perpendicular * petalWidth;

            Dust.QuickDust(left, Color.Green);
            Dust.QuickDust(right, Color.Green);

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), left, right, 6f, ref _);
        }
    }
}
