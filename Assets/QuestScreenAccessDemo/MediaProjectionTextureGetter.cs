using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MediaProjectionTextureGetter : MonoBehaviour
{
	private byte[] imageBytes;
	private AndroidJavaClass unityPlayerActivity;
	private Texture2D texture;

	public static MediaProjectionTextureGetter Instance { get; private set; }
	public static Texture2D DisplayTexture => Instance.texture;

	public event Action OnReceivedNewFrame = delegate { };

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		gameObject.name = "MediaProjectionTextureReceiver";
		texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, 1, false);
		unityPlayerActivity = new AndroidJavaClass("com.trev3d.UnityPlayerActivityWithMediaProjector");
	}

	public void OnNewFrameAvailable()
	{
		imageBytes = unityPlayerActivity.CallStatic<byte[]>("getLastRecordedImage");

		if (imageBytes == null || imageBytes.Length == 0) return;

		texture.LoadRawTextureData(imageBytes);
		texture.Apply();

		OnReceivedNewFrame.Invoke();
	}
}
