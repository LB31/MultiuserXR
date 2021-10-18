using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


// Helper methods used to set drawing settings
public class DrawingSettings : MonoBehaviour
{
    public static bool isCursorOverUI = false;
    public float Transparency = 1f;

    // Changing pen settings is easy as changing the static properties Drawable.Pen_Colour and Drawable.Pen_Width
    public void SetMarkerColour(Color new_color)
    {
        DrawManager.PenColor = new_color;
    }
    // new_width is radius in pixels
    public void SetMarkerWidth(int new_width)
    {
        DrawManager.Pen_Width = new_width;
    }
    public void SetMarkerWidth(float new_width)
    {
        SetMarkerWidth((int)new_width);
    }

    public void SetTransparency(float amount)
    {
        Transparency = amount;
        Color c = DrawManager.PenColor;
        c.a = amount;
        DrawManager.PenColor = c;
    }


    // Call these these to change the pen settings
    public void SetMarkerRed()
    {
        Color c = Color.red;
        c.a = Transparency;
        SetMarkerColour(c);
        DrawManager.drawable.SetPenBrush();
    }
    public void SetMarkerGreen()
    {
        Color c = Color.green;
        c.a = Transparency;
        SetMarkerColour(c);
        DrawManager.drawable.SetPenBrush();
    }
    public void SetMarkerBlue()
    {
        Color c = Color.blue;
        c.a = Transparency;
        SetMarkerColour(c);
        DrawManager.drawable.SetPenBrush();
    }
    public void SetEraser()
    {
        SetMarkerColour(new Color(255f, 255f, 255f, 0f));
    }

    public void PartialSetEraser()
    {
        SetMarkerColour(new Color(255f, 255f, 255f, 0.5f));
    }
}
