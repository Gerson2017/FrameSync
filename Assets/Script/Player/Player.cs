using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public uint guid;
    public void Move(Fixed64Vector2 dir)
    {
        transform.position += new Vector3((float)dir.x/3, 0, (float)dir.y/3);
    }
    public void OnOpts(Opts opts)
    {
        switch (opts.operation)
        {
            case Operation.Joystick:
                Move(new Fixed64Vector2(opts.param[0], opts.param[1]));
                break;
            default:
                break;
        }
    }
}
