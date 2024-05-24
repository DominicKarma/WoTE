using System;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL;
using WoTE.Content.NPCs.EoL.Projectiles;
using WoTE.Content.Particles;

namespace WoTE.Content.Items
{
    public class SilverReleaseLanternProj : ModProjectile
    {
        /// <summary>
        /// How long this lantern has existed for, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[0];

        public override string Texture => "WoTE/Content/Items/SilverReleaseLantern";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 24;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 510;
        }

        public override void AI()
        {
            float windInfluence = Main.WindForVisuals;
            if (windInfluence == 0f)
                windInfluence = 0.1f;
            float horizontalAcceleration = windInfluence * 0.07f;
            float slowdownInterpolant = Utilities.InverseLerp(90f, 300f, Time) * 0.77f;
            float maxSpeed = (1f - slowdownInterpolant) * 3f;

            Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X + horizontalAcceleration, -maxSpeed, maxSpeed);

            if (Projectile.velocity.Y < -2f)
                Projectile.velocity.Y *= 0.99f;

            else if (slowdownInterpolant < 0.7f)
                Projectile.velocity.Y -= MathF.Cos(Time / 150f % 1f * MathHelper.TwoPi) * 0.05f;

            if (MathF.Abs(Projectile.velocity.Y) > maxSpeed)
                Projectile.velocity.Y *= 0.97f;

            Projectile.rotation = Projectile.velocity.X * 0.12f;

            DelegateMethods.v3_1 = new Vector3(0.85f, 1f, 1f);
            Utils.PlotTileLine(Projectile.Top, Projectile.Bottom, Projectile.width * Projectile.scale, DelegateMethods.CastLight);

            if (Time >= 150f)
            {
                Projectile.velocity *= MathHelper.Lerp(1f, 0.8f, Utilities.InverseLerp(150f, 240f, Time));

                if (Projectile.timeLeft >= 90)
                    ReleaseEnergyParticles();
            }

            Projectile.Opacity = Utilities.InverseLerp(0f, 9f, Projectile.timeLeft);

            if (Projectile.timeLeft == 40)
                SoundEngine.PlaySound(SoundID.Item161);

            if (Projectile.timeLeft == 8 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Utilities.NewProjectileBetter(Projectile.GetSource_Death(), Projectile.Center - Vector2.UnitY * 150f, Vector2.Zero, ModContent.ProjectileType<GlowingAurora>(), 0, 0f);
                NPC.NewNPC(Projectile.GetSource_Death(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<EmpressOfLight>(), 1);
            }

            Time++;
        }

        public void ReleaseEnergyParticles()
        {
            float energyParticleReleaseChance = Utilities.InverseLerp(150f, 240f, Time).Squared();
            EmpressPaletteSet palette = EmpressPalettes.Choose();

            for (int i = 0; i < 2; i++)
            {
                if (!Main.rand.NextBool(energyParticleReleaseChance))
                    continue;

                float energySpawnOffsetAngle = MathHelper.TwoPi * (Main.rand.Next(4) + 0.5f) / 4f + Main.rand.NextFloatDirection() * 0.2f;
                Color energyColor = palette.MulticolorLerp(EmpressPaletteType.LacewingTrail, Main.rand.NextFloat());
                Vector2 energySpawnPosition = Projectile.Center + energySpawnOffsetAngle.ToRotationVector2() * Main.rand.NextFloat(820f, 1100f);
                Vector2 energyVelocity = (Projectile.Center - energySpawnPosition).RotatedBy(MathHelper.PiOver2) * 0.011f;
                BloomPixelParticle bloom = new(energySpawnPosition, energyVelocity, Color.Wheat, energyColor, 180, Vector2.One * Main.rand.NextFloat(2f, 4f), () => Projectile.Center, Vector2.One * 0.036f, 4f);
                bloom.Spawn();
            }

            for (int i = 0; i < 3; i++)
            {
                if (!Main.rand.NextBool(energyParticleReleaseChance))
                    continue;

                Color energyColor = palette.MulticolorLerp(EmpressPaletteType.LacewingTrail, Main.rand.NextFloat());
                Vector2 energySpawnPosition = Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(820f, 1100f);
                Vector2 energyVelocity = (Projectile.Center - energySpawnPosition) * 0.042f;
                BloomPixelParticle bloom = new(energySpawnPosition, energyVelocity, Color.Wheat, energyColor, 180, Vector2.One * Main.rand.NextFloat(2f, 4f), null, Vector2.One * 0.023f);
                bloom.Spawn();
            }

            for (int i = 0; i < 3; i++)
            {
                if (!Main.rand.NextBool(energyParticleReleaseChance.Squared() * 0.1f))
                    continue;

                Color lacewingColor = palette.MulticolorLerp(EmpressPaletteType.LacewingTrail, Main.rand.NextFloat());
                Vector2 lacewingSpawnPosition = Projectile.Center + new Vector2(Main.rand.NextFloatDirection() * 1200f, 850f);
                Vector2 lacewingVelocity = -Vector2.UnitY.RotatedBy(0.4f) * Main.rand.NextFloat(11f, 30f);
                PrismaticLacewingParticle lacewing = new(lacewingSpawnPosition, lacewingVelocity, lacewingColor, Main.rand.Next(60, 150), Vector2.One, () => lacewingSpawnPosition + lacewingVelocity * 280f);
                lacewing.Spawn();
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return null;
        }

        public void DrawGleam(Vector2 drawPosition)
        {
            EmpressPaletteSet palette = EmpressPalettes.Choose();
            Texture2D flare = MiscTexturesRegistry.ShineFlareTexture.Value;
            Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;

            float glimmerInterpolant = Utilities.InverseLerp(8f, 48f, Projectile.timeLeft);
            float flareOpacity = Utilities.InverseLerp(1f, 0.75f, glimmerInterpolant);
            float flareScale = MathF.Pow(Utilities.Convert01To010(glimmerInterpolant), 1.4f) * 2.8f + 1.4f;
            float flareRotation = MathHelper.SmoothStep(0f, MathHelper.TwoPi, MathF.Pow(glimmerInterpolant, 0.2f)) + MathHelper.PiOver4;
            Vector2 flarePosition = drawPosition;

            Color flareColorA = palette.MulticolorLerp(EmpressPaletteType.PrismaticBolt, 0f);
            Color flareColorB = palette.MulticolorLerp(EmpressPaletteType.PrismaticBolt, 0.33f) * 1.6f;
            Color flareColorC = palette.MulticolorLerp(EmpressPaletteType.PrismaticBolt, 0.66f);

            Main.spriteBatch.Draw(bloom, flarePosition, null, flareColorA with { A = 0 } * flareOpacity * 0.3f, 0f, bloom.Size() * 0.5f, flareScale * 1.9f, 0, 0f);
            Main.spriteBatch.Draw(bloom, flarePosition, null, flareColorB with { A = 0 } * flareOpacity * 0.54f, 0f, bloom.Size() * 0.5f, flareScale, 0, 0f);
            Main.spriteBatch.Draw(flare, flarePosition, null, flareColorC with { A = 0 } * flareOpacity, flareRotation, flare.Size() * 0.5f, flareScale, 0, 0f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D backglow = ModContent.Request<Texture2D>("WoTE/Content/Items/SilverReleaseLanternGlow").Value;
            Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color lanternColor = Projectile.GetAlpha(lightColor);
            Color backglowColor = Projectile.GetAlpha(Main.dayTime ? Color.Orange : Color.DeepSkyBlue) with { A = 0 } * 0.6f;

            float bloomScale = (Time - 240f) * Projectile.Opacity * 0.017f;
            Color bloomColor = new Color(155, 255, 255, 0) * Utilities.Saturate(bloomScale);
            Main.EntitySpriteDraw(backglow, drawPosition, null, backglowColor, Projectile.rotation, backglow.Size() * 0.5f, Projectile.scale * 1.25f, 0);
            Main.EntitySpriteDraw(texture, drawPosition, null, lanternColor, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, 0);
            if (bloomScale > 0f)
            {
                Main.EntitySpriteDraw(bloom, drawPosition, null, bloomColor, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale * bloomScale, 0);
                Main.EntitySpriteDraw(bloom, drawPosition, null, bloomColor * 1.76f, Projectile.rotation, bloom.Size() * 0.5f, Projectile.scale * bloomScale * 0.6f, 0);
            }

            DrawGleam(drawPosition);

            return false;
        }
    }
}
