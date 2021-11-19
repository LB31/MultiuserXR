using MLAPI;
using UnityEngine;


public class DrawManager : MonoBehaviour
{
    public static Color PenColor = Color.red;
    // PEN WIDTH (actually, it's a radius, in pixels)
    public static int Pen_Width = 3;

    public delegate void Brush_Function(Vector2 world_position);

    public Brush_Function currentBrush;

    public LayerMask Drawing_Layers;

    public bool ResetCanvasOnPlay = true;
    public Color ResetColor = new Color(0, 0, 0, 0);  // By default, reset the canvas to be transparent

    // Used to reference THIS specific file without making all methods static
    public static DrawManager drawable;
    // MUST HAVE READ/WRITE enabled set in the file editor of Unity
    public Texture2D DrawableTexture;

    public Vector2 PreviousDragPosition;

    private Color[] clean_colors_array;
    private Color32[] cur_colors;

    [HideInInspector]
    public float Width, Height;

    private NetworkDrawSharer drawSharer;

    private void Awake()
    {
        Width = DrawableTexture.width;
        Height = DrawableTexture.height;

        drawable = this;

        drawSharer = GetComponent<NetworkDrawSharer>();

        // Initialize clean pixels to use
        clean_colors_array = new Color[DrawableTexture.width * DrawableTexture.height];
        for (int x = 0; x < clean_colors_array.Length; x++)
            clean_colors_array[x] = ResetColor;

        // Should we reset our canvas image when we hit play in the editor?
        if (ResetCanvasOnPlay && NetworkManager.Singleton.IsServer)
            ResetCanvas();


    }


    public void Draw(Vector2 pixelUV)
    {
        Vector2 pixel_pos = pixelUV;

        cur_colors = DrawableTexture.GetPixels32();

        // Dirst time we've ever dragged on this image, simply colour the pixels at our mouse position
        if (PreviousDragPosition == Vector2.zero)
        {
            MarkPixelsToColour(pixel_pos, Pen_Width, PenColor);
        }
        else
        {
            // Colour in a line from where we were on the last update call
            ColorBetween(PreviousDragPosition, pixel_pos, Pen_Width, PenColor);
        }

        ApplyMarkedPixelChanges();

        PreviousDragPosition = pixel_pos;
    }


    // Set the colour of pixels in a straight line from start_point all the way to end_point, to ensure everything inbetween is coloured
    public void ColorBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
    {
        // Get the distance from start to finish
        float distance = Vector2.Distance(start_point, end_point);
        Vector2 direction = (start_point - end_point).normalized;
        Vector2 cur_position = start_point;

        // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixelsToColour(cur_position, width, color);
        }
    }

    public void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        // Figure out how many pixels we need to colour in each direction (x and y)
        int center_x = (int)center_pixel.x;
        int center_y = (int)center_pixel.y;
        //int extra_radius = Mathf.Min(0, pen_thickness - 2);

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
            if (x >= DrawableTexture.width || x < 0)
                continue;

            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                MarkPixelToChange(x, y, color_of_pen);
                //Debug.Log($"x{x} y{y}");
            }
        }
    }

    public void MarkPixelToChange(int x, int y, Color color)
    {
        // Need to transform x and y coordinates to flat coordinates of array
        int array_pos = y * (int)DrawableTexture.width + x;

        // Check if this is a valid position
        if (array_pos > cur_colors.Length || array_pos < 0)
            return;

        cur_colors[array_pos] = color;
    }
    public void ApplyMarkedPixelChanges()
    {
        DrawableTexture.SetPixels32(cur_colors);
        DrawableTexture.Apply();

        if (drawSharer)
            drawSharer.ShareUpdate(NetworkManager.Singleton.ServerClientId, NetworkManager.Singleton.LocalClientId);
    }


    // Directly colours pixels. This method is slower than using MarkPixelsToColour then using ApplyMarkedPixelChanges
    // SetPixels32 is far faster than SetPixel
    // Colours both the center pixel, and a number of pixels around the center pixel based on pen_thickness (pen radius)
    public void ColourPixels(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        // Figure out how many pixels we need to colour in each direction (x and y)
        int center_x = (int)center_pixel.x;
        int center_y = (int)center_pixel.y;
        //int extra_radius = Mathf.Min(0, pen_thickness - 2);

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                DrawableTexture.SetPixel(x, y, color_of_pen);
            }
        }

        DrawableTexture.Apply();
    }


    public Vector2 WorldToPixelCoordinates(Vector2 world_position)
    {
        // Change coordinates to local coordinates of this image
        Vector3 local_pos = transform.InverseTransformPoint(world_position);

        // Change these to coordinates of pixels
        float pixelWidth = DrawableTexture.width;
        float pixelHeight = DrawableTexture.height;
        float unitsToPixels = pixelWidth / pixelWidth * transform.localScale.x;

        // Need to center our coordinates
        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

        // Round current mouse position to nearest pixel
        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

        return pixel_pos;
    }


    // Changes every pixel to be the reset colour
    public void ResetCanvas()
    {
        DrawableTexture.SetPixels(clean_colors_array);
        DrawableTexture.Apply();

        if (drawSharer)
            drawSharer.ShareUpdate(NetworkManager.Singleton.ServerClientId, NetworkManager.Singleton.LocalClientId);
    }


}