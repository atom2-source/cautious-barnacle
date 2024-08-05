using StereoKit;
using System;

namespace MyMathShapes
{
    // Torus: A class for generating and rendering a torus (donut shape) in StereoKit.
    // This implementation uses a parametric approach to generate the torus geometry.
    public class Torus
    {
        // DrawTorus: Generates and renders a torus with the given parameters.
        // This method handles everything from geometry creation to final rendering.
        // 
        // Performance note: This method generates a new mesh every call. For static toruses,
        // consider caching the mesh and reusing it to improve performance.
        public static void DrawTorus(Matrix transform, Color color, float majorRadius, float minorRadius, int segments = 32)
        {
            // Vertex and index buffer initialization
            // We use a square number of vertices for simplicity, though this isn't strictly necessary.
            // Memory usage grows quadratically with segment count - be cautious with very high values.
            Vertex[] verts = new Vertex[segments * segments];
            uint[] inds = new uint[segments * segments * 6];  // 6 indices per quad (2 triangles)

            // Nested loops to generate torus surface points
            // Outer loop: Major circle of the torus
            // Inner loop: Minor circle (tube cross-section)
            for (int i = 0; i < segments; i++)
            {
                float u = (float)i / segments;  // Normalized angle around major circle [0, 1)
                for (int j = 0; j < segments; j++)
                {
                    float v = (float)j / segments;  // Normalized angle around minor circle [0, 1)

                    // Parametric equations for torus
                    // These equations map the 2D (u,v) coordinate to a 3D point on the torus surface
                    float theta = u * 2 * (float)Math.PI;  // Angle around major circle [0, 2π)
                    float phi = v * 2 * (float)Math.PI;    // Angle around minor circle [0, 2π)
                    float x = (majorRadius + minorRadius * (float)Math.Cos(phi)) * (float)Math.Cos(theta);
                    float y = (majorRadius + minorRadius * (float)Math.Cos(phi)) * (float)Math.Sin(theta);
                    float z = minorRadius * (float)Math.Sin(phi);

                    // Generate vertex data
                    Vec3 position = new Vec3(x, y, z);
                    // Normal vector is simply the normalized position vector (relative to torus center)
                    // This works because the torus is centered at the origin before transformation
                    Vec3 normal = position.Normalized;

                    // Apply the transformation matrix
                    // Note: We only rotate the normal, as it's a direction, not a position
                    normal = transform.Rotation * normal;
                    position = transform * position;

                    // Store the vertex
                    int vertIndex = i * segments + j;
                    verts[vertIndex] = new Vertex(position, normal);

                    // Generate indices for triangles
                    // We skip the last row and column as they wrap around to the first
                    if (i < segments - 1 && j < segments - 1)
                    {
                        int quadIndex = (i * segments + j) * 6;
                        uint topLeft = (uint)(i * segments + j);
                        uint topRight = (uint)((i + 1) * segments + j);
                        uint bottomLeft = (uint)(i * segments + (j + 1));
                        uint bottomRight = (uint)((i + 1) * segments + (j + 1));

                        // First triangle of quad
                        inds[quadIndex] = topLeft;
                        inds[quadIndex + 1] = topRight;
                        inds[quadIndex + 2] = bottomLeft;

                        // Second triangle of quad
                        inds[quadIndex + 3] = bottomLeft;
                        inds[quadIndex + 4] = topRight;
                        inds[quadIndex + 5] = bottomRight;
                    }
                }
            }

            // Create and populate the mesh
            // StereoKit handles GPU resource management for us
            Mesh mesh = new Mesh();
            mesh.SetVerts(verts);
            mesh.SetInds(inds);

            // Create a simple material
            // We use the default shader and set only the color for simplicity
            // For more complex rendering, consider creating a custom shader
            Material mat = new Material(Shader.Default);
            mat.SetColor("color", color);

            // Render the torus
            // Note: This draw call happens immediately. For more complex scenes,
            // consider using a batching system to reduce draw calls.
            mesh.Draw(mat, transform);

            // Note: We're not explicitly disposing of the Mesh or Material here.
            // StereoKit handles garbage collection for these resources.
            // For high-frequency operations, manual disposal might be beneficial.
        }
    }
}