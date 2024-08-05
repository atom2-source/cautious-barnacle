using StereoKit;

namespace Project2024
{
    // Assets: Static class for centralized management of shared resources.
    // This approach allows for easy access to common assets across the project,
    // reducing duplication and improving maintainability.
    public static class Assets
    {
        // Floor Transform: Defines the position, rotation, and scale of the floor.
        // We use a single, large floor instead of tiling smaller pieces for simplicity and performance.
        // The Y position of -1.5 places the floor below the default camera height,
        // creating a sense of standing on the ground.
        // The scale of 30x0.1x30 creates a large, thin floor that extends beyond the immediate play area.
        public static Matrix FloorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));

        // Floor Material: Defines the visual properties of the floor.
        // We use a custom shader (floor.hlsl) for the floor material.
        // This allows for more complex visual effects than a basic color material.
        // IMPORTANT: Ensure that "floor.hlsl" exists in the project's asset folder.
        // If the shader is missing, this will throw an exception when accessed.
        public static Material FloorMaterial = new Material("floor.hlsl");

        // Static constructor: Initializes the Assets class.
        // This runs only once, when the Assets class is first accessed.
        // It's a good place for one-time setup of shared resources.
        static Assets()
        {
            // Set the floor material to use alpha blending.
            // This allows for partial transparency, creating a subtle visual effect.
            // Be cautious with transparent floors in VR, as they can affect depth perception.
            // Consider making this configurable if you need different floor styles.
            FloorMaterial.Transparency = Transparency.Blend;

            // Potential Improvements:
            // 1. Add error handling for missing shader file.
            // 2. Consider loading floor parameters (size, position, transparency) from a config file.
            // 3. If more assets are added, implement a resource management system to handle loading/unloading.
        }

        // Note: As the project grows, consider adding methods for:
        // - Explicitly initializing assets (allowing for async loading)
        // - Cleaning up resources when the application exits
        // - Reloading assets during runtime (useful for development iterations)
    }
}