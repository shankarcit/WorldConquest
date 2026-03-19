using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WorldConquest.Map
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class CountryMesh : MonoBehaviour
    {
        private CountryData data;
        private Material baseMaterial;
        private MeshRenderer meshRenderer;

        private Color baseColor;
        private static readonly Color HoverColor = new Color(1f, 1f, 0.4f, 1f); // yellow highlight

        public CountryData Data => data;

        public void Initialize(CountryData countryData, Material mat, float scale)
        {
            data = countryData;
            baseMaterial = mat;
            meshRenderer = GetComponent<MeshRenderer>();

            baseColor = Random.ColorHSV(0f, 1f, 0.4f, 0.7f, 0.6f, 0.9f);

            Material instanceMat = new Material(mat);
            instanceMat.color = baseColor;
            meshRenderer.material = instanceMat;

            Mesh combinedMesh = BuildCombinedMesh(countryData.Polygons, scale);
            GetComponent<MeshFilter>().mesh = combinedMesh;
            GetComponent<MeshCollider>().sharedMesh = combinedMesh;
        }

        private Mesh BuildCombinedMesh(List<Vector2[]> polygons, float scale)
        {
            CombineInstance[] combine = new CombineInstance[polygons.Count];
            for (int i = 0; i < polygons.Count; i++)
            {
                combine[i].mesh = TriangulatePolygon(polygons[i], scale);
                combine[i].transform = Matrix4x4.identity;
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.CombineMeshes(combine, true, false);
            mesh.RecalculateNormals();
            return mesh;
        }

        /// <summary>
        /// Simple fan triangulation. Works well for convex/mildly concave country shapes.
        /// </summary>
        private Mesh TriangulatePolygon(Vector2[] ring, float scale)
        {
            int n = ring.Length;
            Vector3[] verts = new Vector3[n];
            for (int i = 0; i < n; i++)
                verts[i] = new Vector3(ring[i].x * scale, 0f, ring[i].y * scale);

            int triCount = Mathf.Max(0, n - 2);
            int[] tris = new int[triCount * 3];
            for (int i = 0; i < triCount; i++)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = tris;
            return mesh;
        }

        void OnMouseEnter()
        {
            meshRenderer.material.color = HoverColor;
        }

        void OnMouseExit()
        {
            meshRenderer.material.color = baseColor;
        }

        void OnMouseDown()
        {
            MapEventBus.OnCountryClicked?.Invoke(data);
        }
    }
}
