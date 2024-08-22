using UnityEngine;

public class MaterialSetter : MonoBehaviour
{
	[SerializeField] private Material material;

	private void Start()
	{
		material.mainTexture = MediaProjectionTextureGetter.DisplayTexture;
	}
}
