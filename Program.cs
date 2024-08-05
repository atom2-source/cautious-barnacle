using StereoKit;

namespace Project2024
{
    // Program: Entry point and main loop for our StereoKit application.
    // This class sets up the environment, initializes core components,
    // and manages the main render loop.
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize StereoKit with our custom settings
            // If initialization fails, we exit immediately to prevent undefined behavior
            if (!SK.Initialize(AppSettings.GetSKSettings()))
            {
                return;
            }

            // Create core managers for the application
            // We separate concerns into ShapeManager (for 3D objects) and UIManager (for user interface)
            // This separation allows for easier maintenance and potential reuse in other projects
            var shapeManager = new ShapeManager();
            var uiManager = new UIManager(shapeManager);

            // UI color customization
            // We define a custom blue color for UI elements to maintain a consistent theme
            // TODO: Consider moving this to a separate UI configuration class if we add more customization options
            Color primaryColor = new Color(0.2f, 0.5f, 0.8f, 1.0f);
            UI.SetThemeColor(UIColor.Primary, primaryColor);
            UI.SetElementColor(UIVisual.WindowHead, UIColor.Primary);

            // Main render loop
            // This is where all the action happens - it's called every frame
            SK.Run(() =>
            {
                // Floor rendering
                // We only draw a floor in VR (opaque display). In AR, we assume the real world provides the floor.
                // This check allows our app to work seamlessly in both VR and AR environments.
                if (Device.DisplayBlend == DisplayBlend.Opaque)
                {
                    Mesh.Cube.Draw(Assets.FloorMaterial, Assets.FloorTransform);
                }

                // Render order is important:
                // 1. UI is rendered first so it's not occluded by 3D shapes
                // 2. Shapes are rendered last so they appear "in" the world
                uiManager.RenderUI();
                shapeManager.RenderShapes();

                // Note: If we need to add more managers or systems, they should be updated here in the main loop
                // Be cautious about adding too much to this loop, as it can impact performance
            });

            // Graceful shutdown
            // This ensures that all StereoKit resources are properly released
            SK.Shutdown();
        }
    }
}