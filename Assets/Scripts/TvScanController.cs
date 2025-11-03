using UnityEngine;

public class TVScanMaterialController : MonoBehaviour
{
	public string Unscaled = "_UnscaledTime";
	public Material material;

	void Update()
	{
	   material.SetFloat("_UnscaledTime", Time.unscaledTime);
            
	}
}