// Draws a torus (donut shape) in StereoKit.

using StereoKit;
using System;

namespace MyMathShapes
{
    public class MathShapes
    {
     
        public static void DrawTorus(Matrix transform, Color color, float majorRadius, float minorRadius, int segments = 32)
        {
            // Vertex/Index buffers: Raw data for our torus mesh.
            Vertex[] verts = new Vertex[segments * segments];
            uint[] inds = new uint[segments * segments * 6];

            // Nested loops to generate points on the torus surface.
            for (int i = 0; i < segments; i++) // Loop around the major radius
            {
                float u = (float)i / segments; // "Angle" around the major ring
                for (int j = 0; j < segments; j++) // Loop around the minor radius
                {
                    float v = (float)j / segments; // "Angle" around the minor ring

                    // Parametric equations for torus geometry:
                    float x = (majorRadius + minorRadius * (float)Math.Cos(v * 2 * (float)Math.PI)) * (float)Math.Cos(u * 2 * (float)Math.PI);
                    float y = (majorRadius + minorRadius * (float)Math.Cos(v * 2 * (float)Math.PI)) * (float)Math.Sin(u * 2 * (float)Math.PI);
                    float z = minorRadius * (float)Math.Sin(v * 2 * (float)Math.PI);

                    // Vertex data: Position & normal (important for lighting).
                    Vec3 position = new Vec3(x, y, z);
                    Vec3 normal = position.Normalized; // Unit vector for correct shading

                    // Transform: Apply the matrix to get world-space position/normal.
                    normal = transform.Rotation * normal;
                    position = transform * position;

                    // Store the vertex in our buffer.
                    verts[i * segments + j] = new Vertex(position, normal);

                    // Index buffer setup (only for non-edge verts): Triangle strips for efficiency.
                    if (i < segments - 1 && j < segments - 1)
                    {
                        inds[(i * segments + j) * 6 + 0] = (uint)((i + 0) * segments + (j + 0));
                        inds[(i * segments + j) * 6 + 1] = (uint)((i + 1) * segments + (j + 0));
                        inds[(i * segments + j) * 6 + 2] = (uint)((i + 0) * segments + (j + 1));
                        inds[(i * segments + j) * 6 + 3] = (uint)((i + 0) * segments + (j + 1));
                        inds[(i * segments + j) * 6 + 4] = (uint)((i + 1) * segments + (j + 0));
                        inds[(i * segments + j) * 6 + 5] = (uint)((i + 1) * segments + (j + 1));
                    }
                }
            }

            // Create the mesh and set up the vertex/index data.
            Mesh mesh = new Mesh();
            mesh.SetVerts(verts);
            mesh.SetInds(inds);

            // Simple material: Just a flat color.
            Material mat = new Material(Shader.Default);
            mat.SetColor("color", color);

            // Draw call! Renders our torus with the given material and transform.
            mesh.Draw(mat, transform);
        }

    }
}