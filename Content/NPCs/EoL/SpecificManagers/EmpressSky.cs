using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public class EmpressSky : CustomSky
    {
        private bool skyActive;

        /// <summary>
        /// The opacity of this sky.
        /// </summary>
        public static new float Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// The identifier key for this sky.
        /// </summary>
        public const string SkyKey = "WoTE:EmpressSky";

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            // Prevent drawing beyond the back layer.
            if (maxDepth >= float.MaxValue && minDepth < float.MaxValue)
            {
                Texture2D sky = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/SpecificManagers/EmpressSky").Value;

                Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
                Rectangle skyRectangle = new(0, 0, (int)screenSize.X, (int)screenSize.Y);
                Main.spriteBatch.Draw(sky, skyRectangle, Color.White * Opacity * 0.4f);

                Texture2D moon = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/SpecificManagers/TheMoonFromInfernum").Value;
                Vector2 moonDrawPosition = Main.ScreenSize.ToVector2() * new Vector2(0.5f, 0.15f);

                Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
                Main.spriteBatch.Draw(bloom, moonDrawPosition, null, Color.Silver with { A = 0 } * 0.6f, 0f, bloom.Size() * 0.5f, 2f, 0, 0f);
                Main.spriteBatch.Draw(bloom, moonDrawPosition, null, Color.Silver with { A = 0 } * 0.31f, 0f, bloom.Size() * 0.5f, 3.3f, 0, 0f);
                Main.spriteBatch.Draw(bloom, moonDrawPosition, null, new Color(17, 172, 209, 0) * 0.15f, 0f, bloom.Size() * 0.5f, 6f, 0, 0f);

                Main.spriteBatch.Draw(moon, moonDrawPosition, null, new(200, 238, 235, 75), 0f, moon.Size() * 0.5f, 0.16f, 0, 0f);

                Matrix backgroundMatrix = Main.BackgroundViewMatrix.TransformationMatrix;
                Vector3 translationDirection = new(1f, Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically) ? -1f : 1f, 1f);
                backgroundMatrix.Translation -= Main.BackgroundViewMatrix.ZoomMatrix.Translation * translationDirection;

                // Draw clouds.
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, backgroundMatrix);
                DrawClouds();

                // Return to standard drawing.
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, backgroundMatrix);
            }
        }

        private static void DrawClouds()
        {
            Texture2D clouds = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/SpecificManagers/CloudTexture").Value;
            ManagedShader cloudShader = ShaderManager.GetShader("WoTE.CloudBackgroundShader");
            cloudShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.PointWrap);
            cloudShader.TrySetParameter("baseTextureSize", clouds.Size());
            cloudShader.Apply();

            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
            Rectangle cloudsRectangle = new(0, 0, Main.screenWidth, (int)(screenSize.Y * 0.3f));

            Main.spriteBatch.Draw(clouds, cloudsRectangle, new Color(17, 172, 209, 128) * Opacity);
        }

        public override void Update(GameTime gameTime)
        {
            // Disable ambient sky objects like wyverns and eyes appearing in front of the background.
            if (Opacity >= 0.5f)
                SkyManager.Instance["Ambience"].Deactivate();

            Opacity = Utilities.Saturate(Opacity + skyActive.ToDirectionInt() * 0.05f);

            Main.numClouds = 75;

            if (!skyActive)
                ResetVariablesWhileInactive();
        }

        public void ResetVariablesWhileInactive()
        {
        }

        #region Boilerplate
        public override void Deactivate(params object[] args) => skyActive = false;

        public override void Reset() => skyActive = false;

        public override bool IsActive() => skyActive || Opacity > 0f;

        public override void Activate(Vector2 position, params object[] args) => skyActive = true;

        public override float GetCloudAlpha() => 1f - Opacity;
        #endregion Boilerplate
    }
}
