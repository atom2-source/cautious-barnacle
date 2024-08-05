// ShapeManager: Handles the rendering and interaction of 3D shapes in our scene.

using StereoKit;
using MyMathShapes;

namespace Project2024
{
    public class ShapeManager
    {
        // State variables to track which shapes are currently active and their properties.
        private bool showCube = false;
        private bool showMathShape = false;
        private Pose cubePose = new Pose(Vec3.Zero, Quat.Identity);
        private Pose mathShapePose = new Pose(Vec3.Zero, Quat.Identity);
        private Color cubeColor = Color.HSV(0, 1, 1);
        private Material cubeMaterial;

        // Public properties to control axis indicators and initial shape sizes.
        public bool ShowAxisIndicators { get; set; } = true;
        public float CubeSize { get; set; } = AppSettings.INITIAL_CUBE_SIZE;
        public float TorusMajorRadius { get; set; } = AppSettings.INITIAL_TORUS_MAJOR_RADIUS;
        public float TorusMinorRadius { get; set; } = AppSettings.INITIAL_TORUS_MINOR_RADIUS;

        // Constructor to initialize the cube material with a default color.
        public ShapeManager()
        {
            cubeMaterial = new Material(Shader.Default);
            cubeMaterial.SetColor("color", cubeColor);
        }

        // Main rendering function: Renders the currently active shapes.
        public void RenderShapes()
        {
            if (showCube) RenderCube();
            if (showMathShape) RenderTorus();
        }

        // Private functions to render individual shapes.
        private void RenderCube()
        {
            // Draw the cube using the mesh and material.
            Mesh.Cube.Draw(cubeMaterial, Matrix.TRS(cubePose.position, cubePose.orientation, Vec3.One * CubeSize));

            // Optionally draw axis indicators for debugging.
            if (ShowAxisIndicators) Lines.AddAxis(cubePose, CubeSize * 2);
        }

        private void RenderTorus()
        {
            // Create the transformation matrix for the torus.
            Matrix torusTransform = Matrix.TRS(mathShapePose.position, mathShapePose.orientation, Vec3.One);

            // Use the custom function from MyMathShapes to draw the torus.
            MathShapes.DrawTorus(torusTransform, new Color(0, 1, 0, 1), TorusMajorRadius, TorusMinorRadius);

            // Optionally draw axis indicators for debugging.
            if (ShowAxisIndicators) Lines.AddAxis(mathShapePose, TorusMajorRadius * 2);
        }

        // Public functions to control shape visibility, color, movement, resizing, and rotation.
        public void ToggleCube()
        {
            showCube = !showCube;
            if (showCube)
            {
                cubePose = new Pose(Input.Head.position + Input.Head.Forward * AppSettings.SPAWN_DISTANCE, Quat.Identity);
            }
        }

        public void ToggleMathShape()
        {
            showMathShape = !showMathShape;
            if (showMathShape)
            {
                mathShapePose = new Pose(Input.Head.position + Input.Head.Forward * AppSettings.SPAWN_DISTANCE, Quat.Identity);
            }
        }

        public void SetCubeColor(Color color)
        {
            cubeColor = color;
            cubeMaterial.SetColor("color", cubeColor);
        }

        public void UpdateTorusMinorRadius()
        {
            TorusMinorRadius = TorusMajorRadius / 3f;
        }

        public void MoveActiveShape(Vec3 moveAmount)
        {
            if (showCube) cubePose.position += moveAmount;
            else if (showMathShape) mathShapePose.position += moveAmount;
        }

        public void ResizeActiveShape(float scaleFactor)
        {
            if (showCube) CubeSize *= scaleFactor;
            else if (showMathShape)
            {
                TorusMajorRadius *= scaleFactor;
                UpdateTorusMinorRadius(); // Update the minor radius to keep the proportions
            }
        }

        // Rotate the currently active shape by the given angles.
        public void RotateActiveShape(Vec3 rotationAngles)
        {
            Quat rotation = Quat.FromAngles(rotationAngles);
            if (showCube)
            {
                cubePose.orientation = cubePose.orientation * rotation;
            }
            else if (showMathShape)
            {
                mathShapePose.orientation = mathShapePose.orientation * rotation;
            }
        }
    }
}
