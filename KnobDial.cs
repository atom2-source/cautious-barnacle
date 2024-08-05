// A UI control for adjusting values with a circular knob in StereoKit.

using System;
using StereoKit;

namespace StereoKitApp.UI
{
    public class KnobDial
    {
        // Public Properties: Customizable range, angles, size, and colors.
        public float Minimum { get; set; } = 0f;          // Min value (defaults to 0)
        public float Maximum { get; set; } = 100f;       // Max value (defaults to 100)
        public float Value { get; set; } = 50f;          // Current value (defaults to halfway)
        public float StartAngle { get; set; } = 135f;    // Start angle in degrees (135 is top-left)
        public float EndAngle { get; set; } = 405f;      // End angle in degrees (405 is a full circle + start)
        public float Radius { get; set; } = 0.02f;       // Radius of the knob
        public Color KnobColor { get; set; } = new Color(1, 1, 1);  // White by default
        public Color PointerColor { get; set; } = new Color(1, 0, 0); // Red by default

        // Internal State: Tracking grab interaction.
        private bool _isGrabbed;            // Flag if the knob is currently grabbed
        private float _grabAngleOffset;    // Angle offset when the grab started

        // Main Update Method: Handles knob interaction and rendering.
        public void Step(Pose pose) // 'pose' is the position and orientation of the knob.
        {
            // Create a bounding box for interaction.
            Bounds bounds = new Bounds(Vec3.Zero, new Vec3(Radius * 2));

            // Check for initial grab (pinch gesture inside the knob's bounds).
            if (Input.Hand(Handed.Right).IsJustPinched && bounds.Contains(pose.ToMatrix().Inverse * Input.Hand(Handed.Right).pinchPt))
            {
                _isGrabbed = true;
                _grabAngleOffset = GetAngleToHand(pose) - GetValueAngle(); // Store the initial offset
            }

            // Check for grab release (unpinch gesture).
            if (Input.Hand(Handed.Right).IsJustUnpinched)
            {
                _isGrabbed = false;
            }

            // Update the knob's value if it's being grabbed.
            if (_isGrabbed)
            {
                float newAngle = GetAngleToHand(pose) - _grabAngleOffset;
                Value = AngleToValue(newAngle);  // Clamp the value to the min/max range
            }

            // Draw the knob's visual elements.
            DrawKnob(pose);
        }

        // Draws the knob's sphere and pointer line.
        private void DrawKnob(Pose pose)
        {
            // Draw the main knob sphere.
            Mesh.Sphere.Draw(Material.Default, Matrix.TRS(pose.position, pose.orientation, new Vec3(Radius)));

            // Calculate the endpoint of the pointer based on the current value.
            float valueAngle = GetValueAngle();
            Vec3 pointerEnd = new Vec3(
                MathF.Cos(valueAngle) * Radius,
                MathF.Sin(valueAngle) * Radius,
                0
            );

            // Draw a line from the center of the knob to the pointer endpoint.
            Lines.Add(pose.position, pose.position + pose.orientation * pointerEnd, PointerColor, 0.001f);
        }

        // Converts the current Value to an angle within the knob's angular range.
        private float GetValueAngle()
        {
            float t = (Value - Minimum) / (Maximum - Minimum); // Normalized value (0 to 1)
            return (StartAngle + t * (EndAngle - StartAngle)) * (MathF.PI / 180f); // Angle in radians
        }

        // Calculates the angle (in radians) from the knob's center to the hand's position.
        private float GetAngleToHand(Pose pose)
        {
            Vec3 handPos = Input.Hand(Handed.Right).pinchPt; // Get hand position
            Vec3 localHandPos = handPos - pose.position; // Make it relative to the knob
            localHandPos = pose.orientation.Inverse * localHandPos; // Rotate to knob's local space
            return MathF.Atan2(localHandPos.y, localHandPos.x); // Angle in radians
        }

        // Converts an angle (in radians) to a value within the knob's min/max range.
        private float AngleToValue(float angle)
        {
            // Normalize the angle to 0 to 2*PI.
            while (angle < 0) angle += 2 * MathF.PI;
            while (angle > 2 * MathF.PI) angle -= 2 * MathF.PI;

            // Convert angle to degrees for easier interpolation.
            float angleDegrees = angle * (180f / MathF.PI);
            float t = (angleDegrees - StartAngle) / (EndAngle - StartAngle); // Normalized angle

            // Convert back to value within the range.
            return Minimum + t * (Maximum - Minimum);
        }
    }
}
