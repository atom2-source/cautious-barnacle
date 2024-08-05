using StereoKit;
using MyMathShapes;
using System.Collections.Generic;

namespace Project2024
{
    // ShapeManager: Core class for managing 3D shapes in our virtual environment.
    // This class is responsible for creating, rendering, and manipulating various 3D shapes.
    // It acts as a central point of control for all shape-related operations in the application.
    public class ShapeManager
    {
        // State variables for shape tracking
        // We use separate variables for the cube and dictionaries for other shapes to allow for easy expansion
        // TODO: Consider unifying this approach if we add many more primitive shapes like the cube
        private bool showCube = false;
        private Pose cubePose = new Pose(Vec3.Zero, Quat.Identity);
        private Pose mathShapePose = new Pose(Vec3.Zero, Quat.Identity);
        private Color cubeColor = Color.HSV(0, 1, 1);  // Default to red
        private Material cubeMaterial;

        // Public properties for global shape controls
        // Exposing these as public properties allows for easy external control and configuration
        public bool ShowAxisIndicators { get; set; } = true;
        public float CubeSize { get; set; } = AppSettings.INITIAL_CUBE_SIZE;
        public float TorusMajorRadius { get; set; } = AppSettings.INITIAL_TORUS_MAJOR_RADIUS;
        public float TorusMinorRadius { get; set; } = AppSettings.INITIAL_TORUS_MINOR_RADIUS;

        // Dictionaries to track multiple math shapes
        // This approach allows for easy addition of new shape types without modifying existing code
        // The tradeoff is slightly more complex logic when rendering and manipulating shapes
        private Dictionary<string, bool> showMathShape = new Dictionary<string, bool>()
        {
            { "torus", false }
            // Add entries for other math shapes as you add them
        };

        private Dictionary<string, Pose> mathShapePoses = new Dictionary<string, Pose>()
        {
            { "torus", new Pose(Vec3.Zero, Quat.Identity) }
            // Add more shapes and their initial poses here
        };

        // Constructor: Initialize materials and default states
        public ShapeManager()
        {
            // We create a single material for the cube to optimize performance
            // If we need different colors for multiple cubes, we'd need to rethink this approach
            cubeMaterial = new Material(Shader.Default);
            cubeMaterial.SetColor("color", cubeColor);
        }

        // Main rendering function: Draws all active shapes
        // This is called every frame, so it needs to be as efficient as possible
        public void RenderShapes()
        {
            // Render the cube if it's active
            if (showCube) RenderCube();

            // Render all active math shapes
            // This foreach loop allows us to easily add new shape types without modifying this method
            foreach (var kvp in showMathShape)
            {
                if (kvp.Value) // Only render if the shape is set to be visible
                {
                    switch (kvp.Key)
                    {
                        case "torus":
                            RenderTorus();
                            break;
                            // Add cases for new shape types here
                    }
                }
            }
        }

        // Private method to render the cube
        // Separated for clarity and to allow for easy customization of cube rendering
        private void RenderCube()
        {
            // Draw the cube using the pre-created material
            // We use TRS matrix for efficient transformation
            Mesh.Cube.Draw(cubeMaterial, Matrix.TRS(cubePose.position, cubePose.orientation, Vec3.One * CubeSize));

            // Optionally draw axis indicators for debugging
            // This is useful for understanding the cube's orientation in 3D space
            if (ShowAxisIndicators) Lines.AddAxis(cubePose, CubeSize * 2);
        }

        // Method to render the torus
        // Uses the Torus class from MyMathShapes namespace
        private void RenderTorus()
        {
            // Create a transform matrix for the torus
            Matrix torusTransform = Matrix.TRS(mathShapePoses["torus"].position, mathShapePoses["torus"].orientation, Vec3.One);

            // Draw the torus using the custom Torus class
            // Note: The color is hardcoded here. Consider making this configurable if needed
            Torus.DrawTorus(torusTransform, new Color(0, 1, 0, 1), TorusMajorRadius, TorusMinorRadius);

            // Draw axis indicators if enabled
            if (ShowAxisIndicators) Lines.AddAxis(mathShapePoses["torus"], TorusMajorRadius * 2);
        }

        // Public method to toggle the cube's visibility
        // This also handles positioning the cube when it's made visible
        public void ToggleCube()
        {
            showCube = !showCube;
            if (showCube)
            {
                // Position the cube in front of the user when it's made visible
                // This provides immediate visual feedback and prevents the cube from spawning in unpredictable locations
                cubePose = new Pose(Input.Head.position + Input.Head.Forward * AppSettings.SPAWN_DISTANCE, Quat.Identity);
            }
        }

        // Method to toggle visibility of math shapes
        // This handles both showing and hiding shapes, as well as initial positioning
        public void ToggleMathShape(string shapeType)
        {
            // Toggle visibility for the specified shape type
            if (showMathShape.ContainsKey(shapeType))
            {
                showMathShape[shapeType] = !showMathShape[shapeType];
            }
            else
            {
                // If the shape type isn't recognized, we simply return
                // TODO: Consider adding error logging or a debug message here
                return;
            }

            // If the shape is now visible, set its initial position
            // This ensures that newly visible shapes always appear in front of the user
            if (showMathShape[shapeType])
            {
                mathShapePoses[shapeType] = new Pose(Input.Head.position + Input.Head.Forward * AppSettings.SPAWN_DISTANCE, Quat.Identity);
            }
        }

        // Method to change the cube's color
        // This could be expanded to handle colors for other shapes as well
        public void SetCubeColor(Color color)
        {
            cubeColor = color;
            cubeMaterial.SetColor("color", cubeColor);
        }

        // Helper method to update the torus's minor radius
        // This maintains a constant proportion between major and minor radii
        public void UpdateTorusMinorRadius()
        {
            TorusMinorRadius = TorusMajorRadius / 3f;
        }

        // Method to move the active shape
        // Currently only handles the cube, but could be expanded for other shapes
        public void MoveActiveShape(Vec3 moveAmount)
        {
            if (showCube) cubePose.position += moveAmount;
            // TODO: Implement movement for other shape types
        }

        // Method to resize the active shape
        // Handles both the cube and the torus
        public void ResizeActiveShape(float scaleFactor)
        {
            if (showCube)
            {
                CubeSize *= scaleFactor;
            }
            else if (showMathShape["torus"])
            {
                TorusMajorRadius *= scaleFactor;
                UpdateTorusMinorRadius(); // Maintain proportion
            }
            // TODO: Implement resizing for other shape types
        }

        // Method to rotate the active shape
        // Currently only handles the cube, but could be expanded for other shapes
        public void RotateActiveShape(Vec3 rotationAngles)
        {
            Quat rotation = Quat.FromAngles(rotationAngles);
            if (showCube)
            {
                cubePose.orientation = cubePose.orientation * rotation;
            }
            // TODO: Implement rotation for other shape types
        }

        // Method to get the size of the active shape
        // This is used for displaying shape information in the UI
        public float GetActiveShapeSize()
        {
            if (showCube)
            {
                return CubeSize;
            }
            else if (showMathShape["torus"])
            {
                return TorusMajorRadius;
            }
            // TODO: Add size retrieval for other shape types

            return 0f; // Default return if no shape is active
        }
    }
}