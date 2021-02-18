using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(ClickableTMP_InputField))]
public class NumericTextInput : MonoBehaviour
{
    string Text
    {
        get { return InputField.text; }
        set { InputField.text = value; }
    }

    public TMPro.TMP_InputField InputField
    { get { return GetComponent<ClickableTMP_InputField>().InputField; } }

    public float Value
    {
        get
        {
            string scrubbed = new string(Text.ToLower()
                .Where(c => char.IsDigit(c) || 
                            c == '.' || 
                            c == 'e').ToArray());

            float value;
            if (!float.TryParse(scrubbed, out value))
            {
                Value = 0;
                return 0;
            }

            return value;
        }

        set { Text = value.ToString(Format); }
    }

    public string Format = "F2";

    public bool IsSelected
    { get { return InputField.IsSelected(); } }

    private void Start()
    {
        InputField.onSubmit.AddListener(delegate 
        {
            Value = Value;
        });
    }

    private void Update()
    {

    }
}
