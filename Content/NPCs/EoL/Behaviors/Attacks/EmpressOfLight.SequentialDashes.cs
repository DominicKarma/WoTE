using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;
using WoTE.Content.Particles;
using WoTE.Content.Particles.Metaballs;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// The amount of dashes the Empress has performed so far during the Sequential Dashes attack.
        /// </summary>
        public ref float SequentialDashes_DashCounter => ref NPC.ai[0];

        /// <summary>
        /// The direction the Empress will attempt to dash in during the Sequential Dashes attack when she's ready.
        /// </summary>
        public ref float SequentialDashes_DashDirection => ref NPC.ai[1];

        /// <summary>
        /// How long the Empress spends redirecting during her Sequential Dashes attack.
        /// </summary>
        public static int SequentialDashes_RedirectTime => Utilities.SecondsToFrames(0.63f);

        /// <summary>
        /// How long the Empress spends dashing during her Sequential Dashes attack.
        /// </summary>
        public static int SequentialDashes_DashTime => Utilities.SecondsToFrames(0.34f);

        /// <summary>
        /// How long the Empress spends slowing down during her Sequential Dashes attack, after her dash.
        /// </summary>
        public static int SequentialDashes_SlowDownTime => Utilities.SecondsToFrames(0.15f);

        /// <summary>
        /// The amount of dashes that the Empress should perform during her Sequential Dashes attack before choosing a new attack. 
        /// </summary>
        public static int SequentialDashes_DashCount => 3;

        /// <summary>
        /// The speed of dashes performed by the Empress during her Sequential Dashes attack.
        /// </summary>
        public float SequentialDashes_DashSpeed => ByPhase(69f, 78f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_SequentialDashes()
        {
            StateMachine.RegisterTransition(EmpressAIType.SequentialDashes, null, false, () =>
            {
                return SequentialDashes_DashCounter >= SequentialDashes_DashCount;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.SequentialDashes, DoBehavior_SequentialDashes);
        }

        /// <summary>
        /// Performs the Empress' Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes()
        {
            if (AITimer <= SequentialDashes_RedirectTime)
            {
                DoBehavior_SequentialDashes_Redirect();

                if (AITimer == SequentialDashes_RedirectTime)
                    DoBehavior_SequentialDashes_PerformStartDashEffects();
            }
            else if (AITimer <= SequentialDashes_RedirectTime + SequentialDashes_DashTime)
            {
                DoBehavior_SequentialDashes_PerformDash();
            }
            else if (AITimer <= SequentialDashes_RedirectTime + SequentialDashes_DashTime + SequentialDashes_SlowDownTime)
            {
                NPC.velocity *= 0.7f;
                DashAfterimageInterpolant *= 0.81f;
            }
            else
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 boltVelocity = NPC.SafeDirectionTo(Target.Center).RotatedBy(MathHelper.TwoPi * i / 6f) * 1.3f;
                        Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, boltVelocity, ModContent.ProjectileType<StarBolt>(), StarBurstDamage, 0f, -1);
                    }
                }

                AITimer = 0;
                SequentialDashes_DashCounter++;
                TeleportTo(Target.Center + Main.rand.NextVector2CircularEdge(500f, 500f), (int)(DefaultTeleportDuration * 1.15f));
                NPC.netUpdate = true;
            }

            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;

            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
            NPC.rotation = NPC.velocity.X * 0.00175f;
        }

        /// <summary>
        /// Performs initial redirecting behaviors for the Empress' Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes_Redirect()
        {
            // Determine which direction the Empress should attempt to hover relative to the target.
            // The abs(x) * -0.985 calculation serves two functions:
            // 1. It ensures that the Empress always attempts to stay above the player, not below them.
            // 2. It makes the vertical hover offset regress to a horizontal one.
            Vector2 hoverOffsetDirection = Target.SafeDirectionTo(NPC.Center);
            hoverOffsetDirection.Y = MathF.Abs(hoverOffsetDirection.Y) * -0.985f;

            float backwardsWindUpOffset = Utilities.InverseLerp(-26f, -4f, SequentialDashes_RedirectTime - AITimer).Squared() * 400f;
            Vector2 hoverDestination = Target.Center + hoverOffsetDirection * new Vector2(400f, 345f) - NPC.SafeDirectionTo(Target.Center) * backwardsWindUpOffset;

            NPC.SmoothFlyNear(hoverDestination, 0.17f, 0.85f);
        }

        /// <summary>
        /// Performs frame-one effects for the Empress' dashes during the Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes_PerformStartDashEffects()
        {
            // Reset the old position and rotation arrays, to ensure that the dash starts off with a brand new
            // afterimage state.
            NPC.oldRot = new float[NPC.oldRot.Length];
            NPC.oldPos = new Vector2[NPC.oldPos.Length];

            // TODO -- Probably replace this with something better sometime later?
            SoundEngine.PlaySound(SoundID.Item160 with { MaxInstances = 0 });

            SequentialDashes_DashDirection = NPC.AngleTo(Target.Center);
            NPC.netUpdate = true;

            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 4f);
        }

        /// <summary>
        /// Performs during-dash effects for the Empress' during the Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes_PerformDash()
        {
            float dashInterpolant = Utilities.InverseLerp(0f, 6f, AITimer - SequentialDashes_RedirectTime);
            float targetDirectionErringInterpolant = Utilities.InverseLerp(0f, SequentialDashes_DashTime, AITimer - SequentialDashes_RedirectTime) * Utilities.InverseLerp(200f, 400f, NPC.Distance(Target.Center));
            Vector2 targetDirectionErring = NPC.SafeDirectionTo(Target.Center - Vector2.UnitY * 9f) * targetDirectionErringInterpolant;
            Vector2 idealVelocity = (SequentialDashes_DashDirection.ToRotationVector2() + targetDirectionErring * 0.66f) * SequentialDashes_DashSpeed;

            NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, dashInterpolant * 0.6f);
            NPC.damage = NPC.defDamage;
            DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, 1f, 0.3f);

            if (AITimer % 5 == 0)
                ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center + Main.rand.NextVector2Circular(75f, 75f) - NPC.velocity, Vector2.Zero, 120f, 1f, 0.1f, 0.016f);

            for (int i = 0; i < 5; i++)
            {
                Vector2 particleVelocity = -NPC.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(15f, 25f) + Main.rand.NextVector2Circular(3f, 3f);
                Color particleColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.67f) * 0.8f;
                BloomCircleParticle particle = new(NPC.Center + Main.rand.NextVector2Circular(80f, 80f), particleVelocity, Vector2.One * Main.rand.NextFloat(0.015f, 0.042f), Color.Wheat, particleColor, 120, 1.8f, 1.75f);
                particle.Spawn();
            }
        }
    }
}
