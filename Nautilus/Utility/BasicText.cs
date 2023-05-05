using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nautilus.Utility;

/* [this message was moved out of the XML docs because it was absurdly long and out of place]
 * The idea of the defaults is that new modders don't have to bootstrap a bunch of irritating Unity stuff -- don't have to understand
 * what a "Material" is or how to make one, don't have to know to initialize a font, or even a color. Can just start adding text and
 * then can always custom and configure on further revision.
*/

/// <summary>
/// <para>
/// Places a simple text object on the screen and keeps it there until either hidden (or a designated fade-to-black timer has passed). 
/// By default uses the same font/size/color as the "Press Any Button To Begin" message at the beginning of the game, and appears 
/// centered about 1/3 down the screen, but all parameters can be reconfigured.
/// </para>
/// </summary>
/// <example>
/// SIMPLE USAGE EXAMPLE:
/// <code>
/// BasicText message = new BasicText();
/// message.ShowMessage("This Message Will Fade In 10 Seconds", 10);
/// </code>
/// 
/// COMPLEX USAGE EXAMPLE:
/// <code>
/// BasicText message = new BasicText(TextAnchor.UpperLeft); // Note many other properties could also be set as constructor parameters
/// message.setColor(Color.red); // Set Color
/// message.setSize(20);         // Set Font Size
/// message.setLoc(200, 400);    // Set x/y position (0,0 is center of screen)
/// message.setFontStyle(FontStyle.Bold); // Bold 
/// message.ShowMessage("This message stays on screen until hidden"); // Display message; if fadeout seconds not specified, it just keeps showing
/// ... // other things happen, time goes by
/// message.Hide(); // Hides the message
/// </code>
/// </example>
public class BasicText
{
    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> with all default options.
    /// </summary>
    public BasicText()
    {
        CloneAlign = true;
        CloneColor = true;
        CloneSize = true;
        CloneFont = true;
        CloneStyle = true;
        CloneMaterial = true;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> at the specified origin point.
    /// </summary>
    /// <param name="set_x">The x coordinate to set</param>
    /// <param name="set_y">The y coordinate to set</param>
    public BasicText(int set_x, int set_y) : this()
    {
        X = set_x;
        Y = set_y;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> at a specified <see cref="TextAnchor"/>.
    /// </summary>
    /// <param name="useAlign">The text anchor to align to</param>
    public BasicText(TextAlignmentOptions useAlign) : this()
    {
        CloneAlign = false;
        Align = useAlign;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> with a specified <see cref="UnityEngine.Color"/>.
    /// </summary>
    /// <param name="useColor">The text color to use</param>
    public BasicText(Color useColor) : this()
    {
        CloneColor = false;
        Color = useColor;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> with a specified size.
    /// </summary>
    /// <param name="useSize">The text size to uset</param>
    public BasicText(int useSize) : this()
    {
        CloneSize = false;
        Size = useSize;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> with a specified size and <see cref="UnityEngine.Color"/>.
    /// </summary>
    /// <param name="useSize">The text size to use</param>
    /// <param name="useColor">The text color to use</param>
    public BasicText(int useSize, Color useColor) : this()
    {
        CloneColor = false;
        Color = useColor;
        CloneSize = false;
        Size = useSize;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> with a specified size and <see cref="TextAnchor"/>.
    /// </summary>
    /// <param name="useSize">The text size to use</param>
    /// <param name="useAlign">The text anchor to align to</param>
    public BasicText(int useSize, TextAlignmentOptions useAlign) : this()
    {
        CloneAlign = false;
        Align = useAlign;
        CloneSize = false;
        Size = useSize;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> with a specified size, <see cref="UnityEngine.Color"/>, and <see cref="TextAnchor"/>.
    /// </summary>
    /// <param name="useSize">The text size to use</param>
    /// <param name="useColor">The text color to use</param>
    /// <param name="useAlign">The text anchor to align to</param>
    public BasicText(int useSize, Color useColor, TextAlignmentOptions useAlign) : this()
    {
        CloneAlign = false;
        Align = useAlign;
        CloneColor = false;
        Color = useColor;
        CloneSize = false;
        Size = useSize;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> with a specified origin point, size, <see cref="UnityEngine.Color"/>, and <see cref="TextAnchor"/>.
    /// </summary>
    /// <param name="set_x">The x coordinate to set</param>
    /// <param name="set_y">The y coordinate to set</param>
    /// <param name="useSize">The text size to use</param>
    /// <param name="useColor">The text color to use</param>
    /// <param name="useAlign">The text anchor to align to</param>
    public BasicText(int set_x, int set_y, int useSize, Color useColor, TextAlignmentOptions useAlign) : this()
    {
        X = set_x;
        Y = set_y;
        CloneAlign = false;
        Align = useAlign;
        CloneColor = false;
        Color = useColor;
        CloneSize = false;
        Size = useSize;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> with a specified origin point, size, and <see cref="UnityEngine.Color"/>.
    /// </summary>
    /// <param name="set_x">The x coordinate to set</param>
    /// <param name="set_y">The y coordinate to set</param>
    /// <param name="useSize">The text size to use</param>
    /// <param name="useColor">The text color to use</param>
    public BasicText(int set_x, int set_y, int useSize, Color useColor) : this()
    {
        X = set_x;
        Y = set_y;
        CloneColor = false;
        Color = useColor;
        CloneSize = false;
        Size = useSize;
    }

    /// <summary>
    /// Creates a new instances of <see cref="BasicText"/> with a specified origin point and size.
    /// </summary>
    /// <param name="set_x">The x coordinate to set</param>
    /// <param name="set_y">The y coordinate to set</param>
    /// <param name="useSize">The text size to use</param>
    public BasicText(int set_x, int set_y, int useSize) : this()
    {
        X = set_x;
        Y = set_y;
        CloneSize = false;
        Size = useSize;
    }

    /// <summary>
    /// Shows our text item, with no schedule fade (i.e. indefinitely)
    /// </summary>
    /// <param name="s">The text to display</param>
    public void ShowMessage(string s)
    {
        ShowMessage(s, 0);
    }

    /// <summary>
    /// Shows our text item, fading after a specified number of seconds (or stays on indefinitely if 0 seconds)
    /// </summary>
    /// <param name="s">The text to display</param>
    /// <param name="seconds">The duration to hold before fading</param>
    public void ShowMessage(string s, float seconds)
    {
        if (TextObject == null)
        {
            // First time only, initialize the object and components
            InitializeText();
        }

        // Set our actual text
        TextFade.SetText(s);

        // Sets our location on the screen
        DoAlignment();

        // Turns our text item on
        TextFade.SetState(true);
        TextObject.SetActive(true);

        // If specified, sets the fade-out timer
        if (seconds > 0)
        {
            TextFade.FadeOut(seconds, null);
        }
    }

    /// <summary>
    /// Hides our text item if it is displaying.
    /// </summary>
    public void Hide()
    {
        if (TextObject == null)
        {
            return;
        }

        TextFade.SetState(false);
        TextObject.SetActive(false);
    }

    /// <summary>
    /// Returns our current text.
    /// </summary>
    /// <returns></returns>
    public string GetText()
    {
        return TextText?.text ?? string.Empty;
    }

    /// <summary>
    /// Sets screen display location (position relative to the actual text is determined by the alignment)
    /// </summary>
    /// <param name="set_x">The x coordinate to set</param>
    /// <param name="set_y">The y coordinate to set</param>
    public void SetLocation(float set_x, float set_y)
    {
        X = set_x;
        Y = set_y;
        DoAlignment();
    }

    /// <summary>
    /// Sets the text color
    /// </summary>
    /// <param name="useColor">The text color to use</param>
    public void SetColor(Color useColor)
    {
        CloneAlign = false;
        Color = useColor;

        if (TextObject != null)
        {
            TextText.color = Color;
        }
    }

    /// <summary>
    /// Resets to using "cloned" color of Subnautica default.
    /// </summary>
    public void ClearColor()
    {
        CloneColor = true;
        if (TextObject != null)
        {
            TextText.color = uGUI.main.intro.mainText.text.color;
        }
    }

    /// <summary>
    /// Sets the font size.
    /// </summary>
    /// <param name="useSize">The text size to use</param>
    public void SetSize(int useSize)
    {
        CloneSize = false;
        Size = useSize;
        if (TextObject != null)
        {
            TextText.fontSize = Size;
            DoAlignment();
        }
    }

    /// <summary>
    /// Resets to using "cloned" size of Subnautica default.
    /// </summary>
    public void ClearSize()
    {
        CloneSize = true;
        if (TextObject != null)
        {
            TextText.fontSize = uGUI.main.intro.mainText.text.fontSize;
            DoAlignment();
        }
    }

    /// <summary>
    /// Sets the font 
    /// </summary>
    /// <param name="useFont">The font to render the text as.</param>
    public void SetFont(TMP_FontAsset useFont)
    {
        CloneFont = false;
        Font = useFont;
        if (TextObject != null)
        {
            TextText.font = Font;
            DoAlignment();
        }
    }

    /// <summary>
    /// Resets to using "cloned" font of Subnautica default.
    /// </summary>
    public void ClearFont()
    {
        CloneFont = true;
        if (TextObject != null)
        {
            TextText.font = uGUI.main.intro.mainText.text.font;
            DoAlignment();
        }
    }

    /// <summary>
    /// Sets the font style.
    /// </summary>
    /// <param name="useStyle">The text font style to use</param>
    public void SetFontStyle(FontStyles useStyle)
    {
        CloneStyle = false;
        Style = useStyle;
        if (TextObject != null)
        {
            TextText.fontStyle = Style;
            DoAlignment();
        }
    }

    /// <summary>
    /// Resets to using "cloned" font style of Subnautica default.
    /// </summary>
    public void ClearFontStyle()
    {
        CloneStyle = true;
        if (TextObject != null)
        {
            TextText.fontStyle = uGUI.main.intro.mainText.text.fontStyle;
        }
    }

    /// <summary>
    /// Sets the text anchor.
    /// </summary>
    /// <param name="useAlign">The text anchor to align to</param>
    public void SetAlign(TextAlignmentOptions useAlign)
    {
        CloneAlign = false;
        Align = useAlign;
        if (TextObject != null)
        {
            TextText.alignment = Align;
            DoAlignment();
        }
    }

    /// <summary>
    /// Resets to using "cloned" font style of Subnautica default
    /// </summary>
    public void ClearAlign()
    {
        CloneAlign = true;
        if (TextObject != null)
        {
            TextText.alignment = uGUI.main.intro.mainText.text.alignment;
            DoAlignment();
        }
    }

    /// <summary>
    /// Computes proper transform position based on alignment and size of text.
    /// </summary>
    private void DoAlignment()
    {
        if (TextObject == null)
        {
            return;
        }

        float width = TextText.preferredWidth;
        float height = TextText.preferredHeight;

        float displayX, displayY;

        switch (Align)
        {
            case TextAlignmentOptions.TopLeft:
            case TextAlignmentOptions.Left:
            case TextAlignmentOptions.BottomLeft:

                displayX = X + width / 2;
                break;
            case TextAlignmentOptions.TopRight:
            case TextAlignmentOptions.Right:
            case TextAlignmentOptions.BottomRight:
                displayX = X - width / 2;
                break;

            default:
                displayX = X;
                break;
        }

        switch (Align)
        {
            case TextAlignmentOptions.TopLeft:
            case TextAlignmentOptions.Top:
            case TextAlignmentOptions.TopRight:
                displayY = Y - height / 2;
                break;

            case TextAlignmentOptions.BottomLeft:
            case TextAlignmentOptions.Bottom:
            case TextAlignmentOptions.BottomRight:
                displayY = Y + height / 2;
                break;

            default:
                displayY = Y;
                break;
        }

        TextObject.transform.localPosition = new Vector3(displayX, displayY, 0f);
    }

    /// <summary>
    /// Sets up all of our objects/components, when we are ready to actually display text for the first time.
    /// </summary>
    private void InitializeText()
    {
        // Make our own text object
        TextObject = new GameObject("BasicText" + (++index));
        TextFade = TextObject.EnsureComponent<uGUI_TextFade>(); // The uGUI's helpful automatic fade component           
        TextText = TextFade?.text ?? TextObject.EnsureComponent<TextMeshProUGUI>(); // The text itself

        // This makes the text box fit the text (rather than the other way around)
        TextFitter = TextObject.EnsureComponent<ContentSizeFitter>();
        TextFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        TextFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // This clones the in game "Press Any Button To Begin" message's font size, style, etc.
        TextText.font = CloneFont ? uGUI.main.intro.mainText.text.font : Font;
        TextText.fontSize = CloneSize ? uGUI.main.intro.mainText.text.fontSize : Size;
        TextText.fontStyle = CloneStyle ? uGUI.main.intro.mainText.text.fontStyle : Style;
        TextText.alignment = CloneAlign ? uGUI.main.intro.mainText.text.alignment : Align;
        TextText.color = CloneColor ? uGUI.main.intro.mainText.text.color : Color;
        TextText.material = CloneMaterial ? uGUI.main.intro.mainText.text.material : Material;

        // Sets it to SN's highest layer -- note it will not appear on top of the black "you are dead" overlay. To do that
        // requires setting it HIGHER than uGUI.main.overlays.overlays[0].graphic  -- which will make SN throw a sloppy exception
        // in the log, but will WORK.
        TextObject.transform.SetParent(uGUI.main.screenCanvas.transform, false); // Parents our text to the black overlay
        TextText.canvas.overrideSorting = true;              // Turn on canvas sort override so the layers will work                    
        TextObject.layer = 31;                               // Set to what seems to be Subnautica's highest layer
    }

    internal float X { get; set; } = 0;          // X position anchor
    internal float Y { get; set; } = 210f;       // Y position anchor (defaults to a comfortable centered about 1/3 from top of screen)
    internal bool CloneAlign { get; set; }       // True if we're cloning Subnautica's "Press Any Button To Begin" alignment
    internal bool CloneColor { get; set; }       // True if we're cloning Subnautica's "Press Any Button To Begin" color
    internal bool CloneSize { get; set; }        // True if we're cloning Subnautica's "Press Any Button To Begin" fontsize
    internal bool CloneFont { get; set; }        // True if we're cloning Subnautica's "Press Any Button To Begin" font
    internal bool CloneStyle { get; set; }       // True if we're cloning Subnautica's "Press Any Button To Begin" font style
    internal bool CloneMaterial { get; set; }    // True if we're cloning Subnautica's "Press Any Button To Begin" material
    internal TextAlignmentOptions Align { get; set; }      // text alignment
    internal Color Color { get; set; }           // text color
    internal int Size { get; set; }              // text size
    internal TMP_FontAsset Font { get; set; }             // text font
    internal FontStyles Style { get; set; }       // text font style
    internal Material Material { get; set; }     // text material
    internal GameObject TextObject { get; set; } = null;          // Our game object
    internal uGUI_TextFade TextFade { get; set; } = null;         // Our text fader
    internal TextMeshProUGUI TextText { get; set; } = null;                  // Our text object
    internal ContentSizeFitter TextFitter { get; set; } = null;   // Our content size fitter

    internal static int index = 0; // For giving unique names to the game objects

}