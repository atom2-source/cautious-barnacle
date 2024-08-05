using System;
using StereoKit;

namespace StereoKitApp.UI
{
    // KnobDial: A circular UI control for adjusting values in a VR/AR environment.
    // This class implements a knob that can be grabbed and rotated by the user,
    // providing an intuitive way to adjust numeric values within a specified range.
    public class KnobDial
    {
        // Public properties for customization
        // These allow for flexible configuration of the knob's behavior and appearance
        public float Minimum { get; set; } = 0f;
        public float Maximum { get; set; } = 100f;
        public float Value { get; set; } = 50f;
        public float StartAngle { get; set; } = 135f;    // In degrees, 135 is top-left
        public float EndAngle { get; set; } = 405f;      // 405 allows for a full rotation plus overlap
        public float Radius { get; set; } = 0.02f;       // In meters
        public Color KnobColor { get; set; } = new Color(1, 1, 1);  // White
        public Color PointerColor { get; set; } = new Color(1, 0, 0); // Red

        // Internal state for interaction tracking
        private bool _isGrabbed;
        private float _grabAngleOffset;

        // Step: Main update method, handles interaction and rendering
        // This method should be called every frame for each KnobDial instance
        public void Step(Pose pose)
        {
            // Create a spherical boundary for interaction
            // We use a bounding box for simplicity, but a true sphere would be more accurate
            Bounds bounds = new Bounds(Vec3.Zero, new Vec3(Radius * 2));

            // Check for initial grab
            // We only check the right hand here. For ambidextrous support, we'd need to check both hands
            if (Input.Hand(Handed.Right).IsJustPinched &&
                bounds.Contains(pose.ToMatrix().Inverse * Input.Hand(Handed.Right).pinchPt))
            {
                _isGrabbed = true;
                // Store the offset to prevent the value from jumping when first grabbed
                _grabAngleOffset = GetAngleToHand(pose) - GetValueAngle();
            }

            // Check for grab release
            if (Input.Hand(Handed.Right).IsJustUnpinched)
            {
                _isGrabbed = false;
            }

            // Update value if grabbed
            // This provides immediate feedback as the user rotates their hand
            if (_isGrabbed)
            {
                float newAngle = GetAngleToHand(pose) - _grabAngleOffset;
                Value = AngleToValue(newAngle);
            }

            // Render the knob
            DrawKnob(pose);
        }

        // DrawKnob: Renders the knob and its pointer
        private void DrawKnob(Pose pose)
        {
            // Draw the main knob sphere
            // We use the default material for simplicity. For more complex visuals, consider using a custom shader
            Mesh.Sphere.Draw(Material.Default, Matrix.TRS(pose.position, pose.orientation, new Vec3(Radius)));

            // Calculate and draw the pointer
            float valueAngle = GetValueAngle();
            Vec3 pointerEnd = new Vec3(
                MathF.Cos(valueAngle) * Radius,
                MathF.Sin(valueAngle) * Radius,
                0
            );

            // Draw the pointer line
            // The line width (0.001f) is hardcoded. Consider making this configurable if needed
            Lines.Add(pose.position, pose.position + pose.orientation * pointerEnd, PointerColor, 0.001f);
        }

        // GetValueAngle: Converts the current value to an angle
        // This is used for positioning the pointer and for interaction calculations
        private float GetValueAngle()
        {
            // Normalize the value to a 0-1 range
            float t = (Value - Minimum) / (Maximum - Minimum);
            // Convert to radians within the start-end angle range
            return (StartAngle + t * (EndAngle - StartAngle)) * (MathF.PI / 180f);
        }

        // GetAngleToHand: Calculates the angle between the knob center and the hand
        // This is used to determine how much the user has rotated their hand
        private float GetAngleToHand(Pose pose)
        {
            Vec3 handPos = Input.Hand(Handed.Right).pinchPt;
            // Transform hand position to local space relative to the knob
            Vec3 localHandPos = pose.orientation.Inverse * (handPos - pose.position);
            // Calculate the angle using atan2
            return MathF.Atan2(localHandPos.y, localHandPos.x);
        }

        // AngleToValue: Converts an angle back to a value within the knob's range
        // This is used to update the value based on the hand's rotation
        private float AngleToValue(float angle)
        {
            // Normalize angle to 0-2PI range
            // This while loop approach works but could be optimized for performance if needed
            while (angle < 0) angle += 2 * MathF.PI;
            while (angle > 2 * MathF.PI) angle -= 2 * MathF.PI;

            // Convert to degrees and normalize to 0-1 range within start-end angles
            float angleDegrees = angle * (180f / MathF.PI);
            float t = (angleDegrees - StartAngle) / (EndAngle - StartAngle);

            // Convert normalized value back to the actual range
            return Minimum + t * (Maximum - Minimum);
        }
    }
}
