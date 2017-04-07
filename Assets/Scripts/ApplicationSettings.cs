using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine;

public class ApplicationSettings : MonoBehaviour {
    public static List<Vector2>[] neighbours;
    //Save difficulty as an int representing how many enemies should spawn and how big the map should be;
    public static int difficulty = 2;
}
