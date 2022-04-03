using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DecreaseLineSpacingToMin : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private float _minLineSpacing = -12.5f;
    [SerializeField] private float _decreaseSpeed = 1f;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if(_title.lineSpacing > _minLineSpacing)
        {
            _title.lineSpacing -= _decreaseSpeed * Time.deltaTime;
            _decreaseSpeed *= 1.02f;
            if(_title.lineSpacing < _minLineSpacing)
            {
                _title.lineSpacing = _minLineSpacing;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (Input.anyKeyDown)
            {
                SceneManager.LoadScene("MainGameLoop");
            }
        }
    }
}
