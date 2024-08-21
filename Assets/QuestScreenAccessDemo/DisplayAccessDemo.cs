using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class DisplayAccessDemo : MonoBehaviour
{
	private byte[] imageBytes;
	private AndroidJavaClass unityPlayerActivity;
	private MeshRenderer meshRenderer;
	private Texture2D texture;

	private void Start()
	{
		texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, 1, false);
		unityPlayerActivity = new AndroidJavaClass("com.trev3d.UnityPlayerActivityWithMediaProjector");
		meshRenderer = GetComponent<MeshRenderer>();
	}

	// Update is called once per frame
	void Update()
	{
		imageBytes = unityPlayerActivity.CallStatic<byte[]>("getLastRecordedImage");

		if (imageBytes == null || imageBytes.Length == 0) return;

		texture.LoadRawTextureData(imageBytes);
		texture.Apply();
		meshRenderer.sharedMaterial.SetTexture("_MainTex", texture);
	}
}
