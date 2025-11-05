using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SVExtract.Net
{
  public static class ApiClient
  {
    // Default; will be overridden by AppConfig in Awake
    public static string Base = "https://sv-extract-api-production.up.railway.app";

    public static void Configure(string baseUrl) {
      if (!string.IsNullOrEmpty(baseUrl)) Base = baseUrl.TrimEnd('/');
    }

    public static async Task<string> Get(string path) {
      using var req = UnityWebRequest.Get($"{Base}{path}");
      req.SetRequestHeader("X-Client", "sv-extract-1");
#if UNITY_2020_3_OR_NEWER
      var op = req.SendWebRequest();
      while (!op.isDone) await Task.Yield();
      if (req.result != UnityWebRequest.Result.Success)
        throw new Exception(req.error);
#else
      await req.SendWebRequest();
      if (req.isNetworkError || req.isHttpError)
        throw new Exception(req.error);
#endif
      return req.downloadHandler.text;
    }
  }
}
