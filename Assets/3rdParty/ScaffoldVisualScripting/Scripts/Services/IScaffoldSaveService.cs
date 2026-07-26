using UnityEngine;

namespace Scaffold
{
    public interface IScaffoldSaveService
    {
        void SetInt(string key, int value);
        int GetInt(string key, int defaultValue = 0);

        void SetFloat(string key, float value);
        float GetFloat(string key, float defaultValue = 0f);

        void SetString(string key, string value);
        string GetString(string key, string defaultValue = "");

        bool HasKey(string key);
        void DeleteKey(string key);
        void DeleteAll();
        void Save();
    }

    public class DefaultPlayerPrefsSaveService : IScaffoldSaveService
    {
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public int GetInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);

        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
        public float GetFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);

        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
        public string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);

        public bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public void DeleteAll() => PlayerPrefs.DeleteAll();
        public void Save() => PlayerPrefs.Save();
    }
}
