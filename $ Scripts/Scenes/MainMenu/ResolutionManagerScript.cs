using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Workaround for allowing resolutions to be added and removed in the unity editor
[System.Serializable]
public class Resolution
{
    public int x;
    public int y;
}

public class ResolutionManagerScript : MonoBehaviour
{
    public Text text;

    [SerializeField]
    public List<Resolution> Resolutions;

    public int ResolutionIndex = -1;

    public void Start()
    {
        ResolutionIndex = Resolutions.Count-1;
        //ResolutionClick();
    }

    // On resolution button press
    public void ResolutionClick()
    {
        ResolutionIndex++;
        if (ResolutionIndex == Resolutions.Count) { ResolutionIndex = -1; }

        int x;
        int y;

        if (ResolutionIndex == -1) { x = Display.main.systemWidth; y = Display.main.systemHeight; text.text = "(Native) " + x.ToString() + " x " + y.ToString(); }
        else { x = Resolutions[ResolutionIndex].x; y = Resolutions[ResolutionIndex].y; text.text = x.ToString() + " x " + y.ToString(); }

        Screen.SetResolution(x, y, true);
    }
}
