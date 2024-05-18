using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public Vector2 EventideLances_UndirectionedBowOffset => new Vector2(-72f, -14f).RotatedBy(NPC.rotation);

        public ref float EventideLances_BowDirection => ref NPC.ai[0];

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_EventideLances()
        {
            StateMachine.RegisterTransition(EmpressAIType.EventideLances, null, false, () =>
            {
                return AITimer >= 99999999;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.EventideLances, DoBehavior_EventideLances);
        }

        /// <summary>
        /// Performs the Empress' RadialStarBurst attack.
        /// </summary>
        public void DoBehavior_EventideLances()
        {
            LeftHandFrame = EmpressHandFrame.FistedOutstretchedArm;
            RightHandFrame = EmpressHandFrame.UpwardGrip;

            NPC.spriteDirection = NPC.OnRightSideOf(Target).ToDirectionInt();
            NPC.rotation = NPC.velocity.X * 0.0035f;

            EventideLances_BowDirection = NPC.AngleTo(Target.Center);

            NPC.SmoothFlyNearWithSlowdownRadius(Target.Center, 0.1f, 0.9f, 376f);
        }

        public void DoBehavior_EventideLances_DrawBowString(Vector2 drawPosition)
        {
            if (CurrentState != EmpressAIType.EventideLances)
                return;

            Vector2 eventidePosition = drawPosition + EventideLances_UndirectionedBowOffset;

            float angleOffset = EventideLances_BowDirection * NPC.spriteDirection;
            if (NPC.spriteDirection == -1)
                angleOffset += MathHelper.Pi;

            Vector2 eventideTop = eventidePosition - Vector2.UnitY.RotatedBy(NPC.rotation + angleOffset) * 37f;
            Vector2 eventideBottom = eventidePosition + Vector2.UnitY.RotatedBy(NPC.rotation + angleOffset) * 37f;
            Vector2 stringEnd = drawPosition + new Vector2(2f, -36f).RotatedBy(NPC.rotation);

            Utils.DrawLine(Main.spriteBatch, eventideTop + Main.screenPosition, stringEnd + Main.screenPosition, Color.DeepSkyBlue, Color.Wheat, 2f);
            Utils.DrawLine(Main.spriteBatch, eventideBottom + Main.screenPosition, stringEnd + Main.screenPosition, Color.HotPink, Color.Wheat, 2f);
        }

        public void DoBehavior_EventideLances_DrawBow(Vector2 drawPosition)
        {
            if (CurrentState != EmpressAIType.EventideLances)
                return;

            float rotation = EventideLances_BowDirection * NPC.spriteDirection + NPC.rotation;
            if (NPC.spriteDirection == 1)
                rotation += MathHelper.Pi;

            Texture2D eventide = ModContent.Request<Texture2D>("WoTE/Content/NPCs/EoL/Rendering/Eventide").Value;
            Vector2 eventidePosition = drawPosition + EventideLances_UndirectionedBowOffset;
            Main.spriteBatch.Draw(eventide, eventidePosition, null, Color.White, rotation, eventide.Size() * 0.5f, NPC.scale * 1.5f, SpriteEffects.FlipHorizontally, 0f);
        }
    }
}
