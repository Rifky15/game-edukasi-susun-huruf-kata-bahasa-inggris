// using UnityEngine;
// using System.Collections.Generic;
// using System.IO;

// namespace GameEdu.Utils
// {
//     public static class ImageCacheManager
//     {
//         private static Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
//         private static string cachePath = Path.Combine(Application.persistentDataPath, "imageCache");

//         static ImageCacheManager()
//         {
//             if (!Directory.Exists(cachePath))
//                 Directory.CreateDirectory(cachePath);

//             // Preload all cached images
//             // LoadAllCachedImages();
//         }

//         public static bool HasImage(string fileName) => imageCache.ContainsKey(fileName) || File.Exists(GetFilePath(fileName));

//         public static Texture2D GetImage(string fileName)
//         {
//             if (imageCache.TryGetValue(fileName, out Texture2D cachedTex))
//                 return cachedTex;

//             string path = GetFilePath(fileName);
//             if (File.Exists(path))
//             {
//                 try
//                 {
//                     byte[] data = File.ReadAllBytes(path);
//                     Texture2D tex = new Texture2D(2, 2);
//                     if (tex.LoadImage(data) && tex.width > 0 && tex.height > 0)
//                     {
//                         imageCache[fileName] = tex;
//                         return tex;
//                     }
//                     else
//                     {
//                         Debug.LogWarning($"Failed to load image: {fileName}, deleting corrupt file.");
//                         File.Delete(path); // Hapus file corrupt agar bisa di-download ulang nanti
//                     }
//                 }
//                 catch (System.Exception ex)
//                 {
//                     Debug.LogError($"Error loading image {fileName}: {ex.Message}");
//                 }
//             }

//             return null;
//         }

//         public static void AddImage(string fileName, Texture2D texture)
//         {
//             if (!imageCache.ContainsKey(fileName))
//                 imageCache[fileName] = texture;

//             string path = GetFilePath(fileName);

//             if (!File.Exists(path))
//             {
//                 byte[] data = texture.EncodeToPNG();
//                 File.WriteAllBytes(path, data);
//             }
//         }

//         public static void LoadAllCachedImages()
//         {
//             string[] files = Directory.GetFiles(cachePath, "*.png");

//             foreach (string file in files)
//             {
//                 string fileName = Path.GetFileName(file);

//                 if (!imageCache.ContainsKey(fileName))
//                 {
//                     byte[] data = File.ReadAllBytes(file);
//                     Texture2D tex = new Texture2D(2, 2);
//                     if (tex.LoadImage(data))
//                         imageCache[fileName] = tex;
//                 }
//             }
//         }

//         public static void UnloadImage(string fileName)
//         {
//             if (imageCache.TryGetValue(fileName, out Texture2D tex))
//             {
//                 Object.Destroy(tex);
//                 imageCache.Remove(fileName);
//             }
//         }


//         private static string GetFilePath(string fileName)
//         {
//             // Pastikan filename aman
//             string validFileName = fileName.Replace("https://", "")
//                                         .Replace(":", "_")
//                                         .Replace("/", "_")
//                                         .Replace("?", "_")
//                                         .Replace("&", "_");
//             return Path.Combine(cachePath, validFileName + ".png");
//         }

//         public static string GetSafeFileName(string url)
//         {
//             return url.Replace("https://", "")
//                     .Replace("http://", "")
//                     .Replace(":", "_")
//                     .Replace("/", "_")
//                     .Replace("?", "_")
//                     .Replace("&", "_")
//                     .Replace("=", "_");
//         }


//     }
// }

using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace GameEdu.Utils
{
    public static class ImageCacheManager
    {
        private static Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
        private static readonly string cachePath = Path.Combine(Application.persistentDataPath, "imageCache");

        static ImageCacheManager()
        {
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);
        }

        public static bool HasImage(string fileName)
        {
            return imageCache.ContainsKey(fileName) || File.Exists(GetFilePath(fileName));
        }

        public static Texture2D GetImage(string fileName)
        {
            if (imageCache.TryGetValue(fileName, out Texture2D cachedTex))
                return cachedTex;

            string path = GetFilePath(fileName);
            if (File.Exists(path))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(path);
                    if (fileInfo.Length == 0)
                    {
                        Debug.LogWarning($"Image file '{fileName}' is empty. Deleting...");
                        File.Delete(path);
                        return null;
                    }

                    byte[] data = File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(data) && tex.width > 0 && tex.height > 0)
                    {
                        imageCache[fileName] = tex;
                        return tex;
                    }
                    else
                    {
                        Debug.LogWarning($"Image file '{fileName}' is corrupt or invalid. Deleting...");
                        File.Delete(path);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error loading image '{fileName}': {ex.Message}");
                    try { File.Delete(path); } catch { }
                }
            }

            return null;
        }

        public static void AddImage(string fileName, Texture2D texture)
        {
            if (!imageCache.ContainsKey(fileName))
                imageCache[fileName] = texture;

            string path = GetFilePath(fileName);

            if (!File.Exists(path))
            {
                try
                {
                    byte[] data = texture.EncodeToPNG();
                    File.WriteAllBytes(path, data);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error saving image '{fileName}': {ex.Message}");
                }
            }
        }

        public static void LoadAllCachedImages()
        {
            string[] files = Directory.GetFiles(cachePath, "*.png");

            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                if (!imageCache.ContainsKey(fileName))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.Length == 0) continue;

                        byte[] data = File.ReadAllBytes(file);
                        Texture2D tex = new Texture2D(2, 2);
                        if (tex.LoadImage(data) && tex.width > 0 && tex.height > 0)
                            imageCache[fileName] = tex;
                    }
                    catch
                    {
                        // Lewati file yang rusak
                    }
                }
            }
        }

        public static void UnloadImage(string fileName)
        {
            if (imageCache.TryGetValue(fileName, out Texture2D tex))
            {
                Object.Destroy(tex);
                imageCache.Remove(fileName);
            }
        }

        public static void ClearCache()
        {
            foreach (var tex in imageCache.Values)
            {
                Object.Destroy(tex);
            }
            imageCache.Clear();
        }

        private static string GetFilePath(string fileName)
        {
            string validFileName = SanitizeFileName(fileName);
            return Path.Combine(cachePath, validFileName + ".png");
        }

        public static string GetSafeFileName(string url)
        {
            return SanitizeFileName(url);
        }

        private static string SanitizeFileName(string input)
        {
            // Membersihkan karakter ilegal namun tetap izinkan spasi
            char[] invalids = Path.GetInvalidFileNameChars();
            foreach (char c in invalids)
                input = input.Replace(c.ToString(), "_");

            return input.Replace("https://", "")
                        .Replace("http://", "")
                        .Replace(":", "_")
                        .Replace("/", "_")
                        .Replace("?", "_")
                        .Replace("&", "_")
                        .Replace("=", "_");
        }
    }
}
