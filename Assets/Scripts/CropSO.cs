using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New CropSO", menuName ="CropSimulator/New CropSO")]
public class CropSO : ScriptableObject
{
    public string cropName;
    public int stage = 0;
    public Sprite[] cropsSprites;

    
}
