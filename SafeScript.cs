using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SafeScript : MonoBehaviour
{
    private bool isInRange;
    private bool isOpend;
    public Text instreractionsPanel;

    // Start is called before the first frame update
    void Start()
    {
        isInRange = false;
        isOpend = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInRange)
        {
            if (Input.GetKeyDown(KeyCode.X))
                SceneManager.LoadScene("KeyPad_Scene", LoadSceneMode.Additive);
        }

    }


    public void OnTriggerEnter(Collider other)
    {
        isOpend = other.GetComponent<PlayerHealth>().HasTheKey();

        if (!isOpend)
        {
            instreractionsPanel.text = "Open safe (x)";
            isInRange = true;

        }
    }
    public void OnTriggerExit(Collider other)
    {
        isInRange = false;
        instreractionsPanel.text = "";
    }
}
