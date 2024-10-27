using System;
using UnityEngine;
using UnityEngine.Events;

namespace Anaglyph.DisplayCapture
{
	[DefaultExecutionOrder(-1000)]
	public class DisplayCaptureManager : MonoBehaviour
	{
		private unsafe sbyte* imageData;
		private int bufferSize;

		private class AndroidInterface
		{
			private AndroidJavaClass androidClass;
			private AndroidJavaObject androidInstance;

			public AndroidInterface(GameObject messageReceiver, int textureWidth, int textureHeight)
			{
				androidClass = new AndroidJavaClass("com.trev3d.DisplayCaptureManager");
				androidInstance = androidClass.CallStatic<AndroidJavaObject>("getInstance");
				androidInstance.Call("setup", messageReceiver.name, textureWidth, textureHeight);
			}

			public void RequestCapture() => androidInstance.Call("requestCapture");
			public void StopCapture() => androidInstance.Call("stopCapture");

			public unsafe sbyte* GetByteBuffer()
			{
				AndroidJavaObject byteBuffer = androidInstance.Call<AndroidJavaObject>("getByteBuffer");
				return AndroidJNI.GetDirectBufferAddress(byteBuffer.GetRawObject());
			}
		}

		private AndroidInterface androidInterface;

		public static DisplayCaptureManager Instance { get; private set; }

		private Texture2D screenTexture;
		private RenderTexture flipTexture;
		public Texture2D ScreenCaptureTexture => screenTexture;

		public bool startScreenCaptureOnStart = true;
		public bool flipTextureOnGPU = false;

		public UnityEvent<Texture2D> OnTextureInitialized = new();
		public UnityEvent OnScreenCaptureStarted = new();
		public UnityEvent OnScreenCapturePermissionDeclined = new();
		public UnityEvent OnScreenCaptureStopped = new();
		public UnityEvent OnNewFrame = new();

		public static readonly Vector2Int Size = new(1024, 1024);

		private void Awake()
		{
			Instance = this;
			screenTexture = new Texture2D(Size.x, Size.y, TextureFormat.RGBA32, 1, false);
		}

		private void Start()
		{
			androidInterface = new AndroidInterface(gameObject, Size.x, Size.y);

			flipTexture = new RenderTexture(Size.x, Size.y, 1, RenderTextureFormat.ARGB32, 1);
			flipTexture.Create();

			OnTextureInitialized.Invoke(screenTexture);

			if (startScreenCaptureOnStart)
			{
				StartScreenCapture();
			}
			bufferSize = Size.x * Size.y * 4; // RGBA_8888 format: 4 bytes per pixel
		}

		public void StartScreenCapture()
		{
			androidInterface.RequestCapture();
		}

		public void StopScreenCapture()
		{
			androidInterface.StopCapture();
		}


		// Messages sent from Android
		
		private unsafe void OnCaptureStarted()
		{
			OnScreenCaptureStarted.Invoke();
			imageData = androidInterface.GetByteBuffer();
		}

		private void OnPermissionDeclined()
		{
			OnScreenCapturePermissionDeclined.Invoke();
		}

		private unsafe void OnNewFrameAvailable()
		{
			if (imageData == default) return;
			screenTexture.LoadRawTextureData((IntPtr)imageData, bufferSize);
			screenTexture.Apply();

			if (flipTextureOnGPU)
			{
				Graphics.Blit(screenTexture, flipTexture, new Vector2(1, -1), Vector2.zero);
				Graphics.CopyTexture(flipTexture, screenTexture);
			}

			OnNewFrame.Invoke();
		}

		private void OnCaptureStopped()
		{
			OnScreenCaptureStopped.Invoke();
		}
	}
}