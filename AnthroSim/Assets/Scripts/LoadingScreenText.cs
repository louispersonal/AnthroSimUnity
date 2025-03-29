using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreenText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _loadingText;

    private int _cycle;

    private float _interval = 1f;

    // Start is called before the first frame update
    void Start()
    {
        _cycle = 0;
        StartCoroutine(LoadingText());
    }

    private IEnumerator LoadingText()
    {
        while (true)
        {
            yield return new WaitForSeconds(_interval);
            string baseString = "Loading";
            for (int d = 0; d < _cycle; d++)
            {
                baseString += ".";
            }
            _loadingText.text = baseString;
            _cycle = (_cycle + 1) % 4;
        }
    }
}
