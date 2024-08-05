using StereoKit;

namespace Project2024
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!SK.Initialize(AppSettings.GetSKSettings()))
            {
                return;                                                                        // If initialization fails, terminate the application
            }

            // Create the core managers for our app: shapes and UI.
            var shapeManager = new ShapeManager();
            var uiManager = new UIManager(shapeManager); // UI gets a reference to shapes

            // UI Tweaks: Custom blue color, apply to headers.
            Color primarycolor = new Color(0.2f, 0.5f, 0.8f, 1.0f);
            UI.SetThemeColor(UIColor.Primary, primarycolor);
            UI.SetElementColor(UIVisual.WindowHead, UIColor.Primary);

            // Main StereoKit render loop.
            SK.Run(() =>
            {
                // Draw a floor *only* if not on a transparent display (like AR).
                if (Device.DisplayBlend == DisplayBlend.Opaque)
                    Mesh.Cube.Draw(Assets.FloorMaterial, Assets.FloorTransform);

                // Let the managers do their thing: UI first, then the 3D shapes.
                uiManager.RenderUI();
                shapeManager.RenderShapes();
            });

            // Clean shutdown of StereoKit when we're done.
            SK.Shutdown();                                                                  // Shutdown StereoKit gracefully when the application exits
        }
    }
}
