using UnityEngine;

public class DisplayReceiver : MonoBehaviour
{
	private const string Name = "DisplayReceiver";

	private Texture2D externalTexture;
	private int textureID;
	[SerializeField] Material material;

	private AndroidJavaClass UnityPlayer;
	private AndroidJavaObject UnityPlayerActivityWithMediaProjection;
	

	private void Start()
	{
		gameObject.name = Name;

		UnityPlayer = new("com.unity3d.player.UnityPlayer");
		UnityPlayerActivityWithMediaProjection = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		externalTexture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };

		externalTexture.Apply();

		

		material.mainTexture = externalTexture;

		Debug.Log("Created texture with id " + externalTexture.GetNativeTexturePtr());
	}

	private bool initialized = false;
	private void Update()
	{
		if (initialized)
		{
			Debug.Log("Polling for new surface texture update");
			UnityPlayerActivityWithMediaProjection.Call("requestSurfaceTextureUpdate");
			GL.InvalidateState();
		}
		else {
			Debug.Log("Initializing surface and requesting screen capture from Unity");
			UnityPlayerActivityWithMediaProjection.Call("initSurface", externalTexture.GetNativeTexturePtr().ToInt32(), externalTexture.width, externalTexture.height);
			UnityPlayerActivityWithMediaProjection.Call("requestScreenCapturePermissionAndStart");

			initialized = true;
		}
	}
}
