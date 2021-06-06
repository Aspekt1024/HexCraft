using System.Collections;
using System.IO;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class PortraitTaker : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private bool isEnabled;
        [SerializeField] private int widthPixels = 3840;
        [SerializeField] private int heightPixels = 2160;
        [SerializeField] private KeyCode keyCode = KeyCode.O;
        [SerializeField] private string fileName = "image4k.png";
#pragma warning restore 649
        
        private void Update()
        {
            if (!isEnabled) return;
            
            if (Input.GetKeyDown(keyCode))
            {
                StartCoroutine(TakeShot());
            }
        }

        [ContextMenu("Take Portrait")]
        public void TakePortrait()
        {
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("Couldn't find main camera, no screenshot taken.");
                return;
            }
            var originalAspect = mainCam.aspect;
            
            var rt = new RenderTexture(widthPixels, heightPixels, 24);
            
            mainCam.targetTexture = rt;
            Texture2D ss = new Texture2D(widthPixels, heightPixels, TextureFormat.RGB24, false);
            float a = mainCam.aspect;
            mainCam.aspect = (float)widthPixels / heightPixels;
            mainCam.Render();
            RenderTexture.active = rt;
            ss.ReadPixels(new Rect(0, 0, widthPixels, heightPixels), 0, 0);
            mainCam.targetTexture = null;
            RenderTexture.active = null;
            mainCam.aspect = a;
            Destroy(rt);
            
            var bytes = ss.EncodeToPNG();

            var folderName = "Assets/Portrait";
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            File.WriteAllBytes(folderName + "/" + fileName, bytes);
            Debug.Log($"Screenshot saved in project directory: {folderName}/{fileName}");

            mainCam.aspect = originalAspect;
        }

        private IEnumerator TakeShot()
        {
            // We should only read the screen buffer after rendering is complete
            yield return new WaitForEndOfFrame();
            TakePortrait();
        }
    }
}