using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class DI_SceneHelper
{
    public static void LayoutModelsInGrid(DefaultAsset targetFolder)
    {
        if (targetFolder == null) 
            return;

        string folderPath = AssetDatabase.GetAssetPath(targetFolder);
        string[] files = Directory.GetFiles(folderPath, "*.fbx", SearchOption.AllDirectories);

        if (files.Length == 0)
        {
            Debug.LogWarning("No se encontraron archivos FBX en la carpeta seleccionada.");
            return;
        }

        GameObject gridParent = new GameObject($"Grid_Preview_{targetFolder.name}");
        int columns = Mathf.CeilToInt(Mathf.Sqrt(files.Length));

        float currentX = 0;
        float currentZ = 0;
        float maxRowHeight = 0;
        float padding = 2.0f;

        for (int i = 0; i < files.Length; i++)
        {
            string assetPath = files[i].Replace("\\", "/");
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (modelAsset != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
                instance.transform.SetParent(gridParent.transform);
                instance.name = modelAsset.name;

                // --- CÁLCULO DE VOLUMEN ---
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
                Bounds combinedBounds = new Bounds(instance.transform.position, Vector3.zero);

                if (renderers.Length > 0)
                {
                    combinedBounds = renderers[0].bounds;
                    foreach (Renderer renderer in renderers)
                    {
                        combinedBounds.Encapsulate(renderer.bounds);
                    }
                }

                float width = combinedBounds.size.x;
                float depth = combinedBounds.size.z;

                // Posicionar el objeto
                instance.transform.position = new Vector3(currentX + (width / 2), 0, currentZ + (depth / 2));
                currentX += width + padding;

                if (depth > maxRowHeight) 
                    maxRowHeight = depth;

                if ((i + 1) % columns == 0)
                {
                    currentX = 0;
                    currentZ += maxRowHeight + padding;
                    maxRowHeight = 0;
                }
            }
        }

        Undo.RegisterCreatedObjectUndo(gridParent, "Create Smart Model Grid");
        Selection.activeGameObject = gridParent;


        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();
    }
}
