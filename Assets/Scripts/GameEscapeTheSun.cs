using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

//TODO: Consider using only a subset of angles that can be the openings and randomly selecting from them.
//TODO: Jumping
    //Move away at the opposite of collapse speed? Check when we hit a ring. Should only allow player to go through holes. Don't even allow flipping by choice, only as a consequence of the direction we jumped?
//TODO: Flipping Sides?


public class GameEscapeTheSun : MonoBehaviour
{
    [SerializeField] private GameObject _prefabSun, _prefabOrbitLine, _prefabPlayer;
    private GameObject _Sun;
    private GameObject[] _orbitLines;
    private GameObject _player;

    //Ring orbit size from center of sun, max representing offscreen, min representing inside smallest sun.
    [SerializeField] private float _maxOrbitPathRadius = 12.5f, _minOrbitPathRadius = .95f;
    //The orbit paths will start at min transparency and lerp to max transparency as they get closer.
    private float _minOrbitTransparency = 0, _maxOrbitTransparency = 128;
    [SerializeField] private int _maxPathCount = 7;
    private List<KeyValuePair<float, float>> _radianStartEndPairsPossible = new List<KeyValuePair<float, float>>() {new KeyValuePair<float, float>(0,                   330 * Mathf.Deg2Rad), 
                                                                                                                    new KeyValuePair<float, float>(30  * Mathf.Deg2Rad, 360 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(60  * Mathf.Deg2Rad, 390 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(90  * Mathf.Deg2Rad, 420 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(120 * Mathf.Deg2Rad, 450 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(150 * Mathf.Deg2Rad, 480 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(180 * Mathf.Deg2Rad, 510 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(210 * Mathf.Deg2Rad, 540 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(240 * Mathf.Deg2Rad, 570 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(270 * Mathf.Deg2Rad, 600 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(300 * Mathf.Deg2Rad, 630 * Mathf.Deg2Rad),
                                                                                                                    new KeyValuePair<float, float>(330 * Mathf.Deg2Rad, 660 * Mathf.Deg2Rad)};
    

    [SerializeField] private float _collapseSpeed = .08f;

    private float _playerSpeed = 360/1.5f, _playerOffsetFromRadius = .3f, _playerRadius = .2f, _playerAngle, _playerDistance, _playerSnapDistance = .2f;
    private bool _playerOnOutside = true, _playerMidJump = false, _orbitFartherThanPlayerPreJump = false;


    [SerializeField] private Animator _animatorSettings;

    private void Start()
    {
        _Sun = Instantiate(_prefabSun);
        _orbitLines = new GameObject[_maxPathCount];
        float lastDiscRadius = 0;
        float oppositeMiddleOfLastMissingAngle = 0;
        for (int i = 0; i < _maxPathCount; i++)
        {
            _orbitLines[i] = Instantiate(_prefabOrbitLine);  
            _orbitLines[i].name = $"{_prefabOrbitLine.name}, index: {i}";
            Disc disc = _orbitLines[i].GetComponent<Disc>();
            disc.Radius = lastDiscRadius = _maxOrbitPathRadius - (_maxOrbitPathRadius-_minOrbitPathRadius)/_maxPathCount * i;
            KeyValuePair<float, float> startEndRadPair = _radianStartEndPairsPossible[i];
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
        if (_animatorSettings.GetCurrentAnimatorStateInfo(0).IsName("GearIconOnly"))
        {
            Time.timeScale=1;

            float orbitRadiusClosestToPlayer = Mathf.Infinity;
            float orbitSmallestDistanceToPlayer = Mathf.Infinity;
            float minUnaccessibleAngle = Mathf.Infinity;
            float maxUnaccessibleAngle = Mathf.Infinity;
            bool withinSnappingDistance = false;


            for (int i = 0; i < _maxPathCount; i++)
            {
                Disc disc = _orbitLines[i].GetComponent<Disc>();
                //Move the orbit path toward the center
                disc.Radius -= _collapseSpeed * Time.deltaTime;
                //If the orbit path is inside the sun and no longer visible, set it outside the screen again.
                if (disc.Radius <= _minOrbitPathRadius)
                {
                    //Take into account the overshoot, since we want to try and maintain perfect spacing of the rings.
                    disc.Radius = (_minOrbitPathRadius - disc.Radius) + _maxOrbitPathRadius;
                    //Give them a new radius so we don't see the same patterns.
                    //TODO change this to randomly select from a copy of the list, removing from it when choosing, and adding the one we're giving up back to the list.
                    KeyValuePair<float, float> startEndRadPair = _radianStartEndPairsPossible[i];
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
                    if (_playerMidJump && !(_playerAngle > minUnaccessibleAngle && _playerAngle < maxUnaccessibleAngle) && currentOrbitDistanceToPlayer <= _playerSnapDistance)
                    {
                        if (_orbitFartherThanPlayerPreJump && currentOrbitDifferenceFromPlayer <= 0)
                        {
                            _playerMidJump = false;
                            _playerDistance = orbitRadiusClosestToPlayer + _playerOffsetFromRadius;
                        }
                        else if(!_orbitFartherThanPlayerPreJump && currentOrbitDifferenceFromPlayer >= 0)
                        {
                            _playerMidJump = false;
                            _playerDistance = orbitRadiusClosestToPlayer - _playerOffsetFromRadius;
                        }
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
            }

            //Rotate the player towards it's angle, then if it's not in motion to another ring check if it's on the inside or outside of the current ring and add or subtract distance to the closest ring.
            //If we are midjump, don't allow rotation, check each ring that is farther from our position, and if we are within snapping distance of the ring and not between it's start and end angle, we can snap to the inside of that ring.
            if (!_playerMidJump)
            {
                float oldAngle = _playerAngle;
                if (Input.GetKey(KeyCode.A))
                {
                    _playerAngle += _playerSpeed * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    _playerAngle -= _playerSpeed * Time.deltaTime;
                }
                if (_playerAngle < 0)
                {
                    _playerAngle += 360;
                }
                _playerAngle = _playerAngle % 360;
                if (_playerAngle > minUnaccessibleAngle && _playerAngle < maxUnaccessibleAngle)
                {
                    _playerAngle = oldAngle;
                }
                _playerDistance = _player.transform.position.magnitude - (_collapseSpeed * Time.deltaTime);
                if (Input.GetKey(KeyCode.Space))
                {
                    _playerMidJump = true;
                    Debug.Log($"Jump Start.\n");
                }
            }
            else
            {
                //If the ring we jump from is farther than us, then we jump inwards, otherwise we jump outwards.
                if (_orbitFartherThanPlayerPreJump)
                {
                    _playerDistance = _player.transform.position.magnitude - (2 * _collapseSpeed * Time.deltaTime);
                }
                else
                {
                    _playerDistance = _player.transform.position.magnitude + (_collapseSpeed * Time.deltaTime);
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
}
