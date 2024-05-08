using Luminance.Common.DataStructures;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// How long it takes for Terraprismas summoned by the Empress to undo their squish 3D visual.
        /// </summary>
        public static int ConvergingTerraprismas_SquishDissipateTime => Utilities.SecondsToFrames(0.5f);

        /// <summary>
        /// How long Terraprismas summoned by the Empress spend spinning around.
        /// </summary>
        public static int ConvergingTerraprismas_SpinTime => Utilities.SecondsToFrames(0.9f);

        /// <summary>
        /// How long Terraprismas summoned by the Empress spend reeling back in anticipation of a dash.
        /// </summary>
        public static int ConvergingTerraprismas_ReelBackTime => Utilities.SecondsToFrames(0.5f);

        /// <summary>
        /// How long the Empress waits after the Converging Terraprismas attack to choose a new attack to use.
        /// </summary>
        public static int ConvergingTerraprismas_AttackTransitionDelay => Utilities.SecondsToFrames(0.4f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_ConvergingTerraprismas()
        {
            StateMachine.RegisterTransition(EmpressAIType.ConvergingTerraprismas, null, false, () =>
            {
                return AITimer >= ConvergingTerraprismas_SpinTime + ConvergingTerraprismas_ReelBackTime + ConvergingTerraprismas_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.ConvergingTerraprismas, DoBehavior_ConvergingTerraprismas);
        }

        /// <summary>
        /// Performs the Empress' Converging Terraprismas attack.
        /// </summary>
        public void DoBehavior_ConvergingTerraprismas()
        {
            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;

            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
            NPC.rotation = NPC.velocity.X * 0.00175f;
            NPC.velocity *= 0.95f;

            if (AITimer >= 3601)
                AITimer = 0;

            if (AITimer == ConvergingTerraprismas_SpinTime - Utilities.SecondsToFrames(0.23f))
                SoundEngine.PlaySound(SoundID.Item162);

            if (AITimer == 1 || (Main.mouseRight && Main.mouseRightRelease))
            {
                IProjOwnedByBoss<EmpressOfLight>.KillAll();

                int terraprismaCount = 7;
                for (int i = 0; i < terraprismaCount; i++)
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), Target.Center, Vector2.Zero, ModContent.ProjectileType<SpinningTerraprisma>(), 200, 0f, -1, i / (float)terraprismaCount, MathHelper.TwoPi * i / terraprismaCount);
            }
        }
    }
}
