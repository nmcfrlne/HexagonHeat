using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamerTag : MonoBehaviour
{
    // Start is called before the first frame update
    Transform t;
    public HexagonCamera cam;
    public Text gamerTag;

    void Start()
    {
        t = transform;
    }

    void Update()
    {
        if(gamerTag.text.Length != 0)
        {
            t.eulerAngles = new Vector3(cam.tiltAngle, cam.lookAngle, t.eulerAngles.z);
        }
    }
}
