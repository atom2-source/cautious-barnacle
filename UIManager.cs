using StereoKit;
using System;

namespace Project2024
{
    public class UIManager
    {
        // Core components
        private readonly ShapeManager shapeManager;
        private Pose windowPose = new Pose(0, 0, -0.5f, Quat.LookDir(0, 0, 1));
        private Vec2 windowSize = new Vec2(0.3f, 0.4f);
        private UIPage currentPage = UIPage.Main;

        // Movement and rotation tracking
        private Vec3 moveAmount = Vec3.Zero;
        private Vec3 rotationAngles = Vec3.Zero;

        // Window positioning and resetting
        private Vec3 targetWindowPosition;
        private float lerpSpeed = 5f;
        private bool isWindowGrabbed = false;
        private float windowResetTimer = 0f;
        private const float WINDOW_RESET_DELAY = 60f; // 60 seconds before window resets position

        // Grab interaction tracking
        private Vec3 lastGrabbedPosition;
        private bool isResetting = false;
        private Handed? activeHand;
        private const float GRAB_DISTANCE = 0.1f; // Maximum distance for grab in meters

        // Debug information control
        private bool showDebugInfo = false;

        // Constructor initializes core components and sets initial positions
        public UIManager(ShapeManager shapeManager)
        {
            this.shapeManager = shapeManager;
            targetWindowPosition = windowPose.position;
            lastGrabbedPosition = windowPose.position;
        }

        // Main render loop for the UI
        // This method is called every frame and handles all UI interactions and rendering
        public void RenderUI()
        {
            bool isCurrentlyGrabbed = CheckWindowGrab();

            // Update grab state
            // We need to handle both grab initiation and release
            // This affects window movement and reset behavior
            if (isCurrentlyGrabbed && !isWindowGrabbed)
            {
                // Window just grabbed
                isWindowGrabbed = true;
                isResetting = false;
                windowResetTimer = 0f;
                lastGrabbedPosition = windowPose.position;
            }
            else if (!isCurrentlyGrabbed && isWindowGrabbed)
            {
                // Window just released
                isWindowGrabbed = false;
                windowResetTimer = 0f;
                activeHand = null;
            }

            // Move window if grabbed
            // This provides immediate feedback to the user and makes the interaction feel responsive
            if (isWindowGrabbed && activeHand.HasValue)
            {
                Hand hand = Input.Hand(activeHand.Value);
                if (hand.IsTracked)
                {
                    Vec3 handDelta = hand.palm.position - lastGrabbedPosition;
                    windowPose.position += handDelta;
                    lastGrabbedPosition = hand.palm.position;
                }
            }

            // Handle timer and resetting
            // This ensures the window doesn't stay out of reach if the user forgets about it
            if (!isWindowGrabbed)
            {
                windowResetTimer += Time.Elapsedf;

                if (windowResetTimer >= WINDOW_RESET_DELAY && !isResetting)
                {
                    isResetting = true;
                    Vec3 headForward = Input.Head.Forward;
                    headForward.y = 0; // Keep the window at eye level
                    headForward = headForward.Normalized;
                    targetWindowPosition = Input.Head.position + headForward * 0.5f;
                }

                if (isResetting)
                {
                    // Use lerp for smooth movement
                    // This looks more natural than instant teleportation
                    windowPose.position = Vec3.Lerp(windowPose.position, targetWindowPosition, Time.Elapsedf * lerpSpeed);

                    // If we're close enough to the target, stop resetting
                    // This prevents jitter when we're very close to the target
                    if (Vec3.Distance(windowPose.position, targetWindowPosition) < 0.01f)
                    {
                        isResetting = false;
                        windowResetTimer = 0f;
                    }
                }
            }

            // Begin the window
            // All UI elements should be rendered between WindowBegin and WindowEnd
            UI.WindowBegin("Control Panel", ref windowPose, windowSize);

            // Use the primary color for the header text
            // This makes the header stand out and improves readability
            UI.PushTint(new Color(0.2f, 0.5f, 0.8f, 1.0f));
            UI.Label("Control Panel");
            UI.PopTint();

            UI.HSeparator();

            // Render the current page
            // This switch statement allows us to easily add new pages in the future
            UI.PushTint(Color.White);
            switch (currentPage)
            {
                case UIPage.Main:
                    RenderMainPage();
                    break;
                case UIPage.MovementControls:
                    RenderMovementControlsPage();
                    break;
                case UIPage.MathShapesMenu:
                    RenderMathShapesMenu();
                    break;
                case UIPage.ShapeInfo:
                    RenderShapeInfoPage();
                    break;
            }
            UI.PopTint();

            // Debug information
            // Only show this if the user has toggled it on
            if (showDebugInfo)
            {
                UI.HSeparator();
                UI.Label($"Return Timer: {WINDOW_RESET_DELAY - windowResetTimer:F1}s");
                UI.Label($"Is Grabbed: {isWindowGrabbed}");
            }

            UI.WindowEnd();
        }

        // Check if the window is currently being grabbed by either hand
        // Returns true if the window is grabbed, false otherwise
        private bool CheckWindowGrab()
        {
            Hand rightHand = Input.Hand(Handed.Right);
            Hand leftHand = Input.Hand(Handed.Left);

            // Check right hand first, then left hand
            // This prioritizes the right hand if both hands are in valid positions
            if (CheckHandGrab(rightHand))
            {
                activeHand = Handed.Right;
                return true;
            }
            else if (CheckHandGrab(leftHand))
            {
                activeHand = Handed.Left;
                return true;
            }

            return false;
        }

        // Check if a specific hand is grabbing the window
        // This considers both the pinch state and the distance to the window
        private bool CheckHandGrab(Hand hand)
        {
            if (!hand.IsTracked) return false;

            float distToWindow = Vec3.Distance(hand.palm.position, windowPose.position);
            return hand.IsPinched && distToWindow < GRAB_DISTANCE;
        }

        // Render the main page of the UI
        // This includes buttons for spawning shapes, accessing other menus, and toggling debug info
        private void RenderMainPage()
        {
            if (UI.Button("Spawn Cube")) shapeManager.ToggleCube();
            if (UI.Button("Math Shapes Menu")) currentPage = UIPage.MathShapesMenu;
            if (UI.Button("Movement Controls")) currentPage = UIPage.MovementControls;

            bool showAxisIndicators = shapeManager.ShowAxisIndicators;
            if (UI.Toggle("Show Axis Indicators", ref showAxisIndicators))
            {
                shapeManager.ShowAxisIndicators = showAxisIndicators;
            }

            // Toggle for debug info visibility
            if (UI.Button(showDebugInfo ? "Hide Return Timer" : "Show Return Timer"))
            {
                showDebugInfo = !showDebugInfo;
            }
        }

        // Render the movement controls page
        // This includes buttons for moving, rotating, and resizing the active shape
        private void RenderMovementControlsPage()
        {
            UI.Label("Movement Controls");

            // Movement controls
            // We use small increments (0.01f) for fine control
            // Consider adding a "big step" option for larger movements
            if (UI.Button("^")) moveAmount.y += 0.01f;
            UI.SameLine();
            if (UI.Button("v")) moveAmount.y -= 0.01f;

            if (UI.Button("<")) moveAmount.x -= 0.01f;
            UI.SameLine();
            if (UI.Button(">")) moveAmount.x += 0.01f;
            UI.SameLine();
            if (UI.Button("F")) moveAmount.z += 0.01f;
            UI.SameLine();
            if (UI.Button("B")) moveAmount.z -= 0.01f;

            // Apply movement
            // We only apply movement if there's actually been a change
            // This prevents unnecessary calls to MoveActiveShape
            if (moveAmount.Magnitude > 0)
            {
                shapeManager.MoveActiveShape(moveAmount);
                moveAmount = Vec3.Zero;
            }

            UI.HSeparator();

            // Rotation and Size controls
            // We use a helper method (AdjustControl) to handle these adjustments
            // This makes it easier to add new controls or change the behavior
            if (UI.Button("-pitch")) AdjustControl("Pitch", -0.1f);
            UI.SameLine();
            if (UI.Button("+pitch")) AdjustControl("Pitch", 0.1f);

            UI.SameLine();

            if (UI.Button("-roll")) AdjustControl("Roll", -0.1f);
            UI.SameLine();
            if (UI.Button("+roll")) AdjustControl("Roll", 0.1f);

            UI.SameLine();

            if (UI.Button("-yaw")) AdjustControl("Yaw", -0.1f);
            UI.SameLine();
            if (UI.Button("+yaw")) AdjustControl("Yaw", 0.1f);

            if (UI.Button("-size")) AdjustControl("Size", -0.1f);
            UI.SameLine();
            if (UI.Button("+size")) AdjustControl("Size", 0.1f);

            // Apply rotation
            // Similar to movement, we only apply rotation if there's been a change
            if (rotationAngles.Magnitude > 0)
            {
                shapeManager.RotateActiveShape(rotationAngles);
                rotationAngles = Vec3.Zero;
            }

            if (UI.Button("Shape Info")) currentPage = UIPage.ShapeInfo;
            if (UI.Button("Back to Main")) currentPage = UIPage.Main;
        }

        // Render the math shapes menu
        // This allows spawning of various mathematical shapes
        private void RenderMathShapesMenu()
        {
            UI.Label("Math Shapes Menu");
            if (UI.Button("Spawn Torus")) shapeManager.ToggleMathShape("torus");
            if (UI.Button("Back to Main")) currentPage = UIPage.Main;
        }

        // Render the shape info page
        // This displays current information about the active shape
        private void RenderShapeInfoPage()
        {
            UI.Label("Shape Information");

            UI.Text($"Roll: {rotationAngles.x:F2}");
            UI.Text($"Pitch: {rotationAngles.y:F2}");
            UI.Text($"Yaw: {rotationAngles.z:F2}");

            // Get the current size from the ShapeManager
            float currentSize = shapeManager.GetActiveShapeSize();
            UI.Text($"Size: {currentSize:F2}");

            if (UI.Button("Back to Movement Controls")) currentPage = UIPage.MovementControls;
        }

        // Helper method to adjust various controls
        // This centralizes the logic for adjusting rotation and size
        private void AdjustControl(string control, float amount)
        {
            switch (control)
            {
                case "Roll":
                    rotationAngles.x += amount;
                    break;
                case "Pitch":
                    rotationAngles.y += amount;
                    break;
                case "Yaw":
                    rotationAngles.z += amount;
                    break;
                case "Size":
                    // We use a multiplier for size to allow for exponential growth/shrinkage
                    // This feels more natural than linear size changes
                    shapeManager.ResizeActiveShape(amount > 0 ? 1.01f : 0.99f);
                    break;
            }
        }
    }

    // Enum to keep track of which page we're currently on
    // This makes it easy to add new pages in the future
    public enum UIPage
    {
        Main,
        MovementControls,
        MathShapesMenu,
        ShapeInfo
    }
}