using Luminance.Common.StateMachines;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// Whether the Empress is currently summoning lance walls down from above.
        /// </summary>
        /// 
        /// <remarks>
        /// This is intended to be activated by the <see cref="EmpressAIType.LanceWallSupport"/> state, and linger into the one state that occurs afterwards, acting as a temporary support piece.
        /// </remarks>
        public bool PerformingLanceWallSupport
        {
            get;
            set;
        }

        /// <summary>
        /// The X position of the Empress' lance wall. Defaults to 0 if the wall is not active.
        /// </summary>
        public float LanceWallXPosition
        {
            get;
            set;
        }

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_LanceWallSupport()
        {
            StateMachine.RegisterTransition(EmpressAIType.LanceWallSupport, null, false, () =>
            {
                return AITimer >= 90;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.LanceWallSupport, DoBehavior_LanceWallSupport);
        }

        /// <summary>
        /// Performs the Empress' Lance Wall Support attack.
        /// </summary>
        public void DoBehavior_LanceWallSupport()
        {
            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PointingUp;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }

        public void DoBehavior_LanceWallSupport_HandlePostStateSupportBehaviors()
        {
        }
    }
}
