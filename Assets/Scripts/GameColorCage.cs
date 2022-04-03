using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using UnityEngine.SceneManagement;
using TMPro;

public class GameColorCage : MonoBehaviour
{
    [SerializeField] private GameObject _prefabSun, _prefabOrbitLine, _prefabPlayer;
    private GameObject _sun;
    private GameObject[] _orbitLines;
    private GameObject _player;

    [SerializeField] AudioSource _menuMusicAudioSource, _menuSFXAudioSource, _gameMusicAudioSource, _gameSFXAudioSource;
    [SerializeField] AudioClip _gameBGMAudioClip, _gameBGMLoopAudioClip, _gameTitleAudioClip; 
    [SerializeField] List<AudioClip> _gameScoreAudioClip, _gameRingDestroyAudioClip;

    [SerializeField] private TextMeshProUGUI _scoreTMP;

    //Ring orbit size from center of sun, max representing offscreen, min representing inside smallest sun.
    [SerializeField] private float _maxOrbitPathRadius = 12.5f, _minOrbitPathRadius = .25f;
    //The orbit paths will start at min transparency and lerp to max transparency as they get closer.
    private float _minOrbitTransparency = 33, _maxOrbitTransparency = 255*2;
    [SerializeField] private int _maxPathCount = 7;
    private List<KeyValuePair<float, float>> _radianStartEndPairsPossible = new List<KeyValuePair<float, float>>() {new KeyValuePair<float, float>( 25f * Mathf.Deg2Rad, 360f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>( 55f * Mathf.Deg2Rad, 390f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>( 85f * Mathf.Deg2Rad, 420f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(115f * Mathf.Deg2Rad, 450f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(145f * Mathf.Deg2Rad, 480f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(175f * Mathf.Deg2Rad, 510f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(205f * Mathf.Deg2Rad, 540f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(235f * Mathf.Deg2Rad, 570f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(265f * Mathf.Deg2Rad, 600f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(295f * Mathf.Deg2Rad, 630f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(325f * Mathf.Deg2Rad, 660f * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(355f * Mathf.Deg2Rad, 690f * Mathf.Deg2Rad)};

    private List<KeyValuePair<Color, Color>> _roygbivLerpColors = new List<KeyValuePair<Color, Color>>(){new KeyValuePair<Color, Color>(new Color(209f/255f, 0f, 0f), new Color(255f/255f, 102f/255f, 34f/255f)),
                                                                                                         new KeyValuePair<Color, Color>(new Color(255f/255f, 102f/255f, 34f/255f), new Color(255f/255f, 218f/255f, 33f/255f)),
                                                                                                         new KeyValuePair<Color, Color>(new Color(255f/255f, 218f/255f, 33f/255f), new Color(51f/255f, 221f/255f, 0f)),
                                                                                                         new KeyValuePair<Color, Color>(new Color(51f/255f, 221f/255f, 0f), new Color(17f/255f, 51f/255f, 204f/255f)),
                                                                                                         new KeyValuePair<Color, Color>(new Color(17f/255f, 51f/255f, 204f/255f), new Color(34f/255f, 0f, 102f/255f)),
                                                                                                         new KeyValuePair<Color, Color>(new Color(34f/255f, 0f, 102f/255f), new Color(51f/255f, 0f, 68f/255f)),
                                                                                                         new KeyValuePair<Color, Color>(new Color(51f/255f, 0f, 68f/255f), new Color(209f/255f, 0f, 0f))};
    

    [SerializeField] private float _collapseSpeed = .5f, _speedPercentIncrease = .025f;

    private float _playerSpeed = 360/2.5f, _playerOffsetFromRadius = .3f, _playerRadius = .2f, _playerAngle, _playerDistance, _playerSnapDistance = .25f, _playerJumpSpeed = 5f, _playerOrbitPathLandingPadRadius;
    private bool _playerOnOutside = true, _playerMidJump = false, _orbitFartherThanPlayerPreJump = false, _playerFoundLandingPad = false, _gameOver = false, _scoreDisplayed = false;
    private int _score = 0, _destroyedRingCount = 0;
    private System.Random _random = new System.Random();


    [SerializeField] private Animator _animatorSettings;

    private void Start()
    {
        _gameMusicAudioSource.clip = _gameBGMAudioClip;
        _gameMusicAudioSource.Play();

        StartCoroutine(IncreaseCollapseSpeed());

        _sun = Instantiate(_prefabSun);
        _orbitLines = new GameObject[_maxPathCount];
        float lastDiscRadius = 0;
        float oppositeMiddleOfLastMissingAngle = 0;

        for (int i = 0; i < _maxPathCount; i++)
        {
            _orbitLines[i] = Instantiate(_prefabOrbitLine);  
            _orbitLines[i].name = $"{_prefabOrbitLine.name}, index: {i}";
            LerpBetweenColors lerpBetweenColors = _orbitLines[i].GetComponent<LerpBetweenColors>();
            lerpBetweenColors.ColorToLerpFrom = _roygbivLerpColors[i % 7].Key;
            lerpBetweenColors.ColorToLerpTo = _roygbivLerpColors[i % 7].Value;

            Disc disc = _orbitLines[i].GetComponent<Disc>();
            disc.Radius = lastDiscRadius = _maxOrbitPathRadius - (_maxOrbitPathRadius-_minOrbitPathRadius)/_maxPathCount * i;
            
            //BUG: This will fail if the rings exceeds the number of permitted angles, could be fixed but for now this doesn't matter much.
            //BUG: Shuffling not producing random values currently.
            _radianStartEndPairsPossible.Shuffle();
            KeyValuePair<float, float> startEndRadPair = _radianStartEndPairsPossible[0];
            _radianStartEndPairsPossible.RemoveAt(0);
            disc.AngRadiansStart = startEndRadPair.Key;
            disc.AngRadiansEnd = startEndRadPair.Value;
            
            float enclosedSectionInDegrees = ((startEndRadPair.Value * Mathf.Rad2Deg) - (startEndRadPair.Key * Mathf.Rad2Deg)) % 360;
            //The opposite of the gap, so we can guarantee starting in the correct zone.
            oppositeMiddleOfLastMissingAngle = startEndRadPair.Key * Mathf.Rad2Deg + enclosedSectionInDegrees / 2;

            float progressToCenter = disc.Radius / (_maxOrbitPathRadius + _minOrbitPathRadius);
            float alpha = Mathf.Lerp(_maxOrbitTransparency, _minOrbitTransparency, Mathf.Clamp(progressToCenter, 0f, 1f));
            disc.Color = new Color(disc.Color.r, disc.Color.g, disc.Color.b, alpha/255f);
        }

        _player = Instantiate(_prefabPlayer);
        _playerDistance = _playerOffsetFromRadius + lastDiscRadius;
        _player.transform.position = new Vector3(_playerDistance, 0);
        _playerAngle = oppositeMiddleOfLastMissingAngle % 360;
        _player.transform.position = Quaternion.AngleAxis(_playerAngle, Vector3.forward) * _player.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if(!_gameMusicAudioSource.isPlaying && !_gameOver)
        {
            _gameMusicAudioSource.clip = _gameBGMLoopAudioClip;
            _gameMusicAudioSource.loop = true;
            _gameMusicAudioSource.Play();
        }        

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (!_animatorSettings.gameObject.activeSelf || _animatorSettings.GetCurrentAnimatorStateInfo(0).IsName("GearIconOnly"))
        {
            Time.timeScale=1;

            float orbitRadiusClosestToPlayer = Mathf.Infinity;
            float orbitSmallestDistanceToPlayer = Mathf.Infinity;
            float minUnaccessibleAngle = Mathf.Infinity;
            float maxUnaccessibleAngle = Mathf.Infinity;
            bool snapLocationDiscovered = false;
            
            Vector2 playerViewportPoint = Camera.main.WorldToViewportPoint(_player.transform.position);
            if (_playerDistance + _playerRadius < _sun.GetComponent<Disc>().Radius || !(playerViewportPoint.x > 0 && playerViewportPoint.x < 1 && playerViewportPoint.y > 0 && playerViewportPoint.y < 1))
            {
                _player.SetActive(false);
                _gameOver = true;
                _gameMusicAudioSource.volume *= .975f;
            }

            if (_scoreTMP.gameObject.activeSelf && Input.anyKeyDown)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            bool orbitsEnabled = false;
            for (int i = 0; i < _maxPathCount; i++)
            {
                Disc disc = _orbitLines[i].GetComponent<Disc>();
                //Move the orbit path toward the center
                disc.Radius -= _collapseSpeed * Time.deltaTime;
                //If the orbit path is inside the sun and no longer visible, set it outside the screen again.
                if (disc.Radius <= _minOrbitPathRadius)
                {
                    if (_gameOver)
                    {
                        _orbitLines[i].SetActive(false);
                        if(_destroyedRingCount < 7)
                        {
                            _gameSFXAudioSource.PlayOneShot(_gameScoreAudioClip[(_destroyedRingCount + _score) % 4]);
                        }                        
                        _destroyedRingCount++;
                    }
                    else
                    {
                        _gameSFXAudioSource.PlayOneShot(_gameScoreAudioClip[_score%4]);
                        _score++;
                        _scoreTMP.text = $"{_score}";
                    }
                    //Take into account the overshoot, since we want to try and maintain perfect spacing of the rings.
                    disc.Radius = (_minOrbitPathRadius - disc.Radius) + _maxOrbitPathRadius;
                    //Give them a new radius so we don't see the same patterns.
                    //TODO change this to randomly select from a copy of the list, removing from it when choosing, and adding the one we're giving up back to the list.
                    _radianStartEndPairsPossible.Shuffle();
                    KeyValuePair<float, float> startEndRadPair = _radianStartEndPairsPossible[0];
                    _radianStartEndPairsPossible.RemoveAt(0);
                    _radianStartEndPairsPossible.Add(new KeyValuePair<float, float>(disc.AngRadiansStart, disc.AngRadiansEnd));
                    disc.AngRadiansStart = startEndRadPair.Key;
                    disc.AngRadiansEnd = startEndRadPair.Value;
                }
                //Check if this is orbit is closer than ones we've checked.
                float currentOrbitDifferenceFromPlayer = disc.Radius - _playerDistance;
                float currentOrbitDistanceToPlayer = Mathf.Abs(currentOrbitDifferenceFromPlayer);
                //It is, so update how far it is, where it's angle center is, and what side of the player it's on.
                if (currentOrbitDistanceToPlayer < orbitSmallestDistanceToPlayer)
                {
                    float endInDegrees = (disc.AngRadiansEnd * Mathf.Rad2Deg) % 360;
                    float startInDegrees = (disc.AngRadiansStart * Mathf.Rad2Deg) % 360;
                    orbitRadiusClosestToPlayer = disc.Radius;
                    orbitSmallestDistanceToPlayer = currentOrbitDistanceToPlayer;
                    if (endInDegrees < startInDegrees)
                    {
                        minUnaccessibleAngle = endInDegrees;
                        maxUnaccessibleAngle = startInDegrees;
                    }
                    else
                    {
                        minUnaccessibleAngle = startInDegrees;
                        maxUnaccessibleAngle = endInDegrees;
                    }
                    if (!_playerMidJump)
                    {
                        if (currentOrbitDifferenceFromPlayer > 0)
                        {
                            _orbitFartherThanPlayerPreJump = true;
                        }
                        else
                        {
                            _orbitFartherThanPlayerPreJump = false;
                        }
                    }
                }

                //Update the transparency of the path to be more opaque as it gets close to the center.
                float progressToCenter = disc.Radius / (_maxOrbitPathRadius + _minOrbitPathRadius);
                float alpha = Mathf.Lerp(_maxOrbitTransparency, _minOrbitTransparency, Mathf.Clamp(progressToCenter, 0, 1));
                disc.Color = new Color(disc.Color.r, disc.Color.g, disc.Color.b, alpha / 255);

                if (_orbitLines[i].activeSelf)
                {
                    orbitsEnabled = true;
                }
            }

            if (!orbitsEnabled)
            {
                _sun.transform.localScale *= .97f;
            }
            if(_sun.transform.localScale.x < .00025f && !_scoreDisplayed)
            {
                _scoreTMP.gameObject.SetActive(true);
                _gameSFXAudioSource.PlayOneShot(_gameTitleAudioClip);
                _scoreDisplayed = true;
            }

            if (snapLocationDiscovered)
            {
                _playerMidJump = false;
            }

            //Rotate the player towards it's angle, then if it's not in motion to another ring check if it's on the inside or outside of the current ring and add or subtract distance to the closest ring.
            //If we are midjump, don't allow rotation, check each ring that is farther from our position, and if we are within snapping distance of the ring and not between it's start and end angle, we can snap to the inside of that ring.
            if (!_playerMidJump)
            {
                float oldAngle = _playerAngle;
                _playerAngle -= Input.GetAxis("Horizontal") * _playerSpeed * Time.deltaTime;

                if (_playerAngle < 0)
                {
                    _playerAngle += 360;
                }
                _playerAngle = _playerAngle % 360;
                if (_playerAngle >= minUnaccessibleAngle && _playerAngle <= maxUnaccessibleAngle)
                {
                    _playerAngle = oldAngle;
                }
                _playerDistance = _player.transform.position.magnitude - (_collapseSpeed * Time.deltaTime);
                if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2") || Input.GetButtonDown("Fire3") || Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel"))
                {
                    _playerMidJump = true;
                    _playerFoundLandingPad = false;
                    if (_orbitFartherThanPlayerPreJump)
                    {
                        _playerOrbitPathLandingPadRadius = Mathf.NegativeInfinity;
                    }
                    else
                    {
                        _playerOrbitPathLandingPadRadius = Mathf.Infinity;
                    }
                    for (int i = 0; i < _maxPathCount; i++)
                    {
                        Disc disc = _orbitLines[i].GetComponent<Disc>();
                        float endInDegrees = (disc.AngRadiansEnd * Mathf.Rad2Deg) % 360;
                        float startInDegrees = (disc.AngRadiansStart * Mathf.Rad2Deg) % 360;
                        if (endInDegrees < startInDegrees)
                        {
                            minUnaccessibleAngle = endInDegrees;
                            maxUnaccessibleAngle = startInDegrees;
                        }
                        else
                        {
                            minUnaccessibleAngle = startInDegrees;
                            maxUnaccessibleAngle = endInDegrees;
                        }
                        //Checking inward
                        if (_orbitFartherThanPlayerPreJump)
                        {
                            if (disc.Radius <= _playerDistance && disc.Radius >= _playerOrbitPathLandingPadRadius && !(_playerAngle >= minUnaccessibleAngle && _playerAngle <= maxUnaccessibleAngle))
                            {
                                _playerFoundLandingPad = true;
                                _playerOrbitPathLandingPadRadius = disc.Radius;
                            }
                        }
                        else//CheckingOutward
                        {
                            if (disc.Radius >= _playerDistance && disc.Radius <= _playerOrbitPathLandingPadRadius && !(_playerAngle >= minUnaccessibleAngle && _playerAngle <= maxUnaccessibleAngle))
                            {
                                _playerFoundLandingPad = true;
                                _playerOrbitPathLandingPadRadius = disc.Radius;
                            }
                        }
                    }
                    if (_orbitFartherThanPlayerPreJump && !_playerFoundLandingPad)
                    {
                        _playerFoundLandingPad = true;
                        _playerOrbitPathLandingPadRadius = 0f;
                    }
                }
            }
            else
            {
                //If the ring we jump from is farther than us, then we jump inwards, otherwise we jump outwards.
                if (_orbitFartherThanPlayerPreJump)
                {
                    _playerDistance = _player.transform.position.magnitude - (_playerJumpSpeed * Time.deltaTime);
                }
                else
                {
                    _playerDistance = _player.transform.position.magnitude + (_playerJumpSpeed * Time.deltaTime);
                }

                if (_playerFoundLandingPad)
                {
                    _playerOrbitPathLandingPadRadius -= _collapseSpeed * Time.deltaTime;
                    if (_orbitFartherThanPlayerPreJump)
                    {
                        if (Mathf.Approximately(_playerDistance, _playerOrbitPathLandingPadRadius+_playerSnapDistance) || _playerDistance < _playerOrbitPathLandingPadRadius)
                        {
                            _playerMidJump = false;
                            _playerDistance = _playerOrbitPathLandingPadRadius + _playerSnapDistance;
                        }
                    }
                    else
                    {
                        if (Mathf.Approximately(_playerDistance, _playerOrbitPathLandingPadRadius - _playerSnapDistance) || _playerDistance > _playerOrbitPathLandingPadRadius)
                        {
                            _playerMidJump = false;
                            _playerDistance = _playerOrbitPathLandingPadRadius - _playerSnapDistance;
                        }
                    }
                }
            }

            _player.transform.position = new Vector3(_playerDistance, 0);
            _player.transform.position = Quaternion.AngleAxis(_playerAngle, Vector3.forward) * _player.transform.position;
        }
        else
        {
            Time.timeScale=0;
        }
    }

    private IEnumerator IncreaseCollapseSpeed()
    {
        for (int i = 0; i < 30; i++)
        {
            yield return new WaitForSeconds(1.5f);
            _collapseSpeed *= (1 + _speedPercentIncrease);
            _playerJumpSpeed *= (1 + _speedPercentIncrease*3);
            _playerSpeed *= (1 + _speedPercentIncrease/2);
            _maxOrbitTransparency *= (1 + _speedPercentIncrease);
            _maxOrbitTransparency = Mathf.Clamp(_maxOrbitTransparency, 0, 255*3);
            _minOrbitTransparency *= (1 + _speedPercentIncrease);
            _minOrbitTransparency = Mathf.Clamp(_minOrbitTransparency, 0, 255 * 1.5f);
            _gameMusicAudioSource.pitch *= 1.005f;
            if (_gameOver) { break ;}
        }
        while(!_gameOver)
        {
            yield return new WaitForSeconds(3.5f);
            _collapseSpeed *= (1 + _speedPercentIncrease);
            _playerJumpSpeed *= (1 + _speedPercentIncrease*3);
            _playerSpeed *= (1 + _speedPercentIncrease/2);
            _maxOrbitTransparency *= (1 + _speedPercentIncrease);
            _maxOrbitTransparency = Mathf.Clamp(_maxOrbitTransparency, 0, 255 * 3);
            _minOrbitTransparency *= (1 + _speedPercentIncrease);
            _minOrbitTransparency = Mathf.Clamp(_minOrbitTransparency, 0, 255 * 1.5f);
            _gameMusicAudioSource.pitch *= 1.0075f;
        }
        while (true)
        {
            yield return new WaitForSeconds(.03f);
            _collapseSpeed *= (1 + _speedPercentIncrease*1.3f);
            _playerJumpSpeed *= (1 + _speedPercentIncrease * 3);
            _playerSpeed *= (1 + _speedPercentIncrease / 2);
            _maxOrbitTransparency *= (1 + _speedPercentIncrease);
            _maxOrbitTransparency = Mathf.Clamp(_maxOrbitTransparency, 0, 255 * 3);
            _minOrbitTransparency *= (1 + _speedPercentIncrease);
            _minOrbitTransparency = Mathf.Clamp(_minOrbitTransparency, 0, 255 * 1.5f);
            _gameMusicAudioSource.pitch *= 1.005f;
        }
    }

}
