using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.EventSystems;


public class Drawable : MonoBehaviour
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
    public static Drawable drawable;
    // MUST HAVE READ/WRITE enabled set in the file editor of Unity
    public Texture2D DrawableTexture;

    private Vector2 previous_drag_position;
    private Color[] clean_colours_array;
    private Color transparent;
    private Color32[] cur_colors;
    private bool mouse_was_previously_held_down = false;
    private bool no_drawing_on_current_drag = false;
    private float width, height;
    private bool mouse_held_down;

    private NetworkDrawSharer drawSharer;

    private void Awake()
    {
        width = DrawableTexture.width;
        height = DrawableTexture.height;

        drawable = this;

        drawSharer = GetComponent<NetworkDrawSharer>();

        // DEFAULT BRUSH SET HERE
        currentBrush = PenBrush;

        // Initialize clean pixels to use
        clean_colours_array = new Color[DrawableTexture.width * DrawableTexture.height];
        for (int x = 0; x < clean_colours_array.Length; x++)
            clean_colours_array[x] = ResetColor;

        // Should we reset our canvas image when we hit play in the editor?
        if (ResetCanvasOnPlay)
            ResetCanvas();
    }

    public void BrushTemplate(Vector2 world_position)
    {
        // 1. Change world position to pixel coordinates
        Vector2 pixel_pos = WorldToPixelCoordinates(world_position);

        // 2. Make sure our variable for pixel array is updated in this frame
        cur_colors = DrawableTexture.GetPixels32();

        ////////////////////////////////////////////////////////////////
        // FILL IN CODE BELOW HERE

        // Do we care about the user left clicking and dragging?
        // If you don't, simply set the below if statement to be:
        //if (true)

        // If you do care about dragging, use the below if/else structure
        if (previous_drag_position == Vector2.zero)
        {
            // THIS IS THE FIRST CLICK
            // FILL IN WHATEVER YOU WANT TO DO HERE
            // Maybe mark multiple pixels to colour?
            MarkPixelsToColour(pixel_pos, Pen_Width, PenColor);
        }
        else
        {
            // THE USER IS DRAGGING
            // Should we do stuff between the previous mouse position and the current one?
            ColourBetween(previous_drag_position, pixel_pos, Pen_Width, PenColor);
        }
        ////////////////////////////////////////////////////////////////

        // 3. Actually apply the changes we marked earlier
        // Done here to be more efficient
        ApplyMarkedPixelChanges();

        // 4. If dragging, update where we were previously
        previous_drag_position = pixel_pos;
    }

    // Default brush type. Has width and colour.
    // Pass in a point in WORLD coordinates
    // Changes the surrounding pixels of the world_point to the static pen_colour
    public void PenBrush(Vector2 pixelUV)
    {
        Vector2 pixel_pos = pixelUV;

        cur_colors = DrawableTexture.GetPixels32();

        if (previous_drag_position == Vector2.zero)
        {
            // If this is the first time we've ever dragged on this image, simply colour the pixels at our mouse position
            MarkPixelsToColour(pixel_pos, Pen_Width, PenColor);
        }
        else
        {
            // Colour in a line from where we were on the last update call
            ColourBetween(previous_drag_position, pixel_pos, Pen_Width, PenColor);
        }
        ApplyMarkedPixelChanges();

        previous_drag_position = pixel_pos;
    }


    // Helper method used by UI to set what brush the user wants
    // Create a new one for any new brushes you implement
    public void SetPenBrush()
    {
        // PenBrush is the NAME of the method we want to set as our current brush
        currentBrush = PenBrush;
    }


    void Update()
    {
        // Is the user holding down the left mouse button?
        mouse_held_down = Input.GetMouseButton(0);
        if (mouse_held_down && !no_drawing_on_current_drag)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10; // select distance = 10 units from the camera
                             // Convert mouse coordinates to world coordinates
            Vector2 mouse_world_position = Camera.main.ScreenToWorldPoint(mousePos);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Drawing_Layers.value))
            {
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= width;
                pixelUV.y *= height;
                currentBrush(pixelUV);
            }
            else
            {
                // We're not over our destination texture
                previous_drag_position = Vector2.zero;
                if (!mouse_was_previously_held_down)
                {
                    // This is a new drag where the user is left clicking off the canvas
                    // Ensure no drawing happens until a new drag is started
                    no_drawing_on_current_drag = true;
                }
            }
        }
        // Mouse is released
        else if (!mouse_held_down)
        {
            previous_drag_position = Vector2.zero;
            no_drawing_on_current_drag = false;
        }

        // Client finished drawing
        if (mouse_was_previously_held_down && !mouse_held_down)
        {

        }


        mouse_was_previously_held_down = mouse_held_down;
    }


    // Set the colour of pixels in a straight line from start_point all the way to end_point, to ensure everything inbetween is coloured
    public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
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
        DrawableTexture.SetPixels(clean_colours_array);
        DrawableTexture.Apply();
    }


}