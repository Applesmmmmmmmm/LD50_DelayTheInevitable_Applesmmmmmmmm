using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class LerpBetweenColors : MonoBehaviour
{
    [SerializeField] public Color ColorToLerpFrom, ColorToLerpTo;
    [SerializeField] private bool _ignoreAlpha = true;
    private Color _current;
    [SerializeField] private float _speed = .025f;
    [SerializeField] private float _time = 0;
    private bool toFinal = true;

    private void Start()
    {
        _current = ColorToLerpFrom;
    }

    // Update is called once per frame
    void Update()
    {
        Disc d = GetComponent<Disc>();
        
        if (_time < 0)
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
        
        _current = Color.Lerp(ColorToLerpFrom, ColorToLerpTo, _time); 
        
        if (_ignoreAlpha)
        {
            _current.a = d.Color.a;
        }
        GetComponent<Disc>().Color = _current;
    }
}
