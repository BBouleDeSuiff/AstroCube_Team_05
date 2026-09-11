using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectLoader : MonoBehaviour 
{

    bool isActivatedOnce = true;
    public List<GameObject> toActivate = new List<GameObject>();
    public List<GameObject> toDeActivate = new List<GameObject>();
    public void SwitchActivate()
    {
        toActivate.ForEach(t => t.SetActive(isActivatedOnce));
        toDeActivate.ForEach(t => t.SetActive(!isActivatedOnce));
        isActivatedOnce = !isActivatedOnce;
    }

}
