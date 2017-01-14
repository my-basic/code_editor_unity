using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyCodeHead : MonoBehaviour
{
    private const float TEXT_OCCUPATION = 0.8f;

    public const float X_OFFSET = 2.0f;
    public const float Y_OFFSET = 2.0f;

    public Toggle toggleSelection = null;

    public Button buttonLineNumber = null;
    public Text textLineNumber = null;

    private int lineNumber = 0;
    public int LineNumber
    {
        get
        {
            return lineNumber;
        }
        set
        {
            lineNumber = value;

            this.Rect().SetLeftTopPosition(new Vector2(X_OFFSET, -Y_OFFSET - Height * lineNumber));

            textLineNumber.text = (lineNumber + 1).ToString();
        }
    }

    private float height = 0.0f;
    public float Height
    {
        get
        {
            return height;
        }
        set
        {
            height = value;

            int fontSize = (int)(height * TEXT_OCCUPATION);
            textLineNumber.fontSize = fontSize;

            this.Rect().SetHeight(height);
            buttonLineNumber.Rect().SetHeight(height);
            toggleSelection.Rect().SetSize(new Vector2(height, height));
        }
    }

    public float Width
    {
        get
        {
            return this.Rect().GetSize().x;
        }
        set
        {
            this.Rect().SetSize(new Vector2(value, this.Rect().GetSize().y));
        }
    }

    public float TightWidth
    {
        get
        {
            float result = LineNumberTextWidth;
            result += Mathf.Abs(textLineNumber.Rect().offsetMin.x);
            result += Mathf.Abs(textLineNumber.Rect().offsetMax.x);
            result += toggleSelection.Rect().GetSize().x;

            return result;
        }
    }

    public int LineNumberTextWidth
    {
        get
        {
            int result = 0;
            Font font = textLineNumber.font;
            CharacterInfo characterInfo = new CharacterInfo();
            foreach (char c in textLineNumber.text)
            {
                font.GetCharacterInfo(c, out characterInfo, textLineNumber.fontSize);
                result += characterInfo.advance;
            }

            return result;
        }
    }

    public void Relayout()
    {
        this.Rect().SetLeftTopPosition(new Vector2(X_OFFSET, -Y_OFFSET - Height * lineNumber));

        toggleSelection.Rect().SetLeftPosition(-this.Rect().GetSize().x / 2.0f);
        buttonLineNumber.Rect().SetSize(new Vector2(this.Rect().GetSize().x - toggleSelection.Rect().GetSize().x, buttonLineNumber.Rect().GetSize().y));
        buttonLineNumber.Rect().SetLeftPosition(-this.Rect().GetSize().x / 2.0f + toggleSelection.Rect().GetSize().x);
    }
}
