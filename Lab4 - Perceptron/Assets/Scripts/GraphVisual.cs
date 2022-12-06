using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


public class GraphVisual : MonoBehaviour
{
	public RectTransform root;
	public Font textFont;
	public Sprite background;
	public Material backgroundMaterial;
	public Sprite pointImage;
	public Color lineColor;
	
	public Vector2 markPadding = Vector2.one * 15;
	
	public bool fixAspectRatio;
	public float targetAspectRatio;
	public Vector2Int markCountPerAxis;
	public Rect range;

	public IEnumerable<Vector2> plotPoints;
	
	public Rect VisibleRange {
		get {
			var p = markPadding / (root.rect.size - 2 * markPadding) * range.size;
			var ret = new Rect();
			ret.min = range.min - p;
			ret.max = range.max + p;
			return ret;
		}
	}
	
#region Text and Image objects
	private Stack<Image> imageCache = new Stack<Image>();
	private Stack<Image> usedImages = new Stack<Image>();
	
	private Stack<Text> textCache = new Stack<Text>();
	private Stack<Text> usedTexts = new Stack<Text>();
	
	private int imageCount = 0;
	private int textCount = 0;

	private Image AllocImage() {
		if (imageCache.Count > 0) {
			var ret = imageCache.Pop();
			usedImages.Push(ret);
			return ret;
		}
		
		imageCount++;
		var img = new GameObject($"Graph Image {imageCount}").AddComponent<Image>();
		img.rectTransform.SetParent(root, false);
		img.rectTransform.pivot = Vector2.zero;
		usedImages.Push(img);
		return img;
	}
	
	private Text AllocText() {
		if (textCache.Count > 0) {
			var ret = textCache.Pop();
			usedTexts.Push(ret);
			return ret;
		}
		
		textCount++;
		var text = new GameObject($"Graph Text {textCount}").AddComponent<Text>();
		text.rectTransform.SetParent(root, false);
		text.font = textFont;
		usedTexts.Push(text);
		return text;
	}
	
	private void FreeAllImages() {
		foreach (var img in usedImages) {
			imageCache.Push(img);
			//Debug.Log("reclaimed "+img);
		}
		usedImages.Clear();
	}
	
	private void FreeAllText() {
		foreach (var text in usedTexts) {
			textCache.Push(text);
			//Debug.Log("reclaimed "+text);
		}
		usedTexts.Clear();
	}
	
	private void UpdateActive() {
		foreach (var img in usedImages)
			img.gameObject.SetActive(true);
		foreach (var text in usedTexts)
			text.gameObject.SetActive(true);
		foreach (var img in imageCache)
			img.gameObject.SetActive(false);
		foreach (var text in textCache)
			text.gameObject.SetActive(false);
	}
#endregion
	
	private void DrawMark(Vector3 pos, Vector3 value, Vector3 offset, Vector3 axisDir, Vector3 otherDir, float markDist, float textDist, float markWidth, float markHeight) {
		var markPos = offset + Vector3.Dot(pos, axisDir) * axisDir - markDist * otherDir;
		var valueOnAxis = Vector3.Dot(value, axisDir);
		
		var mark = AllocImage();
		mark.rectTransform.sizeDelta = markWidth * axisDir + markHeight * otherDir;
		mark.rectTransform.pivot = Vector2.one * 0.5f;
		mark.rectTransform.localPosition = markPos;
		
		var label = AllocText();
		label.alignment = TextAnchor.MiddleCenter;
		label.rectTransform.pivot = Vector2.one * 0.5f;
		label.rectTransform.localPosition = markPos - otherDir * textDist;
		label.text = $"{valueOnAxis}";
	}
	
	private void UpdateAxes() {
		var rect = root.rect;
		var offset = (Vector3)rect.min;
		
        var xAxis = AllocImage();
		xAxis.rectTransform.pivot = Vector2.zero;
		xAxis.rectTransform.localPosition = offset;
		xAxis.rectTransform.sizeDelta = new Vector2(rect.width, 5);
		
		var yAxis = AllocImage();
		yAxis.rectTransform.pivot = Vector2.zero;
		yAxis.rectTransform.localPosition = offset;
		yAxis.rectTransform.sizeDelta = new Vector2(5, rect.height);
		
		var spacing = (rect.size - 2*markPadding) / (markCountPerAxis - Vector2Int.one);
		var rangeSpacing = range.size / (markCountPerAxis - Vector2Int.one);
		
		for (var i = 0; i < Mathf.Max(markCountPerAxis.x, markCountPerAxis.y); i++) {
			var pos = markPadding + spacing * i;
			var value = range.min + rangeSpacing * i;
			
			if (i < markCountPerAxis.x)
				DrawMark(pos, value, offset, Vector2.right, Vector2.up, 20, 15, 7, 15);
			if (i < markCountPerAxis.y)
				DrawMark(pos, value, offset, Vector2.up, Vector2.right, 20, 15, 7, 15);
		}
	}
	
	private void UpdatePlot(IEnumerable<Vector2> points) {
		var rect = root.rect;
		
		var havePrevPoint = false;
		var prevPoint = Vector2.zero;
		foreach (var pt in points) {
			if (!range.Contains(pt)) {
				havePrevPoint = false;
				continue;
			}
			
			var normalized = (pt - range.min) / range.size;
			var local = rect.min + rect.size * normalized;
			
			if (havePrevPoint) {
				var delta = local - prevPoint;
				var len = delta.magnitude;
				delta /= len;
				var rot = Quaternion.FromToRotation(Vector2.right, delta);
				
				var line = AllocImage();
				line.color = lineColor;
				line.rectTransform.sizeDelta = new Vector2(len, 4);
				line.rectTransform.pivot = new Vector2(0, 0.5f);
				line.rectTransform.localPosition = prevPoint;
				line.rectTransform.localRotation = rot;
				line.sprite = null;
			}
			
			var point = AllocImage();
			point.rectTransform.sizeDelta = 8 * Vector2.one;
			point.rectTransform.pivot = 0.5f * Vector2.one;
			point.rectTransform.localPosition = local;
			point.rectTransform.localRotation = Quaternion.identity;
			point.sprite = pointImage;
			point.transform.SetAsLastSibling();
			
			havePrevPoint = true;
			prevPoint = local;
		}
	}

    private void UpdateBackground()
    {
		if (background == null)
			return;

		var bg = AllocImage();
		bg.rectTransform.pivot = Vector2.zero;
		bg.rectTransform.localPosition = root.rect.min;
        bg.rectTransform.sizeDelta = root.rect.size;
		bg.material = backgroundMaterial;
		bg.sprite = background;
    }




    void Start()
    {
		
    }

    void Update()
    {
		FreeAllImages();
		FreeAllText();

		var displaySize = root.rect.size - 2 * markPadding;
		if (fixAspectRatio) {
			if (targetAspectRatio < 1e-6f)
				targetAspectRatio = range.height * displaySize.x / range.width / displaySize.y;
			range.height = range.width * displaySize.y / displaySize.x * targetAspectRatio;
		}

		UpdateBackground();
        UpdateAxes();

		if (plotPoints != null)
			UpdatePlot(plotPoints);
		
		UpdateActive();
    }
}
