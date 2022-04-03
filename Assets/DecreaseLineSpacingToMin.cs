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
    [SerializeField] private AudioSource _gameSFXAudioSource;
    [SerializeField] private AudioClip _gameTitleAudioClip;
    // Start is called before the first frame update

    private bool _soundPlayed = false;

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
            if (!_soundPlayed)
            {
                _gameSFXAudioSource.PlayOneShot(_gameTitleAudioClip);
                _soundPlayed = true;
            }
            

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
