using UnityEngine;
using System.Collections;

public class ChangeSceneScript : MonoBehaviour
{

    public void ChangeLevel(int level)
    {
        Application.LoadLevel(level);
    }

}
