using System.Collections.Generic;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class LanceWallTelegraph : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<EmpressOfLight>
    {
        public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.AfterProjectiles;

        /// <summary>
        /// How long this telegraph has existed, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[0];

        /// <summary>
        /// How long this telegraph should exist for, in frames.
        /// </summary>
        public static int Lifetime => Utilities.SecondsToFrames(1.7f);

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 5000;

        public override void SetDefaults()
        {
            Projectile.width = (int)EmpressOfLight.LanceWallSupport_WallWidth + 24;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.hide = true;
            Projectile.timeLeft = Lifetime;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.Opacity = Utilities.InverseLerp(0.95f, 0.45f, Time / Lifetime) * Utilities.InverseLerp(0f, 30f, Time);
            Projectile.scale = 1f + (Time / Lifetime).Squared() * 0.8f;
            Time++;
        }

        public float TelegraphWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width;
            return baseWidth * Projectile.scale;
        }

        public Color TelegraphColorFunction(float completionRatio)
        {
            if (EmpressOfLight.Myself is null)
                return Color.Transparent;

            return Projectile.GetAlpha(EmpressOfLight.Myself.As<EmpressOfLight>().Palette.MulticolorLerp(EmpressPaletteType.PrismaticBolt, 0.33f));
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            ManagedShader telegraphShader = ShaderManager.GetShader("WoTE.LanceWallTelegraphShader");
            telegraphShader.Apply();

            List<Vector2> wallPositions = Projectile.GetLaserControlPoints(6, 4000f, -Vector2.UnitY);

            PrimitiveSettings settings = new(TelegraphWidthFunction, TelegraphColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 40f, Pixelate: true, Shader: telegraphShader);
            PrimitiveRenderer.RenderTrail(wallPositions, settings, 25);
        }
    }
}
