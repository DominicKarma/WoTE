using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;

namespace WoTE.Common.ShapeCurves
{
    public class LeapfrogMethodSolver
    {
        private Vector2 previousInputPosition;

        private Vector2 outputPosition;

        private Vector2 outputVelocityHalf;

        /// <summary>
        /// The frequency of motion resulting from the system.
        /// </summary>
        public float Frequency
        {
            get;
            set;
        }

        /// <summary>
        /// Determines how strongly vibrations resulting from <see cref="Frequency"/> decay.
        /// </summary>
        /// 
        /// <remarks>
        /// At 0, no damping occurs, and the results oscillate forever.<br></br>
        /// Between 0 and 1, the vibrations are weakened, in accordance with the intensity.<br></br>
        /// Beyond 1, vibrations cease entirely, with the greater values resulting in a greater propensity for smooth regression to the norm.
        /// </remarks>
        public float DampingCoefficient
        {
            get;
            set;
        }

        /// <summary>
        /// Determines the initial response of the system.
        /// </summary>
        /// 
        /// <remarks>
        /// At 0, a smooth, natural curve occurs at the threshold of change.<br></br>
        /// Between 0 and 1 the motion becomes harsh and discontinuous at the first derivative, with strength relative to the intensity of the value.<br></br>
        /// Beyond 1, the motion overshoots a bit.<br></br>
        /// Below 0, the motion anticipates a bit.
        /// </remarks>
        public float InitialResponseCoefficient
        {
            get;
            set;
        }

        /// <summary>
        /// k1 as defined in the differential equation that this attemps to solve.
        /// </summary>
        public float K1 => DampingCoefficient / (MathHelper.Pi * Frequency);

        /// <summary>
        /// k2 as defined in the differential equation that this attemps to solve.
        /// </summary>
        public float K2 => 1f / (MathHelper.TwoPi * Frequency).Squared();

        /// <summary>
        /// k3 as defined in the differential equation that this attemps to solve.
        /// </summary>
        public float K3 => InitialResponseCoefficient * DampingCoefficient / (MathHelper.TwoPi * Frequency);

        /// <summary>
        /// The time-step of the solver. This is constant because Terraria's update rate is limited via a fixed time-step.
        /// </summary>
        public const float TimeStep = 0.016667f;

        /// <summary>
        /// The squared time-step of the solver.
        /// </summary>
        public const float TimeStepSquared = TimeStep * TimeStep;

        public LeapfrogMethodSolver(float frequency, float dampingCoefficient, float initialResponseCoefficient)
        {
            Frequency = frequency;
            DampingCoefficient = dampingCoefficient;
            InitialResponseCoefficient = initialResponseCoefficient;
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

            // Estimate the velocity at time i + 0.5.
            // In this context, OutputVelocityHalf is the previous estimate, at time i - 0.5.
            Vector2 velocityHalf = outputVelocityHalf + (inputPosition - outputPosition - K1 * outputVelocityHalf) / K2 * TimeStep * 0.5f;

            // Calculate the new position estimate.
            Vector2 newPosition = outputPosition + velocityHalf * TimeStep;

            // Iterate to the next frame.
            Vector2 newVelocityHalf = velocityHalf + (inputPosition + inputVelocity.Value * K3 - newPosition - K1 * velocityHalf) / K2 * TimeStep * 0.5f;
            outputPosition = newPosition;
            outputVelocityHalf = newVelocityHalf;

            return outputPosition;
        }
    }
}
