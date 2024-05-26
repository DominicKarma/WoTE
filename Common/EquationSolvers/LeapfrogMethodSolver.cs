using System.IO;
using Microsoft.Xna.Framework;
using Terraria;

namespace WoTE.Common.EquationSolvers
{
    public class LeapfrogMethodSolver
    {
        private Vector2 previousInputPosition;

        private Vector2 outputVelocityHalf;

        /// <summary>
        /// The resulting output position from the last <see cref="Update(Vector2, Vector2?)"/> call, or from the constructor if it has yet to be called.
        /// </summary>
        public Vector2 OutputPosition
        {
            get;
            set;
        }

        /// <summary>
        /// The movement configuration that governs the system.
        /// </summary>
        public DynamicMovementConfiguration MovementConfiguration;

        /// <summary>
        /// The time-step of the solver. This is constant because Terraria's update rate is limited via a fixed time-step.
        /// </summary>
        public const float TimeStep = 0.016667f;

        /// <summary>
        /// The squared time-step of the solver.
        /// </summary>
        public const float TimeStepSquared = TimeStep * TimeStep;

        public LeapfrogMethodSolver(Vector2 startingPosition, DynamicMovementConfiguration config)
        {
            OutputPosition = startingPosition;
            MovementConfiguration = config;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(MovementConfiguration.K1);
            writer.Write(MovementConfiguration.K2);
            writer.Write(MovementConfiguration.K3);
            writer.WriteVector2(previousInputPosition);
            writer.WriteVector2(outputVelocityHalf);
            writer.WriteVector2(OutputPosition);
        }

        public static LeapfrogMethodSolver ReadFrom(BinaryReader reader)
        {
            float k1 = reader.ReadSingle();
            float k2 = reader.ReadSingle();
            float k3 = reader.ReadSingle();
            Vector2 previousInputPosition = reader.ReadVector2();
            Vector2 outputVelocityHalf = reader.ReadVector2();
            Vector2 outputPosition = reader.ReadVector2();
            return new(outputPosition, new(k1, k2, k3, 0f))
            {
                outputVelocityHalf = outputVelocityHalf,
                previousInputPosition = previousInputPosition,
            };
        }

        /// <summary>
        /// Updates the system via the Leapfrog method.
        /// </summary>
        /// <param name="inputPosition">The base, unmodified position.</param>
        /// <param name="inputVelocity">An optional override for the unmodified velocity. Is estimated if nothing is provided manually.</param>
        /// <returns>The output position imbued with characteristics defined by the various coefficients.</returns>
        public Vector2 Update(Vector2 inputPosition, Vector2? inputVelocity = null)
        {
            if (inputVelocity is null)
            {
                if (previousInputPosition == Vector2.Zero)
                    previousInputPosition = inputPosition;

                inputVelocity = (inputPosition - previousInputPosition) / TimeStep;
                previousInputPosition = inputPosition;
            }

            float k1 = MovementConfiguration.K1;
            float k2 = MovementConfiguration.K2;
            float k3 = MovementConfiguration.K3;
            if (k2 == 0f)
                return inputPosition;

            // Estimate the velocity at time i + 0.5.
            // In this context, OutputVelocityHalf is the previous estimate, at time i - 0.5.
            Vector2 velocityHalf = outputVelocityHalf + (inputPosition - OutputPosition - k1 * outputVelocityHalf) / k2 * TimeStep * 0.5f;

            // Calculate the new position estimate.
            Vector2 newPosition = OutputPosition + velocityHalf * TimeStep;

            // Iterate to the next frame.
            Vector2 newVelocityHalf = velocityHalf + (inputPosition + inputVelocity.Value * k3 - newPosition - k1 * velocityHalf) / k2 * TimeStep * 0.5f;
            OutputPosition = newPosition;
            outputVelocityHalf = newVelocityHalf;

            return OutputPosition;
        }

        /// <summary>
        /// Updates and applies this system to a given entity.
        /// </summary>
        /// <param name="entity">The entity to apply the system positioning to.</param>
        public void ApplyTo(Entity entity) => entity.Center = Update(Main.LocalPlayer.Center, entity.velocity);
    }
}
