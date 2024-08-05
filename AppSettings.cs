// AppSettings: Centralized configuration for our project.

using StereoKit;

namespace Project2024
{
    public static class AppSettings
    {
        // Initial sizes for our shapes: cube and torus dimensions.
        public const float INITIAL_CUBE_SIZE = 0.1f;           // 10cm cube to start
        public const float INITIAL_TORUS_MAJOR_RADIUS = 0.3f; // 30cm major radius for the torus
        public const float INITIAL_TORUS_MINOR_RADIUS = 0.1f; // 10cm minor radius for the torus
        public const float SPAWN_DISTANCE = 0.5f;            // Spawn shapes 50cm in front of the user

        // GetSKSettings: Simple helper to return a configured SKSettings object.
        // This sets up the basic info StereoKit needs to run our app.
        public static SKSettings GetSKSettings()
        {
            return new SKSettings
            {
                appName = "Project2024",         // Our app's name (for window titles, etc.)
                assetsFolder = "Assets",       // Where to find our shaders, models, etc.
            };
        }
    }
}
