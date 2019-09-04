
namespace SuperSystems.UnityBuild
{

[System.Serializable]
public class BuildReleaseType
{
    public string typeName = string.Empty;
    public string bundleIndentifier = string.Empty;
    public string companyName = string.Empty;
    public string productName = string.Empty;

    public bool developmentBuild = false;
    public bool allowDebugging = false;
    public bool enableHeadlessMode = false;
    public string customDefines = string.Empty;
	public bool virtualRealitySupported;
	public string[] virtualRealitySDKs;

    public SceneList sceneList = new SceneList();
}

}