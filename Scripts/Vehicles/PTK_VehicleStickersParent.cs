using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_VehicleStickersParent : MonoBehaviour
{
    public PTK_VehicleStickerInfo[] vehicleStickers;
    LODGroup lodGroup;
    // Start is called before the first frame update
    public void Initialize()
    {
        for (int i = 0; i < vehicleStickers.Length; i++)
            vehicleStickers[i].Initialize(); // initialize so 2nd layer is created to get mesh renderes from them too

        lodGroup = this.gameObject.AddComponent<LODGroup>();
        MeshRenderer[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>(true);

        // Create an array with one LOD level
        LOD[] lods = new LOD[1];

        // Assign the current MeshRenderer to the first (and only) LOD level
        // Note: You may need to adjust this based on how your MeshRenderers are structured
        lods[0] = new LOD(0.02f, meshRenderers);

        // Set the LODs to the LODGroup
        lodGroup.SetLODs(lods);

        // Optional: Adjust the LODGroup's fade mode and other settings as needed
        lodGroup.fadeMode = LODFadeMode.None;
        lodGroup.animateCrossFading = false;

        // This line ensures objects not visible within the LOD's screen size are culled
        lodGroup.RecalculateBounds();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
