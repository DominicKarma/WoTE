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
        /// How long it takes for Terraprismas summoned by the Empress to undo their 3D orbit squish visual and transition to a purely 2D spin.
        /// </summary>
        public static int ConvergingTerraprismas_OrbitSquishDissipateTime => Utilities.SecondsToFrames(0.5f);

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

        /// <summary>
        /// The amount of Terraprisma instances the Empress summons for her Converging Terraprismas attack.
        /// </summary>
        public static int ConvergingTerraprismas_TerraprismaCount => 8;

        /// <summary>
        /// How long Terraprismas summoned by the Empress take to fade in.
        /// </summary>
        public static int ConvergingTerraprismas_TerraprismaFadeInTime => Utilities.SecondsToFrames(0.25f);

        /// <summary>
        /// The standard radius of Terraprismas summoned by the Empresss during her Converging Terraprismas attack relative to the target's center.
        /// </summary>
        public static float ConvergingTerraprismas_InitialRadius => 350f;

        /// <summary>
        /// The maximum radius of Terraprismas summoned by the Empresss during her Converging Terraprismas attack relative to the target's center. This value is reached as Terraprismas reel back in anticipation of their dash.
        /// </summary>
        public static float ConvergingTerraprismas_ReelBackRadius => 850f;

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

            if (AITimer == ConvergingTerraprismas_SpinTime - Utilities.SecondsToFrames(0.23f))
                SoundEngine.PlaySound(SoundID.Item162);

            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == 1)
            {
                int terraprismaCount = ConvergingTerraprismas_TerraprismaCount;
                float spinAngleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < terraprismaCount; i++)
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), Target.Center, Vector2.Zero, ModContent.ProjectileType<SpinningTerraprisma>(), TerraprismaDamage, 0f, -1, i / (float)terraprismaCount, MathHelper.TwoPi * i / terraprismaCount + spinAngleOffset);
            }
        }
    }
}
