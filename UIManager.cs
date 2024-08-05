// UIManager: Handles the user interface and interaction logic for our app.

using StereoKit;
using System;
using StereoKitApp.UI;

namespace Project2024
{
    public class UIManager
    {
        // References to core components for interaction.
        private ShapeManager shapeManager;

        // UI layout variables.
        private Pose windowPose = new Pose(0, 0, -0.5f, Quat.LookDir(0, 0, 1)); // Initial window position and orientation
        private Vec2 windowSize = new Vec2(0.5f, 0.5f); // Initial window size
        private UIPage currentPage = UIPage.Main;    // Tracks the currently active UI page

        // Input tracking variables.
        private Vec3 moveAmount = Vec3.Zero;
        private float resizeAmount = 0f;
        private Vec3 rotationAngles = Vec3.Zero;

        // Custom UI colors for a distinct look.
        private readonly Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);  // Dark gray background
        private readonly Color headerColor = new Color(0.6f, 0.4f, 0.2f, 1f);     // Orange-ish header
        private readonly Color buttonColor = new Color(0.3f, 0.3f, 0.3f, 1f);     // Darker gray buttons
        private readonly Color textColor = new Color(0.9f, 0.9f, 0.9f, 1f);     // Light gray text
        private readonly Color knobColor = new Color(0.8f, 0.3f, 0.1f, 1f);     // Reddish knobs

        // Custom UI elements for rotation controls.
        private KnobDial pitchKnob, yawKnob, rollKnob;

        // Constructor: Initializes the UIManager and sets up the knob controls.
        public UIManager(ShapeManager shapeManager)
        {
            this.shapeManager = shapeManager;
            InitializeKnobs(); // Set up our custom knob controls
        }

        // Initializes the knob dials with their min/max values and color.
        private void InitializeKnobs()
        {
            // Note: These are separate knob objects for each axis.
            pitchKnob = new KnobDial() { Minimum = -180f, Maximum = 180f, KnobColor = knobColor };
            yawKnob = new KnobDial() { Minimum = -180f, Maximum = 180f, KnobColor = knobColor };
            rollKnob = new KnobDial() { Minimum = -180f, Maximum = 180f, KnobColor = knobColor };
        }

        // Renders the UI, switching between pages as needed.
        public void RenderUI()
        {
            // Apply our custom color theme to the UI elements.
            SetCustomTheme();

            // Start drawing the main UI window.
            UI.WindowBegin("Control Panel", ref windowPose, windowSize);

            // Draw the header with a custom color tint.
            UI.PushTint(headerColor);
            UI.Label("Control Panel");
            UI.PopTint();
            UI.HSeparator();

            // Render the appropriate page based on the current selection.
            switch (currentPage)
            {
                case UIPage.Main:
                    RenderMainPage();
                    break;
                case UIPage.MovementControls:
                    RenderMovementControlsPage();
                    break;
            }

            // End the UI window.
            UI.WindowEnd();
        }

        // Applies a custom theme to UI elements.
        private void SetCustomTheme()
        {
            Default.MaterialUI.SetColor("color", buttonColor);
            Default.MaterialUI.SetColor("background", backgroundColor);
            Default.MaterialUI.SetColor("text", textColor);
        }

        // Renders the main control panel page.
        private void RenderMainPage()
        {
            // Buttons to spawn/despawn the cube and math shape.
            if (UI.Button("Spawn Cube")) shapeManager.ToggleCube();
            if (UI.Button("Spawn Math Shape")) shapeManager.ToggleMathShape();

            // Button to navigate to the movement controls page.
            if (UI.Button("Movement Controls")) currentPage = UIPage.MovementControls;

            // Toggle for showing axis indicators on the shapes.
            bool showAxisIndicators = shapeManager.ShowAxisIndicators;
            if (UI.Toggle("Show Axis Indicators", ref showAxisIndicators))
            {
                shapeManager.ShowAxisIndicators = showAxisIndicators;
            }
        }

        // Renders the movement controls page with buttons and knobs.
        private void RenderMovementControlsPage()
        {
            UI.Label("Movement Controls");

            // Simplified directional buttons (up/down/left/right/forward/backward).
            if (UI.Button("Up")) moveAmount.y += 0.01f;
            if (UI.Button("Down")) moveAmount.y -= 0.01f;
            UI.SameLine();
            if (UI.Button("Left")) moveAmount.x -= 0.01f;
            UI.SameLine();
            if (UI.Button("Right")) moveAmount.x += 0.01f;
            if (UI.Button("Forward")) moveAmount.z += 0.01f;
            UI.SameLine();
            if (UI.Button("Backward")) moveAmount.z -= 0.01f;

            // Apply the accumulated movement to the active shape.
            if (moveAmount.x != 0 || moveAmount.y != 0 || moveAmount.z != 0)
            {
                shapeManager.MoveActiveShape(moveAmount);
                moveAmount = Vec3.Zero; // Reset for the next frame
            }

            UI.HSeparator();
            UI.Label("Rotation Controls");

            // Rotation knobs for pitch, yaw, and roll.
            float knobSpacing = 0.12f;
            float knobZ = -0.3f;
            float baseX = -0.2f;

            pitchKnob.Step(new Pose(baseX + 0 * knobSpacing, 0, knobZ));
            UI.SameLine();
            UI.Label("X-Axis (Pitch)");

            yawKnob.Step(new Pose(baseX + 1 * knobSpacing, 0, knobZ));
            UI.SameLine();
            UI.Label("Y-Axis (Yaw)");

            rollKnob.Step(new Pose(baseX + 2 * knobSpacing, 0, knobZ));
            UI.SameLine();
            UI.Label("Z-Axis (Roll)");

            // Apply rotation from knobs to the active shape.
            rotationAngles.x = pitchKnob.Value * (float)Math.PI / 180f;
            rotationAngles.y = yawKnob.Value * (float)Math.PI / 180f;
            rotationAngles.z = rollKnob.Value * (float)Math.PI / 180f;

            if (rotationAngles.x != 0 || rotationAngles.y != 0 || rotationAngles.z != 0)
            {
                shapeManager.RotateActiveShape(rotationAngles);
                // Reset knob values after applying rotation
                pitchKnob.Value = 0f;
                yawKnob.Value = 0f;
                rollKnob.Value = 0f;
                rotationAngles = Vec3.Zero;
            }

            UI.HSeparator();
            UI.Label("Resize");

            // Slider for resizing the active shape.
            UI.PushTint(knobColor);
            if (UI.HSlider("resize_slider", ref resizeAmount, -0.1f, 0.1f, 0f))
            {
                shapeManager.ResizeActiveShape(1 + resizeAmount);
                resizeAmount = 0f;
            }
            UI.PopTint();

            // Button to return to the main menu.
            if (UI.Button("Back to Main")) currentPage = UIPage.Main;
        }
    }

    // Enum to track UI pages.
    public enum UIPage
    {
        Main,
        MovementControls
    }
}
