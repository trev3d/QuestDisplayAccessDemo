using OVR.OpenVR;
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

		externalTexture.filterMode = FilterMode.Bilinear;
		externalTexture.wrapMode = TextureWrapMode.Clamp;

		material.SetTexture("_MainTex", externalTexture);
	}

	private void LateUpdate()
	{
		if(textureID == 0) return;

		Debug.Log("Getting new frame");
		
		
		externalTexture.UpdateExternalTexture(new System.IntPtr(textureID));
	}
}
