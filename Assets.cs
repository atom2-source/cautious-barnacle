// Assets class for managing project resources (specifically, the floor).

using StereoKit;

namespace Project2024
{

    public static class Assets
    {
        // Floor transform: Positioned at Y = -1.5, scaled to 30x0.1x30.
        public static Matrix FloorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));

        // Floor material: Assumes you have a shader named "floor.hlsl".
        public static Material FloorMaterial = new Material("floor.hlsl");

        // Static constructor: Runs only ONCE when the class is first accessed.
        static Assets()
        {
            // Make the floor slightly transparent for a visual effect.
            FloorMaterial.Transparency = Transparency.Blend;
        }
    }
}