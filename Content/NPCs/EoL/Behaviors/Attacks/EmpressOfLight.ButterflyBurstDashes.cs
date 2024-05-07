using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public static int ButterflyBurstDashes_RedirectTime => Utilities.SecondsToFrames(0.95f);

        public static int ButterflyBurstDashes_DashRepositionTime => Utilities.SecondsToFrames(0.12f);

        public static int ButterflyBurstDashes_DashTime => Utilities.SecondsToFrames(0.22f);

        public static int ButterflyBurstDashes_DashSlowdownTime => Utilities.SecondsToFrames(0.3f);

        public static int ButterflyBurstDashes_DashCount => 4;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_ButterflyBurstDashes()
        {
            StateMachine.RegisterTransition(EmpressAIType.ButterflyBurstDashes, null, false, () =>
            {
                return AITimer >= 25 && !NPC.AnyNPCs(ModContent.NPCType<Lacewing>());
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.ButterflyBurstDashes, DoBehavior_ButterflyBurstDashes);
        }

        /// <summary>
        /// Performs the Empress' Butterfly Burst Dashes attack.
        /// </summary>
        public void DoBehavior_ButterflyBurstDashes()
        {
            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;

            if (AITimer >= 20)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == 20)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        float offsetAngle = Main.rand.NextFloatDirection() * 0.6f;
                        if (i >= 16)
                            offsetAngle += MathHelper.Pi;

                        int lacewingIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Lacewing>(), NPC.whoAmI, offsetAngle);
                        if (lacewingIndex >= 0 && lacewingIndex < Main.maxNPCs)
                            Main.npc[lacewingIndex].velocity = Main.rand.NextVector2Circular(38f, 24f);
                    }
                }

                NPC.Opacity = Utilities.InverseLerp(32f, 0f, NPC.CountNPCS(ModContent.NPCType<Lacewing>()));
                if (NPC.Opacity <= 0f)
                    NPC.Center = Target.Center;

                NPC.hide = NPC.Opacity <= 0f;
                NPC.ShowNameOnHover = NPC.Opacity >= 0.4f;
            }

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }
    }
}
