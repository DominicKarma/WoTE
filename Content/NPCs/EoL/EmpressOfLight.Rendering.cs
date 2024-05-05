using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public override void FindFrame(int frameHeight)
        {

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPosition = NPC.Center - screenPos;
            SpriteEffects direction = NPC.spriteDirection.ToSpriteDirection();

            DrawBackglow(drawPosition);
            DrawWings(drawPosition, lightColor);
            Main.EntitySpriteDraw(texture, drawPosition, NPC.frame, NPC.GetAlpha(lightColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, direction);
            DrawHands(drawPosition, lightColor, direction);
            return false;
        }

        public void DrawBackglow(Vector2 drawPosition)
        {
            Color rainbow = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.5f % 1f, 1f, 0.5f, 0);
            Texture2D backglow = MiscTexturesRegistry.BloomCircleSmall.Value;
            Main.EntitySpriteDraw(backglow, drawPosition, null, NPC.GetAlpha(Color.Wheat) with { A = 0 } * 0.25f, NPC.rotation, backglow.Size() * 0.5f, NPC.scale * 4.1f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, NPC.GetAlpha(Color.Wheat) with { A = 0 } * 0.67f, NPC.rotation, backglow.Size() * 0.5f, NPC.scale * 2.85f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, NPC.GetAlpha(rainbow) * 0.7f, NPC.rotation, backglow.Size() * 0.5f, NPC.scale * 1.5f, 0);
            Main.EntitySpriteDraw(backglow, drawPosition, null, NPC.GetAlpha(Color.Wheat) with { A = 0 }, NPC.rotation, backglow.Size() * 0.5f, NPC.scale * 0.8f, 0);
        }

        /// <summary>
        /// Draws the Empress' wings.
        /// </summary>
        /// <param name="drawPosition">The draw position of the wings.</param>
        /// <param name="lightColor">The color of light at the Empress' position.</param>
        public void DrawWings(Vector2 drawPosition, Color lightColor)
        {
            Texture2D wingsTexture = TextureAssets.Extra[ExtrasID.HallowBossWingsBack].Value;
            Texture2D wingsColorShapeTexture = TextureAssets.Extra[ExtrasID.HallowBossWings].Value;
            Rectangle wingsFrame = wingsTexture.Frame(1, 11, 0, (int)(Main.GlobalTimeWrappedHourly * 15f) % 11);

            Main.EntitySpriteDraw(wingsTexture, drawPosition, wingsFrame, NPC.GetAlpha(lightColor), NPC.rotation, wingsFrame.Size() * 0.5f, NPC.scale * 2f, 0);

            Main.spriteBatch.PrepareForShaders();

            ManagedShader gradientShader = ShaderManager.GetShader("WoTE.EmpressWingGradientShader");
            gradientShader.SetTexture(TextureAssets.Extra[ExtrasID.HallowBossGradient], 1, SamplerState.LinearWrap);
            gradientShader.Apply();

            Main.EntitySpriteDraw(wingsColorShapeTexture, drawPosition, wingsFrame, NPC.GetAlpha(Color.White), NPC.rotation, wingsFrame.Size() * 0.5f, NPC.scale * 2f, 0);
            Main.spriteBatch.ResetToDefault();
        }

        /// <summary>
        /// Draws the Empress' arms.
        /// </summary>
        /// <param name="drawPosition">The draw position of the arms.</param>
        /// <param name="lightColor">The color of light at the Empress' position.</param>
        /// <param name="lightColor">The Empress' direction.</param>
        public void DrawHands(Vector2 drawPosition, Color lightColor, SpriteEffects direction)
        {
            Texture2D leftHandTexture = TextureAssets.Extra[ExtrasID.HallowBossArmsLeft].Value;
            Rectangle leftHandFrame = leftHandTexture.Frame(1, 7, 0, (int)LeftHandFrame);
            Main.EntitySpriteDraw(leftHandTexture, drawPosition, leftHandFrame, NPC.GetAlpha(lightColor), NPC.rotation, leftHandFrame.Size() * 0.5f, NPC.scale, direction);

            Texture2D rightHandTexture = TextureAssets.Extra[ExtrasID.HallowBossArmsRight].Value;
            Rectangle rightHandFrame = rightHandTexture.Frame(1, 7, 0, (int)RightHandFrame);
            Main.EntitySpriteDraw(rightHandTexture, drawPosition, rightHandFrame, NPC.GetAlpha(lightColor), NPC.rotation, rightHandFrame.Size() * 0.5f, NPC.scale, direction);
        }
    }
}
