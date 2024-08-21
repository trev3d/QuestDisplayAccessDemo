using UnityEngine;

public class DisplayReceiver : MonoBehaviour
{
	private Texture2D externalTexture;
	private int textureID;

	[SerializeField] Material material;

	public void InitializeExternalTexture(string textureIdStr)
	{
		textureID = int.Parse(textureIdStr);
		int width = 1024;
		int height = 1024;

		Debug.Log($"Initializing texture with ID {textureID}");
		externalTexture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, new System.IntPtr(textureID));

		material.SetTexture("_MainTex", externalTexture);
	}

	public void OnFrameAvailable()
	{
		externalTexture.UpdateExternalTexture(new System.IntPtr(textureID));
	}
}
