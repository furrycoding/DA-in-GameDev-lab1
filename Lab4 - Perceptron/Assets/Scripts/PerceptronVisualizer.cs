using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptronVisualizer : MonoBehaviour
{
	public Perceptron Target;
	public Vector2Int OutputSize;
	public Rect InputRange;
	
	public Texture2D Output {
		get { CheckOutputTexture(); return output; }
	}
	
	private Texture2D output;
	
	
    public void UpdateOutput()
    {
        CheckOutputTexture();
		
		var pixels = new Color[OutputSize.x * OutputSize.y];
		var idx = 0;
		for (var y = 0; y < OutputSize.y; y++)
			for (var x = 0; x < OutputSize.x; x++) {
				var uv = new Vector2(x, y) / (OutputSize - Vector2.one);
				var p = InputRange.min + uv * InputRange.size;
				var v = (float)Target.CalcOutput(p.x, p.y, true);
				
				pixels[idx++] = new Color(v, 0, 0, 1);
			}
		
		output.SetPixels(pixels);
		output.Apply();
    }
	
	private void CheckOutputTexture() {
		if ((output != null) && (output.width == OutputSize.x) && (output.height == OutputSize.y))
			return;
		
		output = new Texture2D(OutputSize.x, OutputSize.y, TextureFormat.RHalf, 0, true);
		output.filterMode = FilterMode.Bilinear;
		output.wrapMode = TextureWrapMode.Clamp;
	}
}
