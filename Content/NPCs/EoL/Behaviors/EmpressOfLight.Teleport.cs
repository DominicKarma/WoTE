using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        private bool performTeleport;

        /// <summary>
        /// How long it'll take the current teleport to elapse.
        /// </summary>
        public int TeleportDuration
        {
            get;
            set;
        }

        /// <summary>
        /// The completion ratio of the teleport.
        /// </summary>
        public float TeleportCompletionRatio
        {
            get;
            set;
        }

        /// <summary>
        /// The Empress' teleport destination.
        /// </summary>
        public Vector2 TeleportDestination
        {
            get;
            set;
        }

        /// <summary>
        /// How long it takes a standard teleport to completely elapse.
        /// </summary>
        public static int DefaultTeleportDuration => Utilities.SecondsToFrames(0.5f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Teleport()
        {
            StateMachine.RegisterTransition(EmpressAIType.Teleport, null, false, () =>
            {
                return TeleportCompletionRatio >= 1f;
            }, () =>
            {
                TeleportCompletionRatio = 0f;
                NPC.oldRot = new float[NPC.oldRot.Length];
                NPC.oldPos = new Vector2[NPC.oldPos.Length];
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.Teleport, DoBehavior_Teleport);
            StateMachine.ApplyToAllStatesExcept(state =>
            {
                StateMachine.RegisterTransition(state, EmpressAIType.Teleport, true, () => performTeleport, () =>
                {
                    performTeleport = false;
                });
            });
        }

        /// <summary>
        /// Performs the Empress' teleport state.
        /// </summary>
        public void DoBehavior_Teleport()
        {
            NPC.velocity *= 0.85f;
            NPC.rotation = MathHelper.Lerp(NPC.rotation, NPC.velocity.X * 0.001f, 0f);
            DashAfterimageInterpolant *= 0.7f;

            TeleportCompletionRatio = Utilities.InverseLerp(0f, TeleportDuration, AITimer);

            for (int i = 0; i < 8; i++)
            {
                Vector2 spawnCenter = Main.rand.NextBool(TeleportCompletionRatio) ? TeleportDestination : NPC.Center;
                Vector2 lightSpawnPosition = spawnCenter + Main.rand.NextVector2Square(-NPC.width, NPC.width);
                Dust light = Dust.NewDustPerfect(lightSpawnPosition, 261);
                light.velocity = -Vector2.UnitY * Main.rand.NextFloat(4f);
                light.color = Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.75f);
                light.noGravity = true;
            }

            if (TeleportCompletionRatio >= 1f)
            {
                ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 5f);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.velocity = Vector2.Zero;
                    NPC.Center = TeleportDestination;
                    NPC.netUpdate = true;
                }
            }

            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.PointingUp;
        }

        /// <summary>
        /// Instructs the Empress to teleport to a given general location.
        /// </summary>
        /// <param name="teleportDestination">The position the Empress will attempt to teleport to.</param>
        /// <param name="teleportDuration">How long the teleport duration should be. When null, default to <see cref="DefaultTeleportDuration"/>.</param>
        public void TeleportTo(Vector2 teleportDestination, int? teleportDuration = null)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 5f);

            performTeleport = true;
            TeleportDestination = teleportDestination;
            TeleportCompletionRatio = 0f;
            TeleportDuration = teleportDuration ?? DefaultTeleportDuration;
            NPC.netUpdate = true;
        }
    }
}
