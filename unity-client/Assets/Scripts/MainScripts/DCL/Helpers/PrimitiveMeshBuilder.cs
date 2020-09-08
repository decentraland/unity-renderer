using UnityEngine;


/*
* PrimitiveMeshBuilder originated in the DecentralandUnityPlugin that can be found at:
* https://github.com/fairwood/DecentralandUnityPlugin
*/

namespace DCL.Helpers
{
    public class PrimitiveMeshBuilder
    {
        public static Mesh BuildSphere(float radius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "DCL Sphere";

            //float radius = 1f;
            // Longitude |||
            int nbLong = 24;
            // Latitude ---
            int nbLat = 16;

            #region Vertices

            Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
            float _pi = Mathf.PI;
            float _2pi = _pi * 2f;

            vertices[0] = Vector3.up * radius;
            for (int lat = 0; lat < nbLat; lat++)
            {
                float a1 = _pi * (float) (lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    float a2 = _2pi * (float) (lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
                }
            }

            vertices[vertices.Length - 1] = Vector3.up * -radius;

            #endregion

            #region Normales

            Vector3[] normales = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
            {
                normales[n] = vertices[n].normalized;
            }

            #endregion

            #region UVs

            Vector2[] uvs = new Vector2[vertices.Length];
            uvs[0] = Vector2.up;
            uvs[uvs.Length - 1] = Vector2.zero;
            for (int lat = 0; lat < nbLat; lat++)
            {
                for (int lon = 0; lon <= nbLong; lon++)
                {
                    uvs[lon + lat * (nbLong + 1) + 1] =
                        new Vector2(1f - (float) lon / nbLong, (float) (lat + 1) / (nbLat + 1));
                }
            }
            //uvs[lon + lat * (nbLong + 1) + 1] = new Vector2( (float)lon / nbLong, 1f - (float)(lat+1) / (nbLat+1) );

            #endregion

            #region Triangles

            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            //Top Cap
            int i = 0;
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = lon + 2;
                triangles[i++] = lon + 1;
                triangles[i++] = 0;
            }

            //Middle
            for (int lat = 0; lat < nbLat - 1; lat++)
            {
                for (int lon = 0; lon < nbLong; lon++)
                {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    triangles[i++] = current;
                    triangles[i++] = current + 1;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = next;
                }
            }

            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildPlane(float _size)
        {
            Mesh mesh = new Mesh();
            mesh.name = "DCL Plane";
            Vector3[] vertices = new Vector3[8];
            Vector3[] normals = new Vector3[8];
            Vector2[] uvs = new Vector2[8];
            Color[] colors = new Color[8];

            int[] tris = new int[4 * 3];

            int vIndex = 0;
            Vector3 start = new Vector3(-_size / 2, _size / 2, 0);
            vertices[vIndex++] = new Vector3(-start.x, -start.y, 0);
            vertices[vIndex++] = new Vector3(start.x, -start.y, 0);
            vertices[vIndex++] = new Vector3(start.x, start.y, 0);
            vertices[vIndex++] = new Vector3(-start.x, start.y, 0);

            vertices[vIndex++] = new Vector3(-start.x, -start.y, 0);
            vertices[vIndex++] = new Vector3(start.x, -start.y, 0);
            vertices[vIndex++] = new Vector3(start.x, start.y, 0);
            vertices[vIndex++] = new Vector3(-start.x, start.y, 0);

            vIndex = 0;
            uvs[vIndex++] = new Vector2(0f, 1f);
            uvs[vIndex++] = new Vector2(1f, 1f);
            uvs[vIndex++] = new Vector2(1f, 0f);
            uvs[vIndex++] = new Vector2(0f, 0f);

            uvs[vIndex++] = new Vector2(0f, 1f);
            uvs[vIndex++] = new Vector2(1f, 1f);
            uvs[vIndex++] = new Vector2(1f, 0f);
            uvs[vIndex++] = new Vector2(0f, 0f);

            vIndex = 0;
            normals[vIndex++] = Vector3.forward;
            normals[vIndex++] = Vector3.forward;
            normals[vIndex++] = Vector3.forward;
            normals[vIndex++] = Vector3.forward;

            normals[vIndex++] = Vector3.back;
            normals[vIndex++] = Vector3.back;
            normals[vIndex++] = Vector3.back;
            normals[vIndex++] = Vector3.back;

            vIndex = 0;
            colors[vIndex++] = Color.white;
            colors[vIndex++] = Color.white;
            colors[vIndex++] = Color.white;
            colors[vIndex++] = Color.white;

            colors[vIndex++] = Color.white;
            colors[vIndex++] = Color.white;
            colors[vIndex++] = Color.white;
            colors[vIndex++] = Color.white;

            int cnt = 0;
            tris[cnt++] = 2;
            tris[cnt++] = 1;
            tris[cnt++] = 0;
            tris[cnt++] = 3;
            tris[cnt++] = 2;
            tris[cnt++] = 0;

            tris[cnt++] = 4 + 1;
            tris[cnt++] = 4 + 2;
            tris[cnt++] = 4 + 0;
            tris[cnt++] = 4 + 2;
            tris[cnt++] = 4 + 3;
            tris[cnt++] = 4 + 0;

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.colors = colors;

            mesh.triangles = tris;
            return mesh;
        }

        public static Mesh BuildCube(float _size)
        {
            Mesh mesh = new Mesh();
            mesh.name = "DCL Box";
            Vector3[] vertices = new Vector3[24]; //top bottom left right front back
            Vector3[] normals = new Vector3[24];
            Vector2[] uvs = new Vector2[24];
            Vector2[] uvs2 = new Vector2[24];
            int[] tris = new int[12 * 3];

            int vIndex = 0;
            //top and bottom
            Vector3 start = new Vector3(-_size / 2, _size / 2, _size / 2);
            vertices[vIndex++] = start;
            vertices[vIndex++] = start + Vector3.right * _size;
            vertices[vIndex++] = start + Vector3.right * _size + Vector3.back * _size;
            vertices[vIndex++] = start + Vector3.back * _size;

            start = new Vector3(-_size / 2, -_size / 2, _size / 2);
            vertices[vIndex++] = start;
            vertices[vIndex++] = start + Vector3.right * _size;
            vertices[vIndex++] = start + Vector3.right * _size + Vector3.back * _size;
            vertices[vIndex++] = start + Vector3.back * _size;

            //left and right
            start = new Vector3(-_size / 2, _size / 2, _size / 2);
            vertices[vIndex++] = start;
            vertices[vIndex++] = start + Vector3.back * _size;
            vertices[vIndex++] = start + Vector3.back * _size + Vector3.down * _size;
            vertices[vIndex++] = start + Vector3.down * _size;

            start = new Vector3(_size / 2, _size / 2, _size / 2);
            vertices[vIndex++] = start;
            vertices[vIndex++] = start + Vector3.back * _size;
            vertices[vIndex++] = start + Vector3.back * _size + Vector3.down * _size;
            vertices[vIndex++] = start + Vector3.down * _size;

            //front and back
            start = new Vector3(-_size / 2, _size / 2, _size / 2);
            vertices[vIndex++] = start;
            vertices[vIndex++] = start + Vector3.right * _size;
            vertices[vIndex++] = start + Vector3.right * _size + Vector3.down * _size;
            vertices[vIndex++] = start + Vector3.down * _size;

            start = new Vector3(-_size / 2, _size / 2, -_size / 2);
            vertices[vIndex++] = start;
            vertices[vIndex++] = start + Vector3.right * _size;
            vertices[vIndex++] = start + Vector3.right * _size + Vector3.down * _size;
            vertices[vIndex++] = start + Vector3.down * _size;

            //uv
            vIndex = 0;
            //top and bottom
            uvs[vIndex++] = new Vector2(1f, 1f);
            uvs[vIndex++] = new Vector2(1f, 0f);
            uvs[vIndex++] = new Vector2(0f, 0f);
            uvs[vIndex++] = new Vector2(0f, 1f);

            uvs[vIndex++] = new Vector2(1f, 0f);
            uvs[vIndex++] = new Vector2(1f, 1f);
            uvs[vIndex++] = new Vector2(0f, 1f);
            uvs[vIndex++] = new Vector2(0f, 0f);

            //left and right
            uvs[vIndex++] = new Vector2(1f, 1f);
            uvs[vIndex++] = new Vector2(1f, 0f);
            uvs[vIndex++] = new Vector2(0f, 0f);
            uvs[vIndex++] = new Vector2(0f, 1f);

            uvs[vIndex++] = new Vector2(1f, 0f);
            uvs[vIndex++] = new Vector2(1f, 1f);
            uvs[vIndex++] = new Vector2(0f, 1f);
            uvs[vIndex++] = new Vector2(0f, 0f);

            //front and back
            uvs[vIndex++] = new Vector2(0f, 0f);
            uvs[vIndex++] = new Vector2(1f, 0f);
            uvs[vIndex++] = new Vector2(1f, 1f);
            uvs[vIndex++] = new Vector2(0f, 1f);

            uvs[vIndex++] = new Vector2(0f, 1f);
            uvs[vIndex++] = new Vector2(1f, 1f);
            uvs[vIndex++] = new Vector2(1f, 0f);
            uvs[vIndex++] = new Vector2(0f, 0f);

            //uv2
            vIndex = 0;
            //top and bottom
            uvs2[vIndex++] = new Vector2(1f, 1f);
            uvs2[vIndex++] = new Vector2(1f, 0f);
            uvs2[vIndex++] = new Vector2(0f, 0f);
            uvs2[vIndex++] = new Vector2(0f, 1f);

            uvs2[vIndex++] = new Vector2(1f, 0f);
            uvs2[vIndex++] = new Vector2(1f, 1f);
            uvs2[vIndex++] = new Vector2(0f, 1f);
            uvs2[vIndex++] = new Vector2(0f, 0f);

            //left and right
            uvs2[vIndex++] = new Vector2(1f, 1f);
            uvs2[vIndex++] = new Vector2(1f, 0f);
            uvs2[vIndex++] = new Vector2(0f, 0f);
            uvs2[vIndex++] = new Vector2(0f, 1f);

            uvs2[vIndex++] = new Vector2(1f, 0f);
            uvs2[vIndex++] = new Vector2(1f, 1f);
            uvs2[vIndex++] = new Vector2(0f, 1f);
            uvs2[vIndex++] = new Vector2(0f, 0f);

            //front and back
            uvs2[vIndex++] = new Vector2(0f, 0f);
            uvs2[vIndex++] = new Vector2(1f, 0f);
            uvs2[vIndex++] = new Vector2(1f, 1f);
            uvs2[vIndex++] = new Vector2(0f, 1f);

            uvs2[vIndex++] = new Vector2(0f, 1f);
            uvs2[vIndex++] = new Vector2(1f, 1f);
            uvs2[vIndex++] = new Vector2(1f, 0f);
            uvs2[vIndex++] = new Vector2(0f, 0f);

            //normal
            vIndex = 0;
            //top and bottom
            normals[vIndex++] = Vector3.up;
            normals[vIndex++] = Vector3.up;
            normals[vIndex++] = Vector3.up;
            normals[vIndex++] = Vector3.up;

            normals[vIndex++] = Vector3.down;
            normals[vIndex++] = Vector3.down;
            normals[vIndex++] = Vector3.down;
            normals[vIndex++] = Vector3.down;

            //left and right
            normals[vIndex++] = Vector3.left;
            normals[vIndex++] = Vector3.left;
            normals[vIndex++] = Vector3.left;
            normals[vIndex++] = Vector3.left;

            normals[vIndex++] = Vector3.right;
            normals[vIndex++] = Vector3.right;
            normals[vIndex++] = Vector3.right;
            normals[vIndex++] = Vector3.right;

            //front and back
            normals[vIndex++] = Vector3.forward;
            normals[vIndex++] = Vector3.forward;
            normals[vIndex++] = Vector3.forward;
            normals[vIndex++] = Vector3.forward;

            normals[vIndex++] = Vector3.back;
            normals[vIndex++] = Vector3.back;
            normals[vIndex++] = Vector3.back;
            normals[vIndex++] = Vector3.back;


            int cnt = 0;

            //top and bottom
            tris[cnt++] = 0;
            tris[cnt++] = 1;
            tris[cnt++] = 2;
            tris[cnt++] = 0;
            tris[cnt++] = 2;
            tris[cnt++] = 3;

            tris[cnt++] = 4 + 0;
            tris[cnt++] = 4 + 2;
            tris[cnt++] = 4 + 1;
            tris[cnt++] = 4 + 0;
            tris[cnt++] = 4 + 3;
            tris[cnt++] = 4 + 2;

            //left and right
            tris[cnt++] = 8 + 0;
            tris[cnt++] = 8 + 1;
            tris[cnt++] = 8 + 2;
            tris[cnt++] = 8 + 0;
            tris[cnt++] = 8 + 2;
            tris[cnt++] = 8 + 3;

            tris[cnt++] = 12 + 0;
            tris[cnt++] = 12 + 2;
            tris[cnt++] = 12 + 1;
            tris[cnt++] = 12 + 0;
            tris[cnt++] = 12 + 3;
            tris[cnt++] = 12 + 2;

            //front and back
            tris[cnt++] = 16 + 0;
            tris[cnt++] = 16 + 2;
            tris[cnt++] = 16 + 1;
            tris[cnt++] = 16 + 0;
            tris[cnt++] = 16 + 3;
            tris[cnt++] = 16 + 2;

            tris[cnt++] = 20 + 0;
            tris[cnt++] = 20 + 1;
            tris[cnt++] = 20 + 2;
            tris[cnt++] = 20 + 0;
            tris[cnt++] = 20 + 2;
            tris[cnt++] = 20 + 3;

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.uv2 = uvs2;

            mesh.triangles = tris;

            return mesh;
        }

        public static Mesh BuildConeOrCylinder(int numVertices, float radiusTop, float radiusBottom, float length,
            float openingAngle, bool outside, bool inside, bool isCylinder, Vector3 offsetPos = default(Vector3))
        {
            if (openingAngle > 0 && openingAngle < 180)
            {
                radiusTop = 0;
                radiusBottom = length * Mathf.Tan(openingAngle * Mathf.Deg2Rad / 2);
            }

            string meshName = isCylinder
                ? "DCL Cylinder"
                : "DCL Cone" + numVertices + "v" + radiusTop + "t" + radiusBottom + "b" + length + "l" + length +
                  (outside ? "o" : "") + (inside ? "i" : "");
            //string meshPrefabPath = "Assets/Decentraland/Internal/" + meshName + ".asset";
            Mesh mesh = null; //(Mesh)AssetDatabase.LoadAssetAtPath(meshPrefabPath, typeof(Mesh));
            if (mesh == null)
            {
                int numVertices2 = numVertices + 1;

                mesh = new Mesh();
                mesh.name = meshName;
                // can't access Camera.current
                //newCone.transform.position = Camera.current.transform.position + Camera.current.transform.forward * 5.0f;
                int multiplier = (outside ? 1 : 0) + (inside ? 1 : 0);
                int offset = (outside && inside ? 2 * numVertices2 : 0);

                bool bTopCap = isCylinder ? true : false;
                bool bBottomCap = true;

                Vector3[] vertices =
                    new Vector3[
                        2 * multiplier * numVertices2 + (bTopCap ? (numVertices + 1) : 0) +
                        (bBottomCap ? (numVertices + 1) : 0)];
                // 0..n-1: top, n..2n-1: bottom
                Vector3[] normals =
                    new Vector3[
                        2 * multiplier * numVertices2 + (bTopCap ? (numVertices + 1) : 0) +
                        (bBottomCap ? (numVertices + 1) : 0)];
                Vector2[] uvs =
                    new Vector2[
                        2 * multiplier * numVertices2 + (bTopCap ? (numVertices + 1) : 0) +
                        (bBottomCap ? (numVertices + 1) : 0)];
                int[] tris;
                float slope = Mathf.Atan((radiusBottom - radiusTop) / length); // (rad difference)/height
                float slopeSin = Mathf.Sin(slope);
                float slopeCos = Mathf.Cos(slope);
                int i;

                for (i = 0; i < numVertices; i++)
                {
                    float angle = 2 * Mathf.PI * i / numVertices;
                    float angleSin = Mathf.Sin(angle);
                    float angleCos = Mathf.Cos(angle);
                    float angleHalf = 2 * Mathf.PI * (i + 0.5f) / numVertices; // for degenerated normals at cone tips
                    float angleHalfSin = Mathf.Sin(angleHalf);
                    float angleHalfCos = Mathf.Cos(angleHalf);

                    vertices[i] = new Vector3(radiusTop * angleCos, length, radiusTop * angleSin) + offsetPos;
                    vertices[i + numVertices2] =
                        new Vector3(radiusBottom * angleCos, 0, radiusBottom * angleSin) + offsetPos;

                    if (radiusTop == 0)
                    {
                        normals[i] = new Vector3(angleHalfCos * slopeCos, -slopeSin, angleHalfSin * slopeCos);
                    }
                    else
                    {
                        normals[i] = new Vector3(angleCos * slopeCos, -slopeSin, angleSin * slopeCos);
                    }

                    if (radiusBottom == 0)
                    {
                        normals[i + numVertices2] =
                            new Vector3(angleHalfCos * slopeCos, -slopeSin, angleHalfSin * slopeCos);
                    }
                    else
                    {
                        normals[i + numVertices2] = new Vector3(angleCos * slopeCos, -slopeSin, angleSin * slopeCos);
                    }

                    uvs[i] = new Vector2(1.0f - 1.0f * i / numVertices, 1);
                    uvs[i + numVertices2] = new Vector2(1.0f - 1.0f * i / numVertices, 0);

                    if (outside && inside)
                    {
                        // vertices and uvs are identical on inside and outside, so just copy
                        vertices[i + 2 * numVertices2] = vertices[i];
                        vertices[i + 3 * numVertices2] = vertices[i + numVertices2];
                        uvs[i + 2 * numVertices2] = uvs[i];
                        uvs[i + 3 * numVertices2] = uvs[i + numVertices2];
                    }

                    if (inside)
                    {
                        // invert normals
                        normals[i + offset] = -normals[i];
                        normals[i + numVertices2 + offset] = -normals[i + numVertices2];
                    }
                }

                vertices[numVertices] = vertices[0];
                vertices[numVertices + numVertices2] = vertices[0 + numVertices2];
                uvs[numVertices] = new Vector2(1.0f - 1.0f * numVertices / numVertices, 1);
                uvs[numVertices + numVertices2] = new Vector2(1.0f - 1.0f * numVertices / numVertices, 0);
                normals[numVertices] = normals[0];
                normals[numVertices + numVertices2] = normals[0 + numVertices2];


                int coverTopIndexStart = 2 * multiplier * numVertices2;
                int coverTopIndexEnd = 2 * multiplier * numVertices2 + numVertices;

                if (bTopCap)
                {
                    for (i = 0; i < numVertices; i++)
                    {
                        float angle = 2 * Mathf.PI * i / numVertices;
                        float angleSin = Mathf.Sin(angle);
                        float angleCos = Mathf.Cos(angle);

                        vertices[coverTopIndexStart + i] =
                            new Vector3(radiusTop * angleCos, length, radiusTop * angleSin) + offsetPos;
                        normals[coverTopIndexStart + i] = new Vector3(0, 1, 0);
                        uvs[coverTopIndexStart + i] = new Vector2(angleCos / 2 + 0.5f, angleSin / 2 + 0.5f);
                    }

                    vertices[coverTopIndexStart + numVertices] = new Vector3(0, length, 0) + offsetPos;
                    normals[coverTopIndexStart + numVertices] = new Vector3(0, 1, 0);
                    uvs[coverTopIndexStart + numVertices] = new Vector2(0.5f, 0.5f);
                }


                int coverBottomIndexStart = coverTopIndexStart + (bTopCap ? 1 : 0) * (numVertices + 1);
                int coverBottomIndexEnd = coverBottomIndexStart + numVertices;
                if (bBottomCap)
                {
                    for (i = 0; i < numVertices; i++)
                    {
                        float angle = 2 * Mathf.PI * i / numVertices;
                        float angleSin = Mathf.Sin(angle);
                        float angleCos = Mathf.Cos(angle);

                        vertices[coverBottomIndexStart + i] =
                            new Vector3(radiusBottom * angleCos, 0f, radiusBottom * angleSin) +
                            offsetPos;
                        normals[coverBottomIndexStart + i] = new Vector3(0, -1, 0);
                        uvs[coverBottomIndexStart + i] = new Vector2(angleCos / 2 + 0.5f, angleSin / 2 + 0.5f);
                    }

                    vertices[coverBottomIndexStart + numVertices] = new Vector3(0, 0f, 0) + offsetPos;
                    normals[coverBottomIndexStart + numVertices] = new Vector3(0, -1, 0);
                    uvs[coverBottomIndexStart + numVertices] = new Vector2(0.5f, 0.5f);
                }

                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.uv = uvs;

                // create triangles
                // here we need to take care of point order, depending on inside and outside
                int cnt = 0;
                if (radiusTop == 0)
                {
                    // top cone
                    tris =
                        new int[numVertices2 * 3 * multiplier + ((bTopCap ? 1 : 0) * numVertices * 3) +
                                ((bBottomCap ? 1 : 0) * numVertices * 3)
                        ];
                    if (outside)
                    {
                        for (i = 0; i < numVertices; i++)
                        {
                            tris[cnt++] = i + numVertices2;
                            tris[cnt++] = i;
                            tris[cnt++] = i + 1 + numVertices2;
                            //							if(i==numVertices-1)
                            //								tris[cnt++]=numVertices;
                            //							else
                            //								tris[cnt++]=i+1+numVertices;
                        }
                    }

                    if (inside)
                    {
                        for (i = offset; i < numVertices + offset; i++)
                        {
                            tris[cnt++] = i;
                            tris[cnt++] = i + numVertices2;
                            tris[cnt++] = i + 1 + numVertices2;
                            //							if(i==numVertices-1+offset)
                            //								tris[cnt++]=numVertices+offset;
                            //							else
                            //								tris[cnt++]=i+1+numVertices;
                        }
                    }
                }
                else if (radiusBottom == 0)
                {
                    // bottom cone
                    tris =
                        new int[numVertices2 * 3 * multiplier + ((bTopCap ? 1 : 0) * numVertices * 3) +
                                ((bBottomCap ? 1 : 0) * numVertices * 3)
                        ];
                    if (outside)
                    {
                        for (i = 0; i < numVertices; i++)
                        {
                            tris[cnt++] = i;
                            tris[cnt++] = i + 1;
                            //							if(i==numVertices-1)
                            //								tris[cnt++]=0;
                            //							else
                            //								tris[cnt++]=i+1;
                            tris[cnt++] = i + numVertices2;
                        }
                    }

                    if (inside)
                    {
                        for (i = offset; i < numVertices + offset; i++)
                        {
                            //							if(i==numVertices-1+offset)
                            //								tris[cnt++]=offset;
                            //							else
                            //								tris[cnt++]=i+1;
                            tris[cnt++] = i + 1;
                            tris[cnt++] = i;
                            tris[cnt++] = i + numVertices2;
                        }
                    }
                }
                else
                {
                    // truncated cone
                    tris =
                        new int[numVertices2 * 6 * multiplier + ((bTopCap ? 1 : 0) * numVertices * 3) +
                                ((bBottomCap ? 1 : 0) * numVertices * 3)
                        ];
                    if (outside)
                    {
                        for (i = 0; i < numVertices; i++)
                        {
                            int ip1 = i + 1;
                            //							if(ip1==numVertices)
                            //								ip1=0;

                            tris[cnt++] = i;
                            tris[cnt++] = ip1;
                            tris[cnt++] = i + numVertices2;

                            tris[cnt++] = ip1 + numVertices2;
                            tris[cnt++] = i + numVertices2;
                            tris[cnt++] = ip1;
                        }
                    }

                    if (inside)
                    {
                        for (i = offset; i < numVertices + offset; i++)
                        {
                            int ip1 = i + 1;
                            //							if(ip1==numVertices+offset)
                            //								ip1=offset;

                            tris[cnt++] = ip1;
                            tris[cnt++] = i;
                            tris[cnt++] = i + numVertices2;

                            tris[cnt++] = i + numVertices2;
                            tris[cnt++] = ip1 + numVertices2;
                            tris[cnt++] = ip1;
                        }
                    }
                }

                if (bTopCap)
                {
                    for (i = 0; i < numVertices; ++i)
                    {
                        int next = coverTopIndexStart + i + 1;

                        if (next == coverTopIndexEnd)
                        {
                            next = coverTopIndexStart;
                        }

                        tris[cnt++] = next;
                        tris[cnt++] = coverTopIndexStart + i;
                        tris[cnt++] = coverTopIndexEnd;
                    }
                }

                if (bBottomCap)
                {
                    for (i = 0; i < numVertices; ++i)
                    {
                        int next = coverBottomIndexStart + i + 1;
                        if (next == coverBottomIndexEnd)
                        {
                            next = coverBottomIndexStart;
                        }

                        tris[cnt++] = coverBottomIndexEnd;
                        tris[cnt++] = coverBottomIndexStart + i;
                        tris[cnt++] = next;
                    }
                }

                mesh.triangles = tris;
                //AssetDatabase.CreateAsset(mesh, meshPrefabPath);
                //AssetDatabase.SaveAssets();
            }

            return mesh;
        }

        public static Mesh BuildCone(int numVertices, float radiusTop, float radiusBottom, float length,
            float openingAngle, bool outside, bool inside)
        {
            return BuildConeOrCylinder(numVertices, radiusTop, radiusBottom, length, openingAngle, outside, inside,
                false,
                new Vector3(0f, -length / 2, 0f));
        }

        public static Mesh BuildCylinder(int numVertices, float radiusTop, float radiusBottom, float length,
            float openingAngle, bool outside, bool inside)
        {
            return BuildConeOrCylinder(numVertices, radiusTop, radiusBottom, length, openingAngle, outside, inside,
                true,
                new Vector3(0f, -length / 2, 0f));
        }
    }
}