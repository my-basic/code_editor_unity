using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyCodeLine : MonoBehaviour
{
    private const float TEXT_OCCUPATION = 0.8f;
    private const float Y_OFFSET = 2.0f;

    public Button buttonCode = null;
    public Text textCode = null;

    public InputField fieldInput = null;
    public Text textInput = null;
    public Text textInputPlaceholder = null;

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

            this.Rect().SetLeftTopPosition(new Vector2(0.0f, -Y_OFFSET - Height * lineNumber));
        }
    }

    public string Text { get; private set; }

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
            textCode.fontSize = fontSize;
            textInput.fontSize = fontSize;
            textInputPlaceholder.fontSize = fontSize;

            this.Rect().SetHeight(height);
            buttonCode.Rect().SetHeight(height);
            fieldInput.Rect().SetHeight(height);
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
            float result = TextWidth;
            result += Mathf.Abs(textCode.Rect().offsetMin.x);
            result += Mathf.Abs(textCode.Rect().offsetMax.x);

            return result;
        }
    }

    public int TextWidth
    {
        get
        {
            int result = 0;
            Font font = textCode.font;
            CharacterInfo characterInfo = new CharacterInfo();
            foreach (char c in Text)
            {
                font.GetCharacterInfo(c, out characterInfo, textCode.fontSize);
                result += characterInfo.advance;
            }

            return result;
        }
    }

    private void Start()
    {
        buttonCode.gameObject.SetActive(true);
        fieldInput.gameObject.SetActive(false);
    }

    private void Awake()
    {
        buttonCode.onClick.AddListener(OnButtonCodeClicked);
    }

    private void OnDestroy()
    {
        buttonCode.onClick.RemoveListener(OnButtonCodeClicked);
    }

    public MyCodeLine SetText(string text, string rich)
    {
        Text = text;

        textCode.text = rich;
        textInput.text = rich;
        textInputPlaceholder.text = text;

        return this;
    }

    public void Relayout()
    {
        this.Rect().SetLeftTopPosition(new Vector2(0.0f, -Y_OFFSET - Height * LineNumber));
    }

    private void OnButtonCodeClicked()
    {
    }
}
