using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Crop : MonoBehaviour
{
    public CropSO crop;
    private SpriteRenderer _renderer;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = crop.cropsSprites[crop.stage];
    }
    public void Envolve()
    {
        if (crop.stage < 5)
        {
            crop.stage++;
            _renderer.sprite = crop.cropsSprites[crop.stage];
        }
    }

    public void CropTime()
    {
        if (crop.stage == 5)
        {
            Debug.Log("Its Science, Bitch!");
        }
    }

    void Update()
    {
        
    }
}
