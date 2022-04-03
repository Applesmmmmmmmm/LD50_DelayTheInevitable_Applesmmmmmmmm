using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private GameObject _playButtonPanel;
    [SerializeField] private GameObject _replayButtonPanel;

    [SerializeField] private GameObject _menuMusicSource;
    [SerializeField] private GameObject _menuSFXSource;
    [SerializeField] private GameObject _gameMusicSource;
    [SerializeField] private GameObject _gameSFXSource;

    private bool _gameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        _playButtonPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayButtonClicked()
    {
        _playButtonPanel.SetActive(false);
        StartTutorial();
    }

    public void StartTutorial()
    {

    }

    public void GameOver()
    {
        //Immediately popup retry button
        //Freeze the camera, and move the ufo across the screen until everything has been destroyed and it goes off screen.
    }

    public IEnumerator SpawnObjects()
    {
        while (!_gameOver)
        {
            //Spawn (cars, road pieces, buildings) from the object pool and slide them towards the left, where the ufo is.

            //Check if enough time has elapsed that we should start spawning destroyed objects instead of new things, once this happens the player will run out of gas inevitably.

            yield return null;
        }
    }

    public void DisableObjectsOffscreen()
    {
        //Disable and reset objects that have gone offscreen so they can be spawned again later.

    }

    public void MoveObjectsLeft()
    {
        //Check if car is being jacked, or driven by player, otherwise slide left.

        //Slide environment left

    }

    public void MoveUFORight()
    {
        //Move the UFO, and it's child ray to the right during the gameover phase.
    }

    public void MovePeopleUpRay()
    {
        //If a person is alive and hits the UFO ray, move them up into the ship and then recycle them
    }

}
