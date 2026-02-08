// Modified file from project https://github.com/audiamus/BookLibConnect
// Original copyright (C) 2021 - 2023 by audiamus
// Originally licensed by audiamus under the GNU General Public License v3.0 (GPL-3.0)
// Slighly modernized and adapted to ProjectMover by MemoTrap
// Still licensed under GPL-3.0

#pragma warning disable IDE0130
#pragma warning disable IDE1006

#nullable disable

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace core.audiamus.aux {
  public static class JsonSerialization {
    private static readonly JsonSerializerOptions __jsonSerializerOptions = new () {
      WriteIndented = true,
      ReadCommentHandling = JsonCommentHandling.Skip,
      AllowTrailingCommas = true,
      Converters ={
        new JsonStringEnumConverter()
      },
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string ToJsonString<T> (this T obj) {
      return JsonSerializer.Serialize<T> (obj, __jsonSerializerOptions);
    }

    public static void ToJsonFile<T> (this T obj, string path) {
      using var fs = new FileStream (path, FileMode.Create);
      var task = Task.Run (async () => await JsonSerializer.SerializeAsync (fs, obj, __jsonSerializerOptions));
      task.Wait ();
    }

    public static T FromJsonString<T> (this string json) {
      return JsonSerializer.Deserialize<T> (json, __jsonSerializerOptions);
    }

    public static T FromJsonFile<T> (this string path) {
      using var fs = new FileStream (path, FileMode.Open, FileAccess.Read);
      Task<T> task = Task.Run (async () => await JsonSerializer.DeserializeAsync<T> (fs, __jsonSerializerOptions));
      return task.Result;
    }

  }
}
