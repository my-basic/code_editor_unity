using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyCodeInput : MonoBehaviour
{
    public MyCodeLine Editing { get; private set; }

    public InputField FieldInput
    {
        get
        {
            InputField i = GetComponent<InputField>();

            return i;
        }
    }

    public void Show(MyCodeLine ln)
    {
        if (Editing != null)
            Editing.gameObject.SetActive(true);

        gameObject.SetActive(true);

        FieldInput.Rect().SetSize(ln.Rect().GetSize());
        FieldInput.Rect().transform.localPosition = (ln.Rect().transform.localPosition);
        FieldInput.text = ln.Text;

        ln.gameObject.SetActive(false);

        Editing = ln;
    }

    public void Hide()
    {
        if (Editing != null)
            Editing.gameObject.SetActive(true);

        gameObject.SetActive(false);

        Editing = null;
    }
}
