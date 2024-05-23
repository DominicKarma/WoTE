using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// The amount of terraprismas to summon during the Empress' enraged state.
        /// </summary>
        public static int Enraged_TerraprismaCount => 15;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Enraged()
        {
            StateMachine.RegisterStateBehavior(EmpressAIType.Enraged, DoBehavior_Enraged);
        }

        /// <summary>
        /// Performs the Enraged state attack.
        /// </summary>
        public void DoBehavior_Enraged()
        {
            Vector2 idealVelocity = NPC.SafeDirectionTo(Target.Center - Vector2.UnitY * 400f) * 23f;
            NPC.SimpleFlyMovement(idealVelocity, 0.67f);
            NPC.rotation = NPC.rotation.AngleLerp(NPC.velocity.X * 0.003f, 0.2f);
            NPC.dontTakeDamage = true;

            if (AITimer % 16 == 15)
                SoundEngine.PlaySound(SoundID.Item162 with { MaxInstances = 0 });

            ScreenShakeSystem.StartShake(1.3f);

            // Release absurd quantities of terraprismas at first.
            if (AITimer % 45 == 1)
                DoBehavior_Enraged_ReleaseTerraprismas();

            // Release light lances from above after 10 seconds.
            if (AITimer >= 600)
                DoBehavior_Enraged_ReleaseLanceWalls();

            // Release basically unavoidable prismatic bolts after 20 seconds.
            if (AITimer >= 1200)
                DoBehavior_Enraged_ReleaseAbsurdBolts();

            // Just kill the player after 60 seconds.
            if (AITimer >= 3600)
                Target.KillMe(PlayerDeathReason.ByNPC(NPC.whoAmI), 999999, 0);
        }

        /// <summary>
        /// Makes the Empress release a ring of Terraprismas around the target during her Enraged state.
        /// </summary>
        public void DoBehavior_Enraged_ReleaseTerraprismas()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int terraprismaCount = Enraged_TerraprismaCount;
            float spinAngleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < terraprismaCount; i++)
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), Target.Center, Vector2.Zero, ModContent.ProjectileType<SpinningTerraprisma>(), EnragedProjectileDamage, 0f, -1, i / (float)terraprismaCount, MathHelper.TwoPi * i / terraprismaCount + spinAngleOffset);
        }

        /// <summary>
        /// Makes the Empress release a torrent of lance walls above the target during her Enraged state.
        /// </summary>
        public void DoBehavior_Enraged_ReleaseLanceWalls()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 2; i++)
            {
                float lanceHue = (AITimer / 30f + Main.rand.NextFloat(0.23f)) % 1f;
                Vector2 horizontalSpawnOffset = Vector2.UnitX * Main.rand.NextFloatDirection() * LanceWallSupport_WallWidth;
                Vector2 verticalSpawnOffset = -Vector2.UnitY * Main.rand.NextFloat(975f, 1100f);
                Vector2 lanceSpawnPosition = Target.Center + horizontalSpawnOffset + verticalSpawnOffset;
                Vector2 lanceVelocity = Vector2.UnitY * 80f;
                if (lanceSpawnPosition.Y < 150f)
                    lanceSpawnPosition.Y = 150f;

                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), lanceSpawnPosition, lanceVelocity, ModContent.ProjectileType<LightLance>(), EnragedProjectileDamage, 0f, -1, 20f, lanceHue, 1f);
            }
        }

        /// <summary>
        /// Makes the Empress release a comical amount of prismatic bolts from her hand during her Enraged state.
        /// </summary>
        public void DoBehavior_Enraged_ReleaseAbsurdBolts()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 handPosition = NPC.Center + new Vector2(30f, -64f).RotatedBy(NPC.rotation);
            Vector2 boltVelocity = (MathHelper.TwoPi * AITimer / 45f).ToRotationVector2() * 50f;
            Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), handPosition, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target);
        }
    }
}
