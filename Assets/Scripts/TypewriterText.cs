using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterTextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textBox;

    [TextArea(3, 8)]
    [SerializeField] private string fullText;

    [SerializeField] private float typingSpeed = 0.025f;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (textBox == null)
            textBox = GetComponent<TMP_Text>();

        textBox.text = fullText;
        textBox.maxVisibleCharacters = 0;
    }

    public void PlayText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        textBox.text = fullText;

        // Forces TMP to calculate the whole layout first.
        textBox.ForceMeshUpdate();

        int totalCharacters =
            textBox.textInfo.characterCount;

        textBox.maxVisibleCharacters = 0;

        for (int i = 0; i <= totalCharacters; i++)
        {
            textBox.maxVisibleCharacters = i;

            yield return new WaitForSecondsRealtime(
                typingSpeed
            );
        }

        typingCoroutine = null;
    }

    public void ClearText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textBox.maxVisibleCharacters = 0;
    }
}