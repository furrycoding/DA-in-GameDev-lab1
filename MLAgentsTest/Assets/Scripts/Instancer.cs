using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instancer : MonoBehaviour
{
    public Vector3 offset;

    public Vector3Int counts;

    public GameObject target;

    protected bool isInstantiated = false;

    void Start()
    {
        if (isInstantiated)
            return;

        var startPos = target.transform.position;
        var rotation = target.transform.rotation;
        var parent = target.transform.parent;

        for (var x = 0; x <= counts.x; x++)
            for (var y = 0; y <= counts.y; y++)
                for (var z = 0; z <= counts.z; z++)
                {
                    var pos = new Vector3(x, y, z);
                    if (pos == Vector3.zero)
                        continue;

                    pos.Scale(offset);
                    pos += startPos;

                    var newObj = Instantiate(target, pos, rotation, parent);
                    
                    foreach (var instancer in newObj.GetComponentsInChildren<Instancer>())
                        instancer.isInstantiated = true;
                }
    }

    void Update()
    {
        if (isInstantiated)
            Destroy(this);
    }
}
