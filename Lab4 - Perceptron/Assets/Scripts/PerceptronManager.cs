using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptronManager : MonoBehaviour
{
    public bool run, reset;
    public float updateTime = 2f;
    
    public Perceptron target;
    
	public GraphVisual plot;
	public GraphVisual visualization;

    private List<Vector2> points = new List<Vector2>();
	private PerceptronVisualizer visualizer;

    private float remUpdateTime = 0;
    private IEnumerator<(int, double)> training;

    void Start()
    {
		visualizer = GetComponent<PerceptronVisualizer>();
		if (visualizer == null) {
			visualizer = gameObject.AddComponent<PerceptronVisualizer>();
			visualizer.OutputSize = Vector2Int.one * 64;
		}
    }


    void Update()
    {
		if (reset) {
			points.Clear();
			training = null;
			remUpdateTime = 0;
			reset = false;
		}
		
        if (!run)
            return;
		
		if (target == null)
			return;
		
        if (remUpdateTime > 0)
        {
            remUpdateTime -= Time.deltaTime;
            return;
        }
        remUpdateTime += updateTime;
        remUpdateTime = Mathf.Max(remUpdateTime, 0);


        if (training == null)
            training = target.Train(10).Select((x, i) => (i, x)).GetEnumerator();

        if (!training.MoveNext())
        {
			training = null;
			remUpdateTime = 0;
            run = false;
            return;
        }

        var step = training.Current.Item1;
        var loss = training.Current.Item2;
        points.Add(new Vector2(step, (float)loss));


        if (plot != null) {			
			var xMin = points.Select(v => v.x).Min();
			var yMin = points.Select(v => v.y).Min();
			var xMax = points.Select(v => v.x).Max();
			var yMax = points.Select(v => v.y).Max();
			plot.range.xMin = xMin - 0.3f;
			plot.range.yMin = yMin - 0.3f;
			plot.range.xMax = xMax + 0.3f;
			plot.range.yMax = yMax + 0.3f;
			plot.plotPoints = points;
		}
		
		if (visualization != null) {
			visualizer.InputRange = visualization.VisibleRange;
			visualizer.Target = target;
			visualizer.UpdateOutput();
			var tex = visualizer.Output;
			
			var s = visualization.background;
			if ((s == null) || (s.texture != tex)) {
				s = visualization.background = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), Vector2.zero);
			}
		}
    }
}
