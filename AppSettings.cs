using StereoKit;

namespace Project2024
{
    // AppSettings: Centralized configuration hub for the entire project.
    // This static class serves as a single source of truth for key application settings.
    // Centralizing these values makes it easier to maintain consistency across the project
    // and simplifies the process of tweaking parameters during development.
    public static class AppSettings
    {
        // Initial dimensions for our shapes
        // These constants define the starting sizes for various 3D objects in our scene.
        // Using constants instead of hard-coded values throughout the project
        // makes it easier to experiment with different sizes and maintain consistency.

        // Initial cube size: 10cm per side
        // This is a reasonable size for hand interaction in VR/AR.
        // Small enough to be easily manipulated, but large enough to be clearly visible.
        public const float INITIAL_CUBE_SIZE = 0.1f;

        // Torus dimensions
        // The torus (donut shape) is defined by two radii:
        // - Major radius: Distance from the center of the tube to the center of the torus
        // - Minor radius: Radius of the tube itself

        // Major radius: 30cm
        // This creates a torus large enough to be a prominent object in the scene,
        // but not so large as to be overwhelming in a typical VR/AR environment.
        public const float INITIAL_TORUS_MAJOR_RADIUS = 0.3f;

        // Minor radius: 10cm
        // This creates a reasonably thick "tube" for the torus.
        // The 1:3 ratio between minor and major radii is aesthetically pleasing
        // and provides a good balance between visibility and manipulability.
        public const float INITIAL_TORUS_MINOR_RADIUS = 0.1f;

        // Spawn distance: 50cm in front of the user
        // This places new objects at a comfortable distance for interaction.
        // Close enough to reach easily, but not so close as to be startling or clip into the user's view.
        public const float SPAWN_DISTANCE = 0.5f;

        // GetSKSettings: Configures and returns StereoKit initialization settings
        // This method encapsulates all the StereoKit-specific configuration in one place,
        // making it easier to adjust these settings as the project evolves.
        public static SKSettings GetSKSettings()
        {
            return new SKSettings
            {
                // Application name
                // This is used for window titles in desktop mode and might be used
                // by the OS or VR/AR runtime for application identification.
                appName = "Project2024",

                // Assets folder
                // This tells StereoKit where to look for shader files, 3D models, textures, etc.
                // Using a relative path like this makes the project more portable.
                assetsFolder = "Assets",

                // Potential additional settings to consider:
                // - displayPreference: Choose between VR, MR, or flatscreen
                // - depthMode: Set the preferred depth buffer precision
                // - logFilter: Configure the verbosity of StereoKit's logging
            };
        }

        // As the project grows, consider:
        // 1. Adding runtime configuration options that can be adjusted in-app
        // 2. Implementing a system to load some settings from a config file
        // 3. Creating debug/development specific settings that differ from production settings
    }
}