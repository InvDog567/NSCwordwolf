using UnityEngine;
 
// Static class — holds all settings values across scenes
// Any script in any scene can read from here
public static class GameSettings
{
    public static float MasterVolume  = 0.5f;
    public static float MusicVolume   = 1f;
    public static float SFXVolume     = 1f;
    public static float DialogVolume  = 1f;
    public static float SensitivityX  = 2f;
    public static float SensitivityY  = 2f;
 
    // Call this once at startup (StartManager does this)
    public static void LoadFromPrefs()
    {
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        MusicVolume  = PlayerPrefs.GetFloat("MusicVolume",  1f);
        SFXVolume    = PlayerPrefs.GetFloat("SFXVolume",    1f);
        DialogVolume = PlayerPrefs.GetFloat("DialogVolume", 1f);
        SensitivityX = PlayerPrefs.GetFloat("SensitivityX", 2f);
        SensitivityY = PlayerPrefs.GetFloat("SensitivityY", 2f);
    }
}