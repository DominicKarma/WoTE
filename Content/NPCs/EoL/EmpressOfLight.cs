using System.Collections.Generic;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        #region Custom Types and Enumerations

        public enum EmpressAIType
        {
            Awaken,

            // Intermediate states.
            Teleport,
            ResetCycle,

            // Useful count constant.
            Count
        }

        #endregion Custom Types and Enumerations

        #region Fields and Properties

        /// <summary>
        /// Private backing field for <see cref="Myself"/>.
        /// </summary>
        private static NPC myself;

        /// <summary>
        /// The current AI state the Empress is using. This uses the <see cref="StateMachine"/> under the hood.
        /// </summary>
        public EmpressAIType CurrentState
        {
            get
            {
                // Add the relevant phase cycle if it has been exhausted, to ensure that Nameless' attacks are cyclic.
                if ((StateMachine?.StateStack?.Count ?? 1) <= 0)
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[EmpressAIType.ResetCycle]);

                return StateMachine?.CurrentState?.Identifier ?? EmpressAIType.Awaken;
            }
        }

        /// <summary>
        /// The Empress' target.
        /// </summary>
        public Player Target => Main.player[NPC.target];

        /// <summary>
        /// The AI timer for the Empress.
        /// </summary>
        public ref int AITimer => ref StateMachine.CurrentState.Time;

        /// <summary>
        /// A shorthand accessor for the Empress NPC. Returns null if not currently present.
        /// </summary>
        public static NPC Myself
        {
            get
            {
                if (myself is not null && !myself.active)
                    return null;

                return myself;
            }
            internal set => myself = value;
        }

        public override string Texture => $"Terraria/Images/NPC_{NPCID.HallowBoss}";

        #endregion Fields and Properties

        #region Loading

        public override void SetStaticDefaults()
        {
            this.ExcludeFromBestiary();

            NPCID.Sets.BossHeadTextures[Type] = NPCID.Sets.BossHeadTextures[NPCID.HallowBoss];

            var npcToBossHead = (IDictionary<int, int>)typeof(NPCHeadLoader).GetField("npcToBossHead", Utilities.UniversalBindingFlags).GetValue(null);
            npcToBossHead[Type] = NPCID.Sets.BossHeadTextures[Type];

            Main.npcFrameCount[Type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 100f;
            NPC.damage = 300;
            NPC.width = 100;
            NPC.height = 100;
            NPC.defense = 100;
            NPC.SetLifeMaxByMode(200000, 300000, 400000);

            if (Main.expertMode)
                NPC.damage = 125;

            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.canGhostHeal = false;
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = null;
            NPC.value = Item.buyPrice(1, 0, 0, 0) / 5;
            NPC.netAlways = true;
            NPC.Opacity = 0f;
        }

        #endregion Loading

        #region AI
        public override void AI()
        {
            // Pick a target if the current one is invalid.
            if (Target.dead || !Target.active)
                NPC.TargetClosest();

            if (!NPC.WithinRange(Target.Center, 4600f))
                NPC.TargetClosest();

            // Hey bozo the player's gone. Leave.
            if (Target.dead || !Target.active)
            {
                NPC.active = false;
                return;
            }

            // Do not despawn.
            NPC.timeLeft = 7200;

            Myself = NPC;

            PerformPreUpdateResets();

            StateMachine.PerformBehaviors();
            StateMachine.PerformStateTransitionCheck();

            // Increment timers.
            AITimer++;

            // Emit light.
            Lighting.AddLight(NPC.Center, Vector3.One * NPC.Opacity);
        }

        /// <summary>
        /// Resets various things pertaining to the fight state prior to behavior updates.
        /// </summary>
        /// 
        /// <remarks>
        /// This serves as a means of ensuring that changes to the fight state are gracefully reset if something suddenly changes, while affording the ability to make changes during updates.<br></br>
        /// As a result, this alleviates behaviors AI states from the burden of having to assume that they may terminate at any time and must account for that to ensure that the state is reset.
        /// </remarks>
        public void PerformPreUpdateResets()
        {
            NPC.damage = NPC.defDamage;
            NPC.defense = NPC.defDefense;
            NPC.dontTakeDamage = false;
            NPC.immortal = false;
            NPC.ShowNameOnHover = true;
        }

        #endregion AI
    }
}
