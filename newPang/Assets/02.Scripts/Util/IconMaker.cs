using System.IO;
using UnityEngine;


namespace SA.Utilities
{
    [ExecuteInEditMode]
    public class IconMaker : MonoBehaviour
    {
        public bool create;
        public RenderTexture ren;
        public Camera bakeCam;

        public string spriteName;
        public int number = 0;
        public int number2 = 0;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                CreateIcon();
                create = false;
                number2++;
                if (number2 >= 10)
                {
                    number2 = 0;
                    number++;
                }
            }
        }

        void CreateIcon()
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                spriteName = "icon";
            }

            string path = SaveLocation();
            path += spriteName;// + number + number2;

            bakeCam.targetTexture = ren;

            RenderTexture currentRT = RenderTexture.active;
            bakeCam.targetTexture.Release();
            RenderTexture.active = bakeCam.targetTexture;
            bakeCam.Render();

            Texture2D imgPng = new Texture2D(bakeCam.targetTexture.width, bakeCam.targetTexture.height, TextureFormat.ARGB32, false);
            imgPng.ReadPixels(new Rect(0, 0, bakeCam.targetTexture.width, bakeCam.targetTexture.height), 0, 0);
            imgPng.Apply();
            RenderTexture.active = currentRT;
            byte[] bytesPng = imgPng.EncodeToPNG();
            File.WriteAllBytes(path + ".png", bytesPng);
        }

        string SaveLocation()
        {
            string saveLocation = Application.streamingAssetsPath + "/Icons/";

            if (!Directory.Exists(saveLocation))
            {
                Directory.CreateDirectory(saveLocation);
            }

            return saveLocation;
        }
    }
}

