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
        /// The direction that the Empress punched in upon releasing Terraprismas during her Orbit Released Terraprismas attack.
        /// </summary>
        public ref float OrbitReleasedTerraprismas_PunchDirection => ref NPC.ai[0];

        /// <summary>
        /// The amount of time the Terraprismas spin during the Empress' Orbit Released Terraprismas attack.
        /// </summary>
        public static int OrbitReleasedTerraprismas_TerraprismaSpinTime => Utilities.SecondsToFrames(1.7f);

        /// <summary>
        /// The amount of Terraprisma instances the Empress summons for her Orbit Released Terraprismas attack.
        /// </summary>
        public static int OrbitReleasedTerraprismas_TerraprismaCount => 9;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_OrbitReleasedTerraprismas()
        {
            StateMachine.RegisterTransition(EmpressAIType.OrbitReleasedTerraprismas, null, false, () =>
            {
                return AITimer >= 20 && !Utilities.AnyProjectiles(ModContent.ProjectileType<EmpressOrbitingTerraprisma>());
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.OrbitReleasedTerraprismas, DoBehavior_OrbitReleasedTerraprismas);
        }

        /// <summary>
        /// Performs the Empress' Dazzling Tornadoes attack.
        /// </summary>
        public void DoBehavior_OrbitReleasedTerraprismas()
        {
            LeftHandFrame = EmpressHandFrame.OpenHandDownwardArm;
            RightHandFrame = EmpressHandFrame.OpenHandDownwardArm;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.rotation.AngleLerp(MathHelper.Clamp(NPC.velocity.X * 0.0021f, -0.4f, 0.4f), 0.16f);

            if (AITimer == OrbitReleasedTerraprismas_TerraprismaSpinTime)
                DoBehavior_OrbitReleasedTerraprismas_PerformTerraprismaReleaseEffects();

            if (AITimer >= OrbitReleasedTerraprismas_TerraprismaSpinTime)
                DoBehavior_OrbitReleasedTerraprismas_HandlePostSwordDashBehavior();
            else
                DoBehavior_OrbitReleasedTerraprismas_FlyNearTarget();

            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == 1)
            {
                int terraprismaCount = OrbitReleasedTerraprismas_TerraprismaCount;
                float spinAngleOffset = Main.rand.NextFromList(0f, MathHelper.Pi / 3f, MathHelper.Pi * 0.667f);
                for (int i = 0; i < terraprismaCount; i++)
                {
                    int fireDelay = i * 3;
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), Target.Center, Vector2.Zero, ModContent.ProjectileType<EmpressOrbitingTerraprisma>(), TerraprismaDamage, 0f, -1, i / (float)terraprismaCount, MathHelper.TwoPi * i / terraprismaCount + spinAngleOffset, fireDelay);
                }
            }
        }

        /// <summary>
        /// Makes the Empress fly around the target during her Orbit Released Terraprismas attack, having the swords spin around like buzzsaws.
        /// </summary>
        public void DoBehavior_OrbitReleasedTerraprismas_FlyNearTarget()
        {
            DashAfterimageInterpolant *= 0.9f;
            NPC.Center = Vector2.Lerp(NPC.Center, Target.Center, 0.006f);
            NPC.SimpleFlyMovement(NPC.SafeDirectionTo(Target.Center) * 11f, 0.21f);

            float initialFlySpeedInterpolant = Utilities.InverseLerp(40f, 0f, AITimer);
            NPC.SmoothFlyNearWithSlowdownRadius(Target.Center + Vector2.UnitX * NPC.OnRightSideOf(Target.Center).ToDirectionInt() * 500f, initialFlySpeedInterpolant * 0.3f, 1f - initialFlySpeedInterpolant * 0.4f, 400f);
        }

        /// <summary>
        /// Makes the Empress perform effects during her Orbit Released Terraprismas attack that indicate that the Terraprismas will begin to fly outward, such as playing sounds.
        /// </summary>
        public void DoBehavior_OrbitReleasedTerraprismas_PerformTerraprismaReleaseEffects()
        {
            SoundEngine.PlaySound(SoundID.Item122);
            SoundEngine.PlaySound(SoundID.Item160);
            SoundEngine.PlaySound(SoundID.Item162);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                OrbitReleasedTerraprismas_PunchDirection = NPC.OnRightSideOf(Target).ToDirectionInt();
                NPC.netUpdate = true;

                Vector2 fistPosition = NPC.Center + new Vector2(OrbitReleasedTerraprismas_PunchDirection * -64f, 8f).RotatedBy(NPC.rotation);
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), fistPosition, Vector2.Zero, ModContent.ProjectileType<PrismaticBurst>(), 0, 0f);
            }
        }

        /// <summary>
        /// Makes the Empress perform effects during her Orbit Released Terraprismas attack that involve making her leave as her Terraprismas fly outward.
        /// </summary>
        public void DoBehavior_OrbitReleasedTerraprismas_HandlePostSwordDashBehavior()
        {
            RightHandFrame = EmpressHandFrame.FistedOutstretchedArm;
            if (OrbitReleasedTerraprismas_PunchDirection == 1f)
                (LeftHandFrame, RightHandFrame) = (RightHandFrame, LeftHandFrame);

            // Fly up and forward.
            NPC.velocity.X *= 1.033f;
            if (AITimer >= OrbitReleasedTerraprismas_TerraprismaSpinTime + 35 && NPC.velocity.Y >= -56f)
                NPC.velocity.Y -= 2.6f;

            DashAfterimageInterpolant = Utilities.InverseLerp(30f, 90f, AITimer - OrbitReleasedTerraprismas_TerraprismaSpinTime);
        }
    }
}
