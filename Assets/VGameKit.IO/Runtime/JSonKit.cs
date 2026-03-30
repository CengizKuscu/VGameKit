using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace VGameKit.IO.Runtime
{
    /// <summary>
    /// Provides utility methods for saving and loading JSON files in a cross-platform Unity environment.
    /// </summary>
    public static class JSonKit
    {
        /// <summary>
        /// Serializes the specified value to JSON and saves it to the given file path.
        /// Creates the directory if it does not exist.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="filePath">The file path to save the JSON to.</param>
        /// <param name="value">The value to serialize and save.</param>
        /// <param name="formatting">Optional. The formatting to use when serializing. Defaults to none.</param>
        public static void Save<T>(string filePath, T value, Formatting formatting = Formatting.None)
        {
            string content = JsonConvert.SerializeObject(value, formatting);
            string name = Path.GetFileName(filePath);
            string path = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            System.IO.File.WriteAllText(filePath, content, new UTF8Encoding(false));
        }

        /// <summary>
        /// Loads and deserializes JSON from the specified file path into an object of type T.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="filePath">The file path to load the JSON from.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The deserialized object, or default if the file is empty.</returns>
        public static async UniTask<T> LoadAsync<T>(string filePath, CancellationToken token)
        {
            string json = await ReadAllTextAsync(filePath, token);

            if (json == "")
                return default;

            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Reads all text from the specified file path, handling platform-specific requirements.
        /// </summary>
        /// <param name="filePath">The file path to read from.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The file contents as a string.</returns>
        public static async UniTask<string> ReadAllTextAsync(string filePath, CancellationToken token)
        {
    #if UNITY_ANDROID
            return await ReadAllTextOnAndroidAsync(filePath, token);
    #else
            return File.ReadAllText(filePath);
    #endif
        }

        /// <summary>
        /// Reads all text from a file on Android, supporting both local and URI-based paths.
        /// </summary>
        /// <param name="filePath">The file path or URI to read from.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The file contents as a string.</returns>
        private static async UniTask<string> ReadAllTextOnAndroidAsync(string filePath, CancellationToken token)
        {
            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                var req = UnityWebRequest.Get(filePath);
                await req.SendWebRequest().WithCancellation(token);
                return req.downloadHandler.text;
            }
            else
            {
                return File.ReadAllText(filePath);
            }
        }
    }
}
