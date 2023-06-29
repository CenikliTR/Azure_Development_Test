using UnityEngine;

public class FPS : MonoBehaviour
{
    public float updateInterval = 0.5f;

    private float accum;
    private int frames;
    private float timeLeft;
    private float fps;

    void Start()
    {
        timeLeft = updateInterval;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeLeft <= 0f)
        {
            fps = accum / frames;
            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;
        style.fontSize = 40;

        GUI.Label(new Rect(30, 30, 100, 40), "FPS: " + fps.ToString("F2"), style);
    }
}