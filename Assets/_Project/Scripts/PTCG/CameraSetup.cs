using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// カメラセットアップ用ユーティリティ
    /// </summary>
    public class CameraSetup : MonoBehaviour
    {
        private void Awake()
        {
            // シーンにMain Cameraが存在しない場合は作成
            if (Camera.main == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                Camera cam = cameraObject.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.transform.position = new Vector3(0, 1, -10);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;

                Debug.Log("Main Camera created by CameraSetup");
            }
        }
    }
}
