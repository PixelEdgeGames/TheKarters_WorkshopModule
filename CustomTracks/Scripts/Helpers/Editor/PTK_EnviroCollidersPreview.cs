using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PTK_EnviroCollidersPreview : EditorWindow
{
    private bool collidersVisible = false; // Track the visibility state
    private List<GameObject> objectsWithoutColliders = new List<GameObject>(); // List to hold objects without colliders
    private List<GameObject> collidersWithoutLayers = new List<GameObject>(); // List to hold objects without colliders

    // Use this variable to store the original state of visibleLayers
    // to restore it when the editor window is closed.
    private int originalVisibleLayers; 
    private Vector2 scrollPositionWithoutColliders;
    private Vector2 scrollPositionWithoutLayers;

    static private bool bShowColliderMeshes = false;

    [MenuItem("PixelTools/EnviroTools/Enviro Colliders Preview")]
    public static void ShowWindow()
    {
        var window = GetWindow<PTK_EnviroCollidersPreview>("Enviro Colliders Preview");
        window.originalVisibleLayers = Tools.visibleLayers;
        window.minSize = new Vector2(300, 200); // Set a minimum size for the window for better layout

        PTK_EnviroCollidersPreview.bShowColliderMeshes = true;
        window.ShowColliderMeshesWithMaterials(PTK_EnviroCollidersPreview.bShowColliderMeshes);

    }

    private void OnFocus()
    {
        UpdateObjectsWithoutCollidersList();
    }

    Collider lastCollider = null;
    bool bLastColliderEnabled = false;
    int iLastLayer = -1;
    // to refresh on adding/removing collider or layer change
    private void OnInspectorUpdate()
    {
        if(Selection.activeGameObject != null)
        {

            Collider selectedCol = Selection.activeGameObject.GetComponent<Collider>();
            if (selectedCol != lastCollider)
            {
                UpdateObjectsWithoutCollidersList();
                this.Repaint();
            }

            if (selectedCol != null)
            {
                if (selectedCol.enabled != bLastColliderEnabled)
                {
                    UpdateObjectsWithoutCollidersList();
                    this.Repaint();
                }
            }

            if(Selection.activeGameObject.layer != iLastLayer)
            {
                UpdateObjectsWithoutCollidersList();
                this.Repaint();
            }

            lastCollider = selectedCol;

            if (lastCollider != null)
                bLastColliderEnabled = lastCollider.enabled;
            else
                bLastColliderEnabled = false;

            iLastLayer = Selection.activeGameObject.layer;
        }
    }

    void RenderMeshesWithMaterialsButtononsToShowUI()
    {
        GUILayout.BeginHorizontal();
        if (bShowColliderMeshes == true)
        {
            GUI.color = Color.green;
            EditorGUILayout.HelpBox("Meshes with Collider Materials - Visible", MessageType.None);
            GUI.color = Color.white;
        }
        else
        {

            GUI.color = Color.yellow;
            EditorGUILayout.HelpBox("Meshes with Collider Materials - Hidden", MessageType.None);
            GUI.color = Color.white;
        }

        if (GUILayout.Button("Show"))
        {
            bShowColliderMeshes = true;
            ShowColliderMeshesWithMaterials(bShowColliderMeshes);
        }

        if (GUILayout.Button("Hide"))
        {
            bShowColliderMeshes = false;
            ShowColliderMeshesWithMaterials(bShowColliderMeshes);
        }

        GUILayout.EndHorizontal();

        GUI.enabled = false;
        for (int i = 0; i < colliderMaterials.Count; i++)
        {
            if (colliderMaterials[i] != null)
                EditorGUILayout.ObjectField(colliderMaterials[i].name, colliderMaterials[i], typeof(Material), true);
            else
                EditorGUILayout.ObjectField("Mat " + (i + 1), colliderMaterials[i], typeof(Material), true);
        }
        GUI.enabled = true;
    }
    private void OnGUI()
    {
        GUILayout.Space(10); // Add some space for aesthetics

        // Toggle button for showing/hiding colliders
        if (!collidersVisible)
        {
            EditorGUILayout.HelpBox("Click the button below to view environment colliders only and look for holes or places without colliders.", MessageType.Info);

            if (GUILayout.Button("View Enviro Colliders Only (Debug)", GUILayout.Height(40)))
            {
                ShowEnvironmentColliders();
                collidersVisible = true;
                UpdateObjectsWithoutCollidersList();
            }


            GUILayout.Space(10);

        }
        else
        {
            GUI.color = Color.green;
            EditorGUILayout.HelpBox("Objects only with Colliding LAYER presented.", MessageType.Info);
            GUI.color = Color.white;

            if (GUILayout.Button("Disable Debug View", GUILayout.Height(40)))
            {
                DisableEnvironmentCollidersPreview();
                collidersVisible = false;
            }

         }


        GUILayout.Space(10);

        RenderMeshesWithMaterialsButtononsToShowUI();

        GUILayout.Space(10);

        if (GUILayout.Button("Refresh & Validate Colliders"))
        {
            UpdateObjectsWithoutCollidersList();
        }

        GUILayout.Space(10);

        if (objectsWithoutColliders.Count == 0)
        {

            EditorGUILayout.HelpBox("Missing Colliders - None", MessageType.None);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Every object assigned to Enviro Layers comes with a collider. We didn't find any objects missing colliders.", MessageType.Info);
            GUI.color = Color.white;


        }


        if (collidersWithoutLayers.Count == 0)
        {
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("Missing Layers - None", MessageType.None);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Every object assigned to Extra Colliders Parent has set layer. We didn't find any objects missing layer.", MessageType.Info);
            GUI.color = Color.white;

        }

        GUILayout.Space(10);


        if (objectsWithoutColliders.Count != 0)
        {
            // Display objects without colliders
            GUI.color = Color.red;
            EditorGUILayout.HelpBox("Objects without colliders:", MessageType.Error);
            GUI.color = Color.white;

            scrollPositionWithoutColliders = EditorGUILayout.BeginScrollView(scrollPositionWithoutColliders, GUILayout.Height(Mathf.Min(objectsWithoutColliders.Count * 25, 100)));

            foreach (GameObject obj in objectsWithoutColliders)
            {
                EditorGUILayout.ObjectField(obj.name, obj, typeof(GameObject), true);
            }

            EditorGUILayout.EndScrollView();
        }

        GUILayout.Space(25);


        if (collidersWithoutLayers.Count != 0)
        {
            // Colliders Without Correct Layer
            GUI.color = Color.yellow;
            EditorGUILayout.HelpBox("Colliders without specified layers:", MessageType.Error);
            GUI.color = Color.white;


            scrollPositionWithoutLayers = EditorGUILayout.BeginScrollView(scrollPositionWithoutLayers, GUILayout.Height(Mathf.Min(collidersWithoutLayers.Count * 25, 100)));
            foreach (GameObject obj in collidersWithoutLayers)
            {
                EditorGUILayout.ObjectField(obj.name, obj, typeof(GameObject), true);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    List<Material> colliderMaterials = new List<Material>();

    void ShowColliderMeshesWithMaterials(bool bShow)
    {
        colliderMaterials.Clear();

        string strColliderMaterialsPath = "Assets/Workshop_Module/CustomTracks/Material/ColliderMaterials/";
        var mat1 = AssetDatabase.LoadAssetAtPath<Material>(strColliderMaterialsPath + "AlwaysRespawnCollider.mat"); 
        var mat2 = AssetDatabase.LoadAssetAtPath<Material>(strColliderMaterialsPath + "FlatRespawnCollider.mat");
        var mat3 = AssetDatabase.LoadAssetAtPath<Material>(strColliderMaterialsPath + "GroundCollider.mat");
        var mat4 = AssetDatabase.LoadAssetAtPath<Material>(strColliderMaterialsPath + "WallCollider.mat");

        if (mat1 == null)
            Debug.LogError("Can't find collider material in path: " + strColliderMaterialsPath + "AlwaysRespawnCollider.mat");
        if (mat2 == null)
            Debug.LogError("Can't find collider material in path: " + strColliderMaterialsPath + "FlatRespawnCollider.mat");
        if (mat3 == null)
            Debug.LogError("Can't find collider material in path: " + strColliderMaterialsPath + "GroundCollider.mat");
        if (mat4 == null)
            Debug.LogError("Can't find collider material in path: " + strColliderMaterialsPath + "WallCollider.mat");

        colliderMaterials.Add(mat1);
        colliderMaterials.Add(mat2);
        colliderMaterials.Add(mat3);
        colliderMaterials.Add(mat4);

        var MeshRenderersAll = GameObject.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for(int i=0;i< MeshRenderersAll.Length;i++)
        {
            if(MeshRenderersAll[i].sharedMaterial != null)
            {
                if(colliderMaterials.Contains(MeshRenderersAll[i].sharedMaterial ) == true)
                {
                    MeshRenderersAll[i].enabled = bShow;
                }
            }
        }
    }

    int iEnviroColliders = (1 << 8) | (1 << 10) | (1 << 11) | (1 << 30);
    void ShowEnvironmentColliders()
    {
        // Create a bitmask that only includes layers 8, 10, 11, 30
        Tools.visibleLayers = iEnviroColliders;
        SceneView.RepaintAll();
    }

    void DisableEnvironmentCollidersPreview()
    {
        // Reset visibleLayers to the original state
        Tools.visibleLayers = -1;
        SceneView.RepaintAll();
    }

    private void OnDestroy()
    {
        // Ensure we restore the original layer visibility when the window is closed
        Tools.visibleLayers = -1;

    }

    private void UpdateObjectsWithoutCollidersList()
    {
        objectsWithoutColliders.Clear();
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            // Check if the object is in one of the visible layers and does not have a Collider component
            if (((1 << obj.layer) & iEnviroColliders) != 0)
            {
                if(obj.GetComponent<Collider>() == null || obj.GetComponent<Collider>().enabled == false)
                    objectsWithoutColliders.Add(obj);
            }
        }

        collidersWithoutLayers.Clear();

        PTK_ModTrack modTrack = GameObject.FindObjectOfType<PTK_ModTrack>();
        if(modTrack != null)
        {
            GameObject extraCollidersParent = GameObject.FindObjectOfType<PTK_ModTrack>().extraCollidersParent;
            List<GameObject> extraColliders = new List<GameObject>();
            for (int i = 0; i < extraCollidersParent.transform.childCount; i++)
            {
                // types
                Transform typeParent = extraCollidersParent.transform.GetChild(i);
                Transform[] childEnviroColliders = typeParent.GetComponentsInChildren<Transform>();

                foreach (Transform childTransform in childEnviroColliders)
                {
                    if (childTransform == typeParent)
                        continue;

                    // object that is inside Extra colliders doesnt have Collider component
                    if (childTransform.GetComponent<Collider>() == null)
                        objectsWithoutColliders.Add(childTransform.gameObject);
                    
                }
            }


            // colliders without layers - only on enviro scene chceck
            Scene s = SceneManager.GetSceneByName(modTrack.gameObject.scene.name);

            GameObject[] gameObjectsInTrackEnviroScene = s.GetRootGameObjects();
            for (int i = 0; i < gameObjectsInTrackEnviroScene.Length; i++)
            {
                Collider[] colliders = gameObjectsInTrackEnviroScene[i].GetComponentsInChildren<Collider>();

                foreach(Collider colliderTransform in colliders)
                {
                    // Check if the object's layer is NOT one of the visible layers
                    // This means the object has a collider but is not on the correct layer
                    if (((1 << colliderTransform.gameObject.layer) & Tools.visibleLayers) == 0)
                    {
                        collidersWithoutLayers.Add(colliderTransform.gameObject);
                    }
                }
            }
        }



    }


}
