using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class LerpBetweenColors : MonoBehaviour
{
    [SerializeField] private Color _initial, _final;
    private Color _current;
    [SerializeField] private float _speed = .025f;
    [SerializeField] private float _time = 0;
    private bool toFinal = true;

    private void Start()
    {
        _current = _initial;
    }

    // Update is called once per frame
    void Update()
    {
        if(_time < 0)
        {
            toFinal = true;
            _time = 0;
        }
        else if(_time > 1)
        {
            toFinal = false;
            _time = 1;
        }

        if (toFinal)
        {
            _time += _speed * Time.deltaTime;
        }
        else
        {
            _time -= _speed * Time.deltaTime;
        }
        _current = Color.Lerp(_initial, _final, _time);

        GetComponent<Disc>().Color = _current;
    }
}
