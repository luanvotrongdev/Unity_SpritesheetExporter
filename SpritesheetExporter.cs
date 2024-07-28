#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lulu.SpriteSheetExporter
{
    namespace PixiJS
    {
        [System.Serializable]
        class Rect
        {
            public int x;
            public int y;
            public int w;
            public int h;

            static public Rect GetRect(UnityEngine.Rect rect)
            {
                return new Rect{
                    x = (int)rect.x,
                    y = (int)rect.y,
                    w = (int)rect.width,
                    h = (int)rect.height
                };
            }

            static public Rect GetSize(UnityEngine.Rect rect)
            {
                return new Rect{
                    x = 0,
                    y = 0,
                    w = (int)rect.width,
                    h = (int)rect.height
                };
            }
        }

        [System.Serializable]
        class Size
        {
            public int w;
            public int h;

            static public Size From(UnityEngine.Rect rect)
            {
                return new Size{
                    w = (int)rect.width,
                    h = (int)rect.height
                };
            }
        }

        [System.Serializable]
        class Frame
        {
            public Rect frame;
            public Size sourcerSize;
            public Rect spriteSourceSize;
        }

        [System.Serializable]
        class Meta
        {
            public string image;
            public string format;
            public Size size;
            public float scale;
        }

        [System.Serializable]
        class SpriteSheetData
        {
            public Frame[] frames;
            public Meta meta;
        }
    }

    static class EditorContextMenu
    {
        [MenuItem("Assets/SpriteSheet Export/PixiJS", true, 115)]
        private static bool CreateColorGradientValidate()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/SpriteSheet Export/PixiJS")]
        public static void CreateColorGradient(MenuCommand context)
        {
            PixiJS.SpriteSheetData spriteSheetData =  new PixiJS.SpriteSheetData();
            Vector2 scaler = Vector2.one;
            Texture2D texture2D = Selection.activeObject as Texture2D;
            string texturePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (textureImporter == null) 
                return;
            
            object[] args = new object[2] { 0, 0 };
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(textureImporter, args);
            scaler.x = (int)args[0] / (float)texture2D.width;
            scaler.y = (int)args[1] / (float)texture2D.height;
            spriteSheetData.meta = new PixiJS.Meta{
                image = texture2D.name,
                format = texture2D.graphicsFormat.ToString(),
                size = new PixiJS.Size{
                    w = (int)args[0],
                    h = (int)args[1]
                },
                scale = 1
            };

            Object[] data = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            if(data == null)
                return;
            List<PixiJS.Frame> frames = new List<PixiJS.Frame>();
            Rect rect;
            foreach (Object obj in data)
            {
                Sprite sprite = obj as Sprite;
                if(sprite == null) continue;

                rect = sprite.rect;
                frames.Add(new PixiJS.Frame{
                    frame = PixiJS.Rect.GetRect(sprite.rect),
                    sourcerSize = PixiJS.Size.From(sprite.rect),
                    spriteSourceSize = PixiJS.Rect.GetSize(sprite.rect)
                });
            }
            spriteSheetData.frames = frames.ToArray();
            EditorGUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(spriteSheetData);
        }
    }
}
#endif