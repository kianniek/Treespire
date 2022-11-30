using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Programs/Doom Game")]
public class DoomGame : Program
{
    internal override bool StartUp()
    {
        // call base
        if (!base.StartUp())
            return false;

        // immediately shut down, 
        // since this program 'isn't working'
        // LOL what kinda computer can't run DOOM :')
        ShutDown();

        // open error window and display message
        GameManager.instance.errorWindow.Open();
        GameManager.instance.errorWindow.SetText(GameManager.instance.programCantRunError);
        GameManager.instance.errorWindow.SetButtonDelegates(null);

        // succesfully(-ish) started 
        return true;
    }
}
