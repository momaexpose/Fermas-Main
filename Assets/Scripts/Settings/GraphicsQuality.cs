using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    public void SetUltra()
    {
        QualitySettings.SetQualityLevel(0);
        RenderSettings.fog = true;
    }

    public void SetHigh()
    {
        QualitySettings.SetQualityLevel(1);
        RenderSettings.fog = true;
    }

    public void SetMedium()
    {
        QualitySettings.SetQualityLevel(2);
        RenderSettings.fog = true;
    }

    public void SetLow()
    {
        QualitySettings.SetQualityLevel(3);
        RenderSettings.fog = false;
    }
}
