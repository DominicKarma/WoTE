using System;
using Luminance.Common.Easings;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.Particles.Metaballs;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// How long the Empress waits before releasing lance walls from below during her Upward Lance Firing attack.
        /// </summary>
        public static int ReleaseLanceWallsFromBelow_WallReleaseDelay => Utilities.SecondsToFrames(1f);

        /// <summary>
        /// How rate at which the Empress waits releases lance walls from below during her Upward Lance Firing attack.
        /// </summary>
        public static int ReleaseLanceWallsFromBelow_WallReleaseRate => Utilities.SecondsToFrames(0.9f);

        /// <summary>
        /// How amount of time lances fired by the Empress during her Upward Lance Firing attack spend telegraphing thing direction.
        /// </summary>
        public static int ReleaseLanceWallsFromBelow_WallTelegraphTime => Utilities.SecondsToFrames(0.7f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_ReleaseLanceWallsFromBelow()
        {
            StateMachine.RegisterTransition(EmpressAIType.ReleaseLanceWallsFromBelow, null, false, () =>
            {
                return AITimer >= 300;
            }, () =>
            {
                TeleportTo(Target.Center - Vector2.UnitY * 350f);
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.ReleaseLanceWallsFromBelow, DoBehavior_ReleaseLanceWallsFromBelow);
        }

        /// <summary>
        /// Performs the Empress' Upward Lance Firing attack.
        /// </summary>
        public void DoBehavior_ReleaseLanceWallsFromBelow()
        {
            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PalmRaisedUp;

            NPC.Opacity = Utilities.InverseLerp(20f, 0f, AITimer);

            if (AITimer >= 15 && AITimer <= 20)
                ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center + Main.rand.NextVector2Circular(50f, 50f), Vector2.Zero, 30f, 0.75f, 0.25f, 0.02f);

            float idealVerticalSpeed = EasingCurves.Circ.Evaluate(EasingType.In, -2f, -60f, 1f - NPC.Opacity);
            NPC.velocity.X *= 0.9f;
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, idealVerticalSpeed, 0.2f);

            if (AITimer >= ReleaseLanceWallsFromBelow_WallReleaseDelay)
            {
                NPC.velocity = Vector2.Zero;
                NPC.Center = Target.Center - Vector2.UnitY * 1100f;
                NPC.dontTakeDamage = true;

                if (AITimer % ReleaseLanceWallsFromBelow_WallReleaseRate == ReleaseLanceWallsFromBelow_WallReleaseRate - 1)
                {
                    SoundEngine.PlaySound(SoundID.Item162 with { MaxInstances = 0 }, Target.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int index = 0;
                        for (float dx = -2000f; dx < 2000f; dx += (MathF.Abs(dx) <= 150f ? 0.2f : 1f) * Main.rand.NextFloat(80f, 215f))
                        {
                            float hue = (Utilities.InverseLerp(-2000f, 2000f, dx) * 2f + AITimer / 300f) % 1f;
                            Vector2 lanceSpawnPosition = Target.Center + new Vector2(dx, 900f);
                            Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), lanceSpawnPosition, -Vector2.UnitY, ModContent.ProjectileType<LightLance>(), 160, 0f, -1, ReleaseLanceWallsFromBelow_WallTelegraphTime, hue, index);
                            index++;
                        }
                    }
                }
            }

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }
    }
}
