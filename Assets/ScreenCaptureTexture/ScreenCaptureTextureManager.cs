using System;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-1000)]
public class ScreenCaptureTextureManager : MonoBehaviour
{
	public enum FlipMethod
	{
		DontFlip = 0,
		GPUOnly = 1,
		CPUOnly = 2,
		Both = 3,
	}

	private AndroidJavaObject byteBuffer;
	private unsafe sbyte* imageData;
	private int bufferSize;
	public static ScreenCaptureTextureManager Instance { get; private set; }

	private AndroidJavaClass UnityPlayer;
	private AndroidJavaObject UnityPlayerActivityWithMediaProjector;

	private Texture2D screenTexture;
	private RenderTexture flipTexture;
	public static Texture2D ScreenCaptureTexture => Instance.screenTexture;

	public bool startScreenCaptureOnStart = true;
	public FlipMethod flipMethod = FlipMethod.GPUOnly;

	public UnityEvent<Texture2D> OnTextureInitialized = new();
	public UnityEvent OnScreenCaptureStarted = new();
	public UnityEvent OnScreenCapturePermissionDeclined = new();
	public UnityEvent OnScreenCaptureStopped = new();
	public UnityEvent OnNewFrameIncoming = new();
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

		screenTexture = new Texture2D(Size.x, Size.y, TextureFormat.RGBA32, 1, false);
		flipTexture = new RenderTexture(Size.x, Size.y, 1, RenderTextureFormat.ARGB32, 1);
		flipTexture.Create();

		OnTextureInitialized.Invoke(screenTexture);

		if(startScreenCaptureOnStart)
		{
			StartScreenCapture();
		}
		bufferSize = Size.x * Size.y * 4; // RGBA_8888 format: 4 bytes per pixel
	}

	private unsafe void InitializeByteBufferRetrieved()
	{
		// Retrieve the ByteBuffer from Java and cache it
		byteBuffer = UnityPlayerActivityWithMediaProjector.Call<AndroidJavaObject>("getLastFrameBytesBuffer");

		// Get the memory address of the direct ByteBuffer
		imageData = AndroidJNI.GetDirectBufferAddress(byteBuffer.GetRawObject());
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
		InitializeByteBufferRetrieved();
	}

	private void ScreenCapturePermissionDeclined()
	{
		OnScreenCapturePermissionDeclined.Invoke();
	}

	private void NewFrameIncoming()
	{
		OnNewFrameIncoming.Invoke();
	}

	private unsafe void NewFrameAvailable()
	{
		if (imageData == default) return;
		screenTexture.LoadRawTextureData((IntPtr)imageData, bufferSize);
		screenTexture.Apply();

		switch(flipMethod)
		{
			case FlipMethod.DontFlip:
				break;

			case FlipMethod.GPUOnly:
				Graphics.Blit(screenTexture, flipTexture, new Vector2(1, -1), Vector2.zero);
				Graphics.CopyTexture(flipTexture, screenTexture);
				break;

			case FlipMethod.CPUOnly:
				FlipImageCPU(screenTexture);
				break;

			case FlipMethod.Both:

				Graphics.Blit(screenTexture, flipTexture, new Vector2(1, -1), Vector2.zero);
				Graphics.CopyTexture(flipTexture, screenTexture);

				FlipImageCPU(screenTexture);
				break;
		}

		RenderTexture previousylActive = RenderTexture.active;
		

		OnNewFrame.Invoke();
	}

	private void ScreenCaptureStopped()
	{
		OnScreenCaptureStopped.Invoke();
	}

	//https://gamedev.stackexchange.com/questions/203539/rotating-a-unity-texture2d-90-180-degrees-without-using-getpixels32-or-setpixels
	private static void FlipImageCPU(Texture2D tex)
	{
		int width = tex.width;
		int height = tex.height;

		var texels = tex.GetRawTextureData<Color32>();
		var copy = System.Buffers.ArrayPool<Color32>.Shared.Rent(texels.Length);
		Unity.Collections.NativeArray<Color32>.Copy(texels, copy, texels.Length);

		int address = 0;
		for (int newY = 0; newY < height; newY++)
		{
			for (int newX = 0; newX < width; newX++)
			{
				int oldX = newX;
				int oldY = height - newY - 1;

				texels[address++] = copy[oldY * width + oldX];
			}
		}

		System.Buffers.ArrayPool<Color32>.Shared.Return(copy);
	}
}
