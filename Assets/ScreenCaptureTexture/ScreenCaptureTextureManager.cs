using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ScreenCaptureTextureManager : MonoBehaviour
{
	public static ScreenCaptureTextureManager Instance { get; private set; }

	private AndroidJavaClass UnityPlayer;
	private AndroidJavaObject UnityPlayerActivityWithMediaProjector;

	public bool startScreenCaptureOnStart;

	private Texture2D receivedTexture;
	private RenderTexture correctedTexture;
	public static RenderTexture ScreenCaptureTexture => Instance.correctedTexture;

	public UnityEvent<Texture> OnTextureInitialized = new();
	public UnityEvent OnScreenCaptureStarted = new();
	public UnityEvent OnScreenCapturePermissionDeclined = new();
	public UnityEvent OnScreenCaptureStopped = new();
	public UnityEvent OnNewFrame = new();

	public static readonly Vector2Int Size = new(1024, 1024);

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		UnityPlayerActivityWithMediaProjector = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		receivedTexture = new Texture2D(Size.x, Size.y, TextureFormat.RGBA32, 1, false);
		correctedTexture = new RenderTexture(Size.x, Size.y, 1, RenderTextureFormat.ARGB32, 1);
		correctedTexture.Create();

		OnTextureInitialized.Invoke(correctedTexture);

		if(startScreenCaptureOnStart)
		{
			StartScreenCapture();
		}
	}

	private byte[] GetLastFrameBytes()
	{
		return UnityPlayerActivityWithMediaProjector.Get<byte[]>("lastFrameBytes");
	}



	public void StartScreenCapture()
	{
		UnityPlayerActivityWithMediaProjector.Call("startScreenCaptureWithPermission", gameObject.name, Size.x, Size.y);
	}

	public void StopScreenCapture()
	{
		UnityPlayerActivityWithMediaProjector.Call("stopScreenCapture");
	}

	// Messages sent from android activity

	private void ScreenCaptureStarted()
	{
		OnScreenCaptureStarted.Invoke();
	}

	private void ScreenCapturePermissionDeclined()
	{
		OnScreenCapturePermissionDeclined.Invoke();
	}

	private void NewFrameAvailable()
	{
		byte[] frameBytes = GetLastFrameBytes();

		if (frameBytes == null || frameBytes.Length == 0) return;

		receivedTexture.LoadRawTextureData(frameBytes);
		receivedTexture.Apply();

		Graphics.Blit(receivedTexture, correctedTexture, new Vector2(1, -1), Vector2.zero);
	}

	private void ScreenCaptureStopped()
	{
		OnScreenCaptureStopped.Invoke();
	}
}
