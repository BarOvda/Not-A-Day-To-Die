using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCrate : MonoBehaviour
{
    public enum TypeOfAmmo
    {
        rifle,m16
    }
    public TypeOfAmmo typeOfAmmo;
    private bool isTaken;
    private bool isInRange;
    public Text instreractionsPanel;
    public GameObject rifle;
    public GameObject m16;
    public AudioClip takeAmmoAudio;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        isTaken = false;
        isInRange = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInRange&&!isTaken)
        {
            if (typeOfAmmo == TypeOfAmmo.m16)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                  //  m16.GetComponent<GunScript>().addAmmo(10);
                    player.GetComponent<GunInventory>().addAmmoTom16(10);
                    isTaken = true;
                    instreractionsPanel.text = "";

                    AudioSource.PlayClipAtPoint(takeAmmoAudio, player.transform.position);
                }
            }
            else if (typeOfAmmo == TypeOfAmmo.rifle)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    
                   // rifle.GetComponent<GunScript>().addAmmo(10);
                    player.GetComponent<GunInventory>().addAmmoToRifle(10);
                    isTaken = true;
                    instreractionsPanel.text = "";
                    AudioSource.PlayClipAtPoint(takeAmmoAudio, player.transform.position);
                }
            }
        }
     
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!isTaken)
        {
            instreractionsPanel.text = "pick ammo (x)";
            isInRange = true;
        }
    }
    public void OnTriggerExit(Collider other)
    {
        isInRange = false;
        instreractionsPanel.text = "";
    }
}
